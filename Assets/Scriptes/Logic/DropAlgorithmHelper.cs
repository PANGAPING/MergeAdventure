using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class DropAlgorithmHelper
{

    public static float GetTreeDropOfItem(TreeConfig treeConfig, int targetId) {
        float lv1Count = 0;

        int chainId = ChainHelper.GetChainRoot(targetId);
        int mergeNeedRootCount = ChainHelper.ConvertItemIntoRootCount(targetId);

        for (int i = 0; i < treeConfig.DropItemIds.Length; i++) { 
            int itemId = treeConfig.DropItemIds[i];
            float itemRatio = treeConfig.DropItemRatios[i];
            if (itemId == chainId) { 
                lv1Count += itemRatio * treeConfig.DropItemCount;
            }
        }

        return lv1Count / mergeNeedRootCount;
    }

    public static Dictionary<int, int> GetGeneratorDropResult(Generator generator,out bool luck) { 
        Dictionary<int, int> result = new Dictionary<int, int>();
        luck = false;

        GeneratorConfig generatorConfig = ConfigSystem.GetGeneratorConfig(generator.Model.ItemConfigID);

        int[] itemIds = generatorConfig.DropItemIds;
        float[] itemWeight = generatorConfig.DropItemRatios;

        // 权重总和
        float totalWeight = 0f;
        foreach (var w in itemWeight)
        {
            totalWeight += w;
        }

        float rand = Random.value * totalWeight;
        float current = 0f;

        int chooseItemId = -1;
        // 根据权重挑选物品
        for (int j = 0; j < itemIds.Length; j++)
        {
            current += itemWeight[j];
            if (rand <= current)
            {
                chooseItemId = itemIds[j];
                break;
            }
        }

        if (chooseItemId < 0) {
            return result;
        }

        //Luck逻辑
        float luckyRatio = generatorConfig.LuckyRatio;
        float[] luckDropRatioWeight = generatorConfig.LuckDropItemRatio;

        if (luckyRatio > Random.value) {
            luck = true;
            chooseItemId += 1; 
        }

        result[chooseItemId] = 1;
        return result;
    }

    public static Dictionary<int, int> GetDropResult(
       int[] itemIds,
       float[] weights,
       int totalDropCount)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();

        // 初始化所有的计数
        foreach (int id in itemIds)
        {
            result[id] = 0;
        }

        // 权重总和
        float totalWeight = 0f;
        foreach (var w in weights)
        {
            totalWeight += w;
        }

        // 多次掉落
        for (int i = 0; i < totalDropCount; i++)
        {
            float rand = Random.value * totalWeight;
            float current = 0f;

            // 根据权重挑选物品
            for (int j = 0; j < itemIds.Length; j++)
            {
                current += weights[j];
                if (rand <= current)
                {
                    result[itemIds[j]]++;
                    break;
                }
            }
        }

        return result;
    }

 
    /// <summary>
    /// 获取某一次掉落的物品数量
    /// 目标：所有掉落次数结束后，各物品总数严格等于 ratios * totalCount（整数分配且总和=totalCount），
    ///       且每次掉落总数严格等于 dropWeights * totalCount（整数分配且总和=totalCount）。
    /// </summary>
    public static Dictionary<int, int> GetDropResultOnce(
        int totalCount,
        int[] itemIds,
        float[] ratios,
        float[] dropWeights,
        int dropIndex)
    {
        var result = new Dictionary<int, int>();

        if (itemIds == null || ratios == null || dropWeights == null ||
            itemIds.Length == 0 || ratios.Length == 0 || dropWeights.Length == 0)
            return result;

        int itemN = Mathf.Min(itemIds.Length, ratios.Length);

        // 初始化返回结构
        for (int i = 0; i < itemN; i++)
            result[itemIds[i]] = 0;

        if (totalCount <= 0) return result;
        if (dropIndex < 0 || dropIndex >= dropWeights.Length) return result;

        // 1) 全局：每个物品总量（列和），严格 sum = totalCount
        int[] totalPerItem = AllocateIntegerByWeights(totalCount, ratios, itemN);

        // 2) 全局：每次掉落总量（行和），严格 sum = totalCount
        int dropK = dropWeights.Length;
        int[] dropPerTime = AllocateIntegerByWeights(totalCount, dropWeights, dropK);

        // 3) 从 0 次开始模拟分配到 dropIndex
        int[] remaining = (int[])totalPerItem.Clone();

        for (int t = 0; t <= dropIndex; t++)
        {
            int need = dropPerTime[t];
            int[] takeThisTime = AllocateOnceFromRemaining(remaining, need);

            if (t == dropIndex)
            {
                // 返回这一轮
                for (int i = 0; i < itemN; i++)
                    result[itemIds[i]] = takeThisTime[i];
                return result;
            }
        }

        return result;
    }

    /// <summary>
    /// 最大余数法：按权重分配整数，保证总和=total。
    /// </summary>
    private static int[] AllocateIntegerByWeights(int total, float[] weights, int n)
    {
        int[] outArr = new int[n];
        if (total <= 0 || n <= 0) return outArr;

        double sum = 0;
        for (int i = 0; i < n; i++) sum += Math.Max(0.0, weights[i]);

        // 如果全是 0，均分
        if (sum <= 1e-12)
        {
            int baseEach = total / n;
            int rem = total - baseEach * n;
            for (int i = 0; i < n; i++) outArr[i] = baseEach;
            for (int i = 0; i < rem; i++) outArr[i]++;
            return outArr;
        }

        int allocated = 0;
        double[] frac = new double[n];

        for (int i = 0; i < n; i++)
        {
            double w = Math.Max(0.0, weights[i]);
            double exact = total * (w / sum);
            int flo = (int)Math.Floor(exact + 1e-12);
            outArr[i] = flo;
            allocated += flo;
            frac[i] = exact - flo;
        }

        int left = total - allocated;
        if (left <= 0) return outArr;

        // 按小数部分从大到小补齐（同分按 index）
        int[] idx = new int[n];
        for (int i = 0; i < n; i++) idx[i] = i;

        Array.Sort(idx, (a, b) =>
        {
            int c = frac[b].CompareTo(frac[a]);
            return c != 0 ? c : a.CompareTo(b);
        });

        for (int k = 0; k < left; k++)
            outArr[idx[k % n]]++;

        return outArr;
    }

    /// <summary>
    /// 从 remaining[] 里取 need 个，按“当前剩余比例”分配，保证：
    /// 1) sum(take)=need（若总剩余>=need）
    /// 2) take[i] <= remaining[i]
    /// 3) 并扣减 remaining
    /// </summary>
    private static int[] AllocateOnceFromRemaining(int[] remaining, int need)
    {
        int n = remaining.Length;
        int[] take = new int[n];

        int totalRemain = 0;
        for (int i = 0; i < n; i++) totalRemain += Mathf.Max(0, remaining[i]);

        if (need <= 0 || totalRemain <= 0) return take;

        // 安全：need 不会超过总剩余（理论上 dropPerTime 总和=totalCount，且 remaining 初始=totalCount）
        need = Mathf.Min(need, totalRemain);

        int allocated = 0;
        double[] frac = new double[n];

        // 先 floor(need * remain / totalRemain)
        for (int i = 0; i < n; i++)
        {
            int r = Mathf.Max(0, remaining[i]);
            if (r == 0) { take[i] = 0; frac[i] = -1; continue; }

            double exact = (double)need * r / totalRemain;
            int flo = (int)Math.Floor(exact + 1e-12);

            // flo 不可能超过 r（但加一道保险）
            flo = Mathf.Min(flo, r);

            take[i] = flo;
            allocated += flo;
            frac[i] = exact - flo;
        }

        int left = need - allocated;
        if (left > 0)
        {
            // 按小数部分从大到小补齐，且不能超过 remaining
            int[] idx = new int[n];
            for (int i = 0; i < n; i++) idx[i] = i;

            Array.Sort(idx, (a, b) =>
            {
                int c = frac[b].CompareTo(frac[a]);
                return c != 0 ? c : a.CompareTo(b);
            });

            for (int k = 0; k < idx.Length && left > 0; k++)
            {
                int i = idx[k];
                if (remaining[i] > take[i])
                {
                    take[i]++;
                    left--;
                    // 继续下一格，保证尽量按余数分配
                    if (k == idx.Length - 1 && left > 0) k = -1; // 再扫一轮（极少发生）
                }
            }
        }

        // 扣减 remaining
        for (int i = 0; i < n; i++)
            remaining[i] -= take[i];

        return take;
    }


    public static int Sum(int[] ints) { 
        int sum = 0;
        for (int i = 0; i < ints.Length; i++) { 
            sum += ints[i];
        }
        return sum; 
    }

}
