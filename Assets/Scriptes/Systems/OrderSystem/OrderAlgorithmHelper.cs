using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class OrderAlgorithmHelper
{
    private OrderSpawnConfig _spawnConfig;

    private Queue<int> recentChains = new Queue<int>();

    private Dictionary<OrderHardType, Vector2> HardnessDomainMap;

    public OrderAlgorithmHelper(OrderSpawnConfig orderSpawnConfig) { 
        _spawnConfig= orderSpawnConfig;

        HardnessDomainMap = new Dictionary<OrderHardType, Vector2>() {
            { OrderHardType.EASY,_spawnConfig.easyDifficultyDomain },
            { OrderHardType.CORE,_spawnConfig.coreDifficultyDomain},
            { OrderHardType.STRETCH,_spawnConfig.stretchDifficultyDomain}
        };
    }
    public OrderModel GenerateNewOrder()
    {
        // 1) 从玩家当前生成器构建“可订单物品池”
        // 你需要实现：获取玩家解锁的 generatorId 列表，然后拿到它们的可产出物。
        List<int> unlockedGenerators = GridControllerSystem._instance.GetGeneratorItemIds(false);
        List<ItemMeta> candidateChainTarget = BuildCandidateItemTargets(unlockedGenerators);

        Debug.Log(candidateChainTarget);
        if (candidateChainTarget == null || candidateChainTarget.Count == 0) return null;

        // 2) 决定订单结构（1/2/3个需求）
        int needTypeCount = RollNeedItemTypeCount();

        // 3) 决定订单难度（Easy/Core/Strech个需求）
        OrderHardType orderHardType = RollOrderHardType(); ;

        Vector2 hardnessDomain = HardnessDomainMap[orderHardType];

        // 4) 采样多个候选，选“得分最高”的那张（稳定、好调）
        OrderModel best = null;
        float bestScore = float.NegativeInfinity;

        int SAMPLE = _spawnConfig.candidateSampleCount;
        for (int s = 0; s < SAMPLE; s++)
        {
            OrderModel cand = RollOrderCandidate(candidateChainTarget,needTypeCount);
            if (cand == null) continue;

            float score = GetOrderScore(cand,hardnessDomain);

            if (score > bestScore)
            {
                bestScore = score;
                best = cand;
            }
        }

        if (best != null)
        {
            // 更新 recentChains
            int chainId = GetMainChainId(best);
            recentChains.Enqueue(chainId);

            int magicToolCount = 1;
            if (orderHardType == OrderHardType.CORE)
            {
                magicToolCount = 2;
            }
            else if (orderHardType == OrderHardType.STRETCH) {
                magicToolCount = 3; 
            }

            magicToolCount = Mathf.Min(magicToolCount, GridControllerSystem._instance.GetAvailableItemFromBoard(_spawnConfig.magicToolId) - OrderSystem._instance.GetAllNeedOfItem(_spawnConfig.magicToolId));

            if (magicToolCount > 0)
            {
                best.AddNeedItem(_spawnConfig.magicToolId, magicToolCount);
            }

            while (recentChains.Count > _spawnConfig.recentChainAvoidCount)
                recentChains.Dequeue();
        }

        return best;
    }

    private OrderModel RollOrderCandidate(List<ItemMeta> items,int itemTypeCount) {

        // 1) 选 needCount 个不同 chain 的目标（尽量）
        List<ItemMeta> picked = new List<ItemMeta>();
        HashSet<int> usedChain = new HashSet<int>();

        for (int k = 0; k < itemTypeCount; k++)
        {
            ItemMeta meta = PickOne(items, recentChains, usedChain);
            if (meta == null) return null;

            picked.Add(meta);
            usedChain.Add(meta.chainId);
        }

        // 2) 数量：tier 越高给 1 个，tier 低可以给 2~3 个
        List<int> needIds = new List<int>();
        List<int> needNums = new List<int>();

        foreach (var m in picked)
        {
            needIds.Add(m.itemId);

            int num = 1;
            needNums.Add(num);
        }

        // 3) 奖励：由难度反推（你也可以加入金币/体力/钥匙混合）
        var order = new OrderModel();
        order.IsLevelTarget = false;
        order.NeedItemId = needIds.ToArray();
        order.NeedItemNum = needNums.ToArray();

        order.RewardItemType = new int[] {101, 2 };
        order.RewardItemId = new int[] {0, 0 };
        order.RewardItemNum = new int[] { 1,(int)(GetOrderHardness(order) * _spawnConfig.hardnessToCoinR) };
        return order;
    }

    private static ItemMeta PickOne(List<ItemMeta> pool, Queue<int> recentChains, HashSet<int> usedChain)
    {
        // 简单策略：随机多次，避开 recentChains & usedChain
        const int TRY = 10;
        for (int t = 0; t < TRY; t++)
        {
            var meta = pool[UnityEngine.Random.Range(0, pool.Count)];
            if (usedChain.Contains(meta.chainId)) continue;
            if (recentChains != null && recentChains.Contains(meta.chainId)) continue;
            return meta;
        }

        // 如果实在避不开，就放宽
        for (int t = 0; t < TRY; t++)
        {
            var meta = pool[UnityEngine.Random.Range(0, pool.Count)];
            return meta;
        }

        return null;
    }


    //判断随机出来的订单的 分数，包括以下几个方面
    //订单难度，以一百体力为1进行难度加减
    //订单种类重复出现，最近记录次数前一个订单出现过为1，前两个订单出现过为0.5
    //需要物品与产出效能匹配，计算完成订单真实需要的体力数量（因为会开出不需要的物品），以一百体力为1进行难度加减
    //需要物品与地面堆积匹配，计算在前面的所有订单消耗了所有订单物品后，该订单直接消耗的订单体力数，以一百体力为1进行难度加减
    private float GetOrderScore(OrderModel order, Vector2 hardnessDomain) { 
        float score = 10;

        //1.难度惩罚
        int orderHardness = GetOrderHardness(order);
        float hardblameBase = 0;
        if (orderHardness < hardnessDomain.x)
        {
            hardblameBase += (hardnessDomain.x - orderHardness) / 100;
        }
        else if (orderHardness > hardnessDomain.y)
        {
            hardblameBase += (orderHardness - hardnessDomain.x) / 100;
        }

        score -= hardblameBase * _spawnConfig.hardnessLoss;

        //2.相同种类惩罚
        int mainChain = GetMainChainId(order);
        float recentBlameBase = 0;
        if (recentChains.Contains(mainChain)) {
            int recentOffset = recentChains.Count - recentChains.ToList().IndexOf(mainChain);
            recentBlameBase += 1f / recentOffset;
        }
        score -= recentBlameBase * _spawnConfig.recentLoss;

        //3.产出效能匹配惩罚
        float effBlameBase = 0;
        float trueHardness = GetTrueOrderHardness(order);

        if (trueHardness < hardnessDomain.x)
        {
            effBlameBase += (hardnessDomain.x - trueHardness) / 100;
        }
        else if (trueHardness > hardnessDomain.y)
        {
             effBlameBase += (trueHardness - hardnessDomain.x) / 100;
        }

        score -= effBlameBase * _spawnConfig.supplyPowerLoss;

        //4.堆积匹配，还未实装


        return score;
    }

    private int GetTrueOrderHardness(OrderModel order) {
        List<int> genItemIds = GridControllerSystem._instance.GetGeneratorItemIds();
        List<GeneratorConfig> gens = new List<GeneratorConfig>();
        foreach (var id in genItemIds) gens.Add(ConfigSystem.GetGeneratorConfig(id));

        var r = OrderClickEstimator.EstimateMinExpectedClicks(order, gens);
        return r.minExpectedClicks;
    }

    private int GetOrderHardness(OrderModel order) {
        int hardness = 0;
        for (int i = 0; i < order.NeedItemId.Length ; i++) {
            hardness += ChainHelper.ConvertItemIntoRootCount(order.NeedItemId[i]) * order.NeedItemNum[i];
        }
        return hardness;
    }

    private int RollNeedItemTypeCount()
    {
        float r = UnityEngine.Random.value * 100;
        if (r < _spawnConfig.oneItemOrderWeight) return 1;
        if (r < _spawnConfig.oneItemOrderWeight + _spawnConfig.twoItemOrderWeight) return 2;
        return 3;
    }

    private OrderHardType RollOrderHardType()
    {
        float r = UnityEngine.Random.value * 100;
        if (r < _spawnConfig.easyOrderWeight) return OrderHardType.EASY;
        if (r < _spawnConfig.easyOrderWeight + _spawnConfig.coreOrderWeight) return OrderHardType.CORE;
        return OrderHardType.STRETCH;
    }


    private int GetMainChainId(OrderModel order)
    {
        int bestChain = -1;
        float best = -1;

        for (int i = 0; i < order.NeedItemId.Length; i++)
        {
            int itemId = order.NeedItemId[i];
            int chainId = ChainHelper.GetChainRoot(itemId);
            int power = ChainHelper.ConvertItemIntoRootCount(itemId);
            if (power > best)
            {
                best = power;
                bestChain = chainId;
            }
        }
        return bestChain;
    }

    private List<ItemMeta> BuildCandidateItemTargets(List<int> generatorIds) {

        List<ChainRuntimeInfo> chainRuntimeInfos = new List<ChainRuntimeInfo>();
        Dictionary<int, float> chainSupplyPower = ChainHelper.GetSupllyEffs();

        foreach (int chainId in chainSupplyPower.Keys) {
            if (chainSupplyPower[chainId] > 0) {
                chainRuntimeInfos.Add(ChainHelper.GetChainRuntimeInfo(chainId)); 
            }
        }

        List<ItemMeta> candidateItems = new List<ItemMeta>();

        List<ItemConfig> candidateConfigs = ConfigSystem.GetItemConfigs().FindAll(x => x.Type == ItemType.NORMAL && chainRuntimeInfos.Exists(y => y.chainId == ChainHelper.GetChainRoot(x.ID)));

        foreach (ItemConfig itemConfig in candidateConfigs)
        {
            candidateItems.Add(ChainHelper.GetItemMeta(itemConfig.ID));
        }

        return candidateItems;
    }




    public enum OrderHardType { 
        EASY,
        CORE,
        STRETCH
    }
}
