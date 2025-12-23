using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChainHelper
{
    private static Dictionary<int, int> _rootCache = new Dictionary<int, int>(2048);
    private static Dictionary<int, int> _tierCache = new Dictionary<int, int>(2048);
    private static Dictionary<int, int> _nextCache = new Dictionary<int, int>(2048);

    public static int GetNextLevel(int itemId)
    {
        if (itemId <= 0) return 0;
        if (_nextCache.TryGetValue(itemId, out var n)) return n;

        var cfg = ConfigSystem.GetItemConfig(itemId);
        int next = cfg != null ? cfg.NextLevelID : 0;
        _nextCache[itemId] = next;
        return next;
    }

    public static int GetChainRoot(int itemId)
    {
        if (itemId <= 0) return 0;
        if (_rootCache.TryGetValue(itemId, out var root)) return root;

        BuildRootIndexIfNeeded();

        // 有索引就直接查，否则退化为自己当root
        if (_rootCache.TryGetValue(itemId, out root))
            return root;

        _rootCache[itemId] = itemId;
        return itemId;
    }

    public static int GetTier(int itemId)
    {
        if (itemId <= 0) return 0;
        if (_tierCache.TryGetValue(itemId, out var tier)) return tier;

        BuildRootIndexIfNeeded();

        if (_tierCache.TryGetValue(itemId, out tier))
            return tier;

        // 退化：未知就当1级
        _tierCache[itemId] = 1;
        return 1;
    }

    private static bool _builtIndex = false;

    private static void BuildRootIndexIfNeeded()
    {
        if (_builtIndex) return;
        _builtIndex = true;

        var allItems =ConfigSystem.GetItemConfigs();
        if (allItems == null || allItems.Count == 0) return;

        // 构建：next -> prev
        var prevMap = new Dictionary<int, int>(allItems.Count);
        foreach (var ic in allItems)
        {
            if (ic == null) continue;
            if (ic.NextLevelID > 0)
                prevMap[ic.NextLevelID] = ic.ID;
        }

        // 对每个 item，找 root 和 tier
        foreach (var ic in allItems)
        {
            if (ic == null) continue;

            int id = ic.ID;
            int tier = 1;
            int cur = id;
            int root = id;

            while (prevMap.TryGetValue(cur, out int prev))
            {
                tier++;
                cur = prev;
                root = cur;

                if (tier > 200) break; // 防止环
            }

            _rootCache[id] = root;
            _tierCache[id] = tier;
        }
    }
}

public class ChainRuntimeInfo
{
    public int chainId; // root item id
    public float supplyWeight;  // 来自生成器
    public float stockValue;  // 来自库存/棋盘统计
    public int recentDemandCount;  // 来自订单历史
}
