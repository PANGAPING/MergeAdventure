using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 订单点击次数估算（最少期望点击数）
/// - 把订单需求折算为各链的 rootCount 需求
/// - 每个生成器一次点击，对各链提供“期望rootCount贡献”
/// - 求整数点击次数向量 x，使得 A*x >= need，并最小化 sum(x)
/// 
/// 适用：
/// - 一个生成器多产出（概率不同）
/// - 多生成器交叉产出（会自动利用）
/// - 订单有多个需求
/// </summary>
public static class OrderClickEstimator
{
    public struct Result
    {
        public bool ok;
        public string error;

        public int minExpectedClicks;
        public int[] clicksPerGenerator; // 与传入 gens 顺序一致
        public Dictionary<int, double> needByChain; // chainId -> rootCount
        public List<Dictionary<int, float>> yieldByGen; // genIndex -> (chainId -> expectedRootCountPerClick)
    }

    /// <summary>
    /// 计算最少期望点击数（整数）
    /// </summary>
    public static Result EstimateMinExpectedClicks(OrderModel order, List<GeneratorConfig> gens)
    {
        var res = new Result
        {
            ok = false,
            error = null,
            minExpectedClicks = 0,
            clicksPerGenerator = null,
            needByChain = null,
            yieldByGen = null
        };

        if (order == null) { res.error = "order is null"; return res; }
        if (gens == null || gens.Count == 0) { res.error = "gens is empty"; return res; }

        // 1) 订单需求 -> chain rootCount 需求
        Dictionary<int, double> need = BuildNeedByChain(order);
        if (need.Count == 0) { res.error = "order needs empty"; return res; }

        // 2) 生成器一次点击对各chain的期望贡献
        List<Dictionary<int, float>> yields = BuildYieldByGenerator(gens, need);

        // 3) 可行性检查：每条链至少有一个生成器 yield>0
        foreach (var kv in need)
        {
            int chainId = kv.Key;
            bool ok = false;
            for (int i = 0; i < gens.Count; i++)
            {
                if (yields[i].TryGetValue(chainId, out float v) && v > 1e-6f) { ok = true; break; }
            }
            if (!ok)
            {
                res.error = $"Chain {chainId} cannot be produced by any generator.";
                return res;
            }
        }

        // 4) 求解整数最小 sum(x)：使用“贪心上界 + DFS剪枝”
        var solver = new MinIntSolver(gens.Count, need, yields);
        var solve = solver.Solve();

        if (!solve.ok)
        {
            res.error = solve.error;
            return res;
        }

        res.ok = true;
        res.minExpectedClicks = solve.bestTotalClicks;
        res.clicksPerGenerator = solve.bestAlloc;
        res.needByChain = need;
        res.yieldByGen = yields;
        return res;
    }

    // =========================
    // Step 1: Need
    // =========================
    private static Dictionary<int, double> BuildNeedByChain(OrderModel order)
    {
        var need = new Dictionary<int, double>();

        int[] ids = order.NeedItemId;
        int[] nums = order.NeedItemNum;
        if (ids == null || nums == null || ids.Length != nums.Length) return need;

        for (int i = 0; i < ids.Length; i++)
        {
            int itemId = ids[i];
            int num = nums[i];
            if (itemId <= 0 || num <= 0) continue;

            int chainId = ChainHelper.GetChainRoot(itemId);
            int rootCount = ChainHelper.ConvertItemIntoRootCount(itemId); // 2^(level-1)
            double add = (double)num * rootCount;

            if (need.TryGetValue(chainId, out double old))
                need[chainId] = old + add;
            else
                need[chainId] = add;
        }

        // ✅ 可选：如果你希望考虑“库存/棋盘已有”，这里减掉 stock
        // for each chainId: need = max(0, need - ChainHelper.GetStock(chainId))
        // 但注意你们 ChainHelper.GetStock 当前 build 逻辑没被调用，这里先不强行依赖。

        return need;
    }

