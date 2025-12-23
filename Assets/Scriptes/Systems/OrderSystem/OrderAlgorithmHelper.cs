using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class OrderAlgorithmHelper
{
    private OrderSpawnConfig _spawnConfig;

    private Queue<int> recentChains;

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
        List<int> unlockedGenerators = GridControllerSystem._instance.GetGeneratorItemIds();
        List<ChainRuntimeInfo> candidateChainTarget = BuildCandidateChainTargets(unlockedGenerators);

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
            while (recentChains.Count > _spawnConfig.recentChainAvoidCount)
                recentChains.Dequeue();
        }

        return best;
    }

    private OrderModel RollOrderCandidate() { 
        OrderModel orderModel = new OrderModel();

        return orderModel;
    }

    private float GetOrderScore(OrderModel order, Vector2 hardnessDomain) { 
        float score = 0;

        return score;
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

    private List<ChainRuntimeInfo> BuildCandidateChainTargets(List<int> generatorIds) {

        List<ChainRuntimeInfo> chainRuntimeInfos = new List<ChainRuntimeInfo>();
        Dictionary<int, float> chainSupplyPower = ChainHelper.GetSupllyEffs();

        foreach (int chainId in chainSupplyPower.Keys) {
            if (chainSupplyPower[chainId] > 0) {
                chainRuntimeInfos.Add(ChainHelper.GetChainRuntimeInfo(chainId)); 
            }
        }

        return chainRuntimeInfos;
    }




    public enum OrderHardType { 
        EASY,
        CORE,
        STRETCH
    }
}