    // =========================
    // Step 2: Yield per click
    // =========================
    private static List<Dictionary<int, float>> BuildYieldByGenerator(
        List<GeneratorConfig> gens,
        Dictionary<int, double> needByChain
    )
    {
        var yields = new List<Dictionary<int, float>>(gens.Count);

        // 只关心订单里出现的链
        var chains = new HashSet<int>(needByChain.Keys);

        for (int gi = 0; gi < gens.Count; gi++)
        {
            var gen = gens[gi];
            var dict = new Dictionary<int, float>();

            // init 0
            foreach (var c in chains) dict[c] = 0f;

            // ratios sum
            float sum = 0f;
            if (gen.DropItemRatios != null)
            {
                for (int i = 0; i < gen.DropItemRatios.Length; i++)
                    sum += Mathf.Max(0f, gen.DropItemRatios[i]);
            }
            if (sum <= 1e-6f)
            {
                yields.Add(dict);
                continue;
            }

            // lucky model
            float luckyRatio = Mathf.Clamp01(gen.LuckyRatio);
            float[] luckyDist = gen.LuckDropItemRatio;
            float luckySum = 0f;
            if (luckyDist != null && luckyDist.Length > 0)
            {
                for (int i = 0; i < luckyDist.Length; i++)
                    luckySum += Mathf.Max(0f, luckyDist[i]);
            }

            // 计算：一次点击的期望 rootCount
            // - 非Lucky：贡献=1（1级rootCount）
            // - Lucky：贡献=E[2^(L-1)]，L由 LuckDropItemRatio 决定（第N项对应 1+N+? 见下注）
            //
            // 你给的定义：LuckDropItemRatio 第N项对应 1+N级（N从0开始对应2级）
            // 所以 level = 1 + (index+1)
            float expectedRootCountWhenLucky = 2f; // 默认2级 => 2^(2-1)=2
            if (luckyDist != null && luckyDist.Length > 0 && luckySum > 1e-6f)
            {
                float e = 0f;
                for (int k = 0; k < luckyDist.Length; k++)
                {
                    float pk = Mathf.Max(0f, luckyDist[k]) / luckySum;
                    int level = 1 + (k + 1);          // 2,3,4...
                    int rc = 1 << (level - 1);         // 2^(level-1)
                    e += pk * rc;
                }
                expectedRootCountWhenLucky = e;
            }

            for (int i = 0; i < gen.DropItemIds.Length; i++)
            {
                int dropId = gen.DropItemIds[i];
                float w = Mathf.Max(0f, gen.DropItemRatios[i]);
                if (w <= 0f) continue;

                int chainId = ChainHelper.GetChainRoot(dropId);
                if (!chains.Contains(chainId)) continue;

                float p = w / sum;

                // 一次点击中，选中该drop链时的期望贡献
                float expectedRootCount =
                    (1f - luckyRatio) * 1f +
                    luckyRatio * expectedRootCountWhenLucky;

                dict[chainId] += p * expectedRootCount;
            }

            yields.Add(dict);
        }

        return yields;
    }

    // =========================
    // Integer solver
    // =========================
    private sealed class MinIntSolver
    {
        public struct SolveResult
        {
            public bool ok;
            public string error;
            public int bestTotalClicks;
            public int[] bestAlloc;
        }

        private readonly int _gCount;
        private readonly Dictionary<int, double> _need;
        private readonly List<Dictionary<int, float>> _yield;
        private readonly List<int> _chains;

        private int _best = int.MaxValue;
        private int[] _bestAlloc;

        // chain -> bestYield among gens
        private readonly Dictionary<int, float> _bestYield = new();

        public MinIntSolver(int gCount, Dictionary<int, double> need, List<Dictionary<int, float>> yield)
        {
            _gCount = gCount;
            _need = need;
            _yield = yield;
            _chains = new List<int>(need.Keys);

            _bestAlloc = new int[_gCount];

            foreach (var c in _chains)
            {
                float best = 0f;
                for (int g = 0; g < _gCount; g++)
                {
                    float y = _yield[g][c];
                    if (y > best) best = y;
                }
                _bestYield[c] = best;
            }
        }

        public SolveResult Solve()
        {
            // 贪心先找上界
            var greedy = Greedy();
            if (!greedy.ok)
                return new SolveResult { ok = false, error = greedy.error };

            _best = greedy.bestTotalClicks;
            Array.Copy(greedy.bestAlloc, _bestAlloc, _bestAlloc.Length);

            // DFS 剪枝搜索最优整数解
            var alloc = new int[_gCount];
            var remain = new Dictionary<int, double>(_need);
            Dfs(0, 0, alloc, remain);

            return new SolveResult { ok = true, bestTotalClicks = _best, bestAlloc = (int[])_bestAlloc.Clone() };
        }

        private void Dfs(int gi, int used, int[] alloc, Dictionary<int, double> remain)
        {
            if (used >= _best) return;

            int lb = LowerBound(remain);
            if (lb == int.MaxValue) return;
            if (used + lb >= _best) return;

            if (gi == _gCount - 1)
            {
                int last = MinClicksSingleGen(remain, gi);
                if (last == int.MaxValue) return;

                int total = used + last;
                if (total < _best)
                {
                    alloc[gi] = last;
                    _best = total;
                    Array.Copy(alloc, _bestAlloc, _gCount);
                    alloc[gi] = 0;
                }
                return;
            }

            int maxK = _best - used - 1;
            if (maxK < 0) return;

            // 一个更紧的上限：只靠本gen填满剩余的“最难链”所需点击
            int ub = UpperBoundReasonable(remain, gi);
            maxK = Mathf.Min(maxK, ub);

            for (int k = 0; k <= maxK; k++)
            {
                if (k > 0)
                {
                    Apply(remain, gi, k);
                    alloc[gi] += k;
                }

                Dfs(gi + 1, used + k, alloc, remain);

                if (k > 0)
                {
                    Undo(remain, gi, k);
                    alloc[gi] -= k;
                }
            }
        }

        private int LowerBound(Dictionary<int, double> remain)
        {
            int lb = 0;
            foreach (var c in _chains)
            {
                double r = remain[c];
                if (r <= 1e-9) continue;
                float best = _bestYield[c];
                if (best <= 1e-6f) return int.MaxValue;
                lb = Math.Max(lb, (int)Math.Ceiling(r / best));
            }
            return lb;
        }

        private int MinClicksSingleGen(Dictionary<int, double> remain, int gi)
        {
            int needClicks = 0;
            foreach (var c in _chains)
            {
                double r = remain[c];
                if (r <= 1e-9) continue;
                float y = _yield[gi][c];
                if (y <= 1e-6f) return int.MaxValue;
                needClicks = Math.Max(needClicks, (int)Math.Ceiling(r / y));
            }
            return needClicks;
        }

        private int UpperBoundReasonable(Dictionary<int, double> remain, int gi)
        {
            int ub = 0;
            foreach (var c in _chains)
            {
                double r = remain[c];
                if (r <= 1e-9) continue;
                float y = _yield[gi][c];
                if (y <= 1e-6f) continue;
                ub = Math.Max(ub, (int)Math.Ceiling(r / y));
            }
            return Math.Min(ub + 3, _best); // +3 余量防止错过交叉最优
        }

        private void Apply(Dictionary<int, double> remain, int gi, int k)
        {
            foreach (var c in _chains)
            {
                double r = remain[c];
                if (r <= 0) continue;
                float y = _yield[gi][c];
                if (y <= 0) continue;
                remain[c] = Math.Max(0, r - y * k);
            }
        }

        private void Undo(Dictionary<int, double> remain, int gi, int k)
        {
            // 近似回滚（不会漏解，只是剪枝弱一点）
            foreach (var c in _chains)
            {
                float y = _yield[gi][c];
                if (y <= 0) continue;
                remain[c] += y * k;
            }
        }

        private SolveResult Greedy()
        {
            var remain = new Dictionary<int, double>(_need);
            var alloc = new int[_gCount];

            int clicks = 0;
            int safety = 200000;

            while (safety-- > 0)
            {
                if (AllCovered(remain)) break;

                int bestG = -1;
                double bestScore = 0;

                for (int g = 0; g < _gCount; g++)
                {
                    double score = 0;
                    foreach (var c in _chains)
                    {
                        double r = remain[c];
                        if (r <= 1e-9) continue;
                        float y = _yield[g][c];
                        score += Math.Min(r, y);
                    }
                    if (score > bestScore + 1e-9)
                    {
                        bestScore = score;
                        bestG = g;
                    }
                }

                if (bestG < 0 || bestScore <= 1e-9)
                    return new SolveResult { ok = false, error = "Greedy cannot progress (yield=0?)" };

                alloc[bestG]++;
                clicks++;
                Apply(remain, bestG, 1);

                if (clicks > 50000)
                    return new SolveResult { ok = false, error = "Greedy exceeded 50000 clicks, check configs." };
            }

            if (!AllCovered(remain))
                return new SolveResult { ok = false, error = "Greedy failed to cover needs." };

            return new SolveResult { ok = true, bestTotalClicks = clicks, bestAlloc = alloc };
        }

        private bool AllCovered(Dictionary<int, double> remain)
        {
            foreach (var c in _chains)
                if (remain[c] > 1e-6) return false;
            return true;
        }
    }
}
