using System.Collections;
using System.Collections.Generic;
using UnityEditor.Purchasing;
using UnityEngine;

public static class ChainHelper
{
    private static Dictionary<int, int> _rootCache = new Dictionary<int, int>(2048);
    private static Dictionary<int, int> _tierCache = new Dictionary<int, int>(2048);
    private static Dictionary<int, int> _nextCache = new Dictionary<int, int>(2048);
    private static Dictionary<int, float> _supplyEffCache = new Dictionary<int, float>(2048);
    private static Dictionary<int, int> _chainStackCache = new Dictionary<int, int>(2048);

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

    public static Dictionary<int, float> GetSupllyEffs() {
        BuildChainEffIfNeeded();
        return _supplyEffCache; 
    }
    public static  float GetSupplyEff(int chainId) {
        if (chainId <= 0) return 0;
        if (_supplyEffCache.TryGetValue(chainId, out var eff)) return eff;

        BuildChainEffIfNeeded();

        // 有索引就直接查，否则退化为自己当root
        if (_supplyEffCache.TryGetValue(chainId, out eff))
            return eff;

        _supplyEffCache[chainId] = 0;
        return 0;
    }

    public static int GetStock(int chainId) {
        if (chainId <= 0) return 0;
        if (_chainStackCache.TryGetValue(chainId, out var stack)) return stack;

        BuildChainEffIfNeeded();

        // 有索引就直接查，否则退化为自己当root
        if (_chainStackCache.TryGetValue(chainId, out stack))
            return stack;

        _chainStackCache[chainId] = 0;
        return 0;
    }

    private static bool _builtIndex = false;

    private static void BuildRootIndexIfNeeded()
    {
        if (_builtIndex) return;
        _builtIndex = true;

        var allItems = ConfigSystem.GetItemConfigs();
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

    private static bool _builtEff = false;
    private static void BuildChainEffIfNeeded(bool force =false) {
        if (_builtEff && !force) return;
        _builtEff = true;

        _supplyEffCache = new Dictionary<int, float>();

        List<int> generatorIds = GridControllerSystem._instance.GetGeneratorItemIds();

        for (int i = 0; i < generatorIds.Count; i++) { 
            GeneratorConfig generatorConfig = ConfigSystem.GetGeneratorConfig(generatorIds[i]);
            int[] dropIds = generatorConfig.DropItemIds;
            for (int dropIdIndex = 0; dropIdIndex < dropIds.Length; dropIdIndex++) {
                int dropId = dropIds[dropIdIndex];
                float dropRatio = generatorConfig.DropItemRatios[dropIdIndex];
                float eff = 1f * dropRatio;
                if (!_supplyEffCache.ContainsKey(dropId) || _supplyEffCache[dropId] < eff) { 
                    _supplyEffCache.Add(dropId, eff);
                }
            }
        }
    }

    private static bool _builtStock = false;

    private static void BuildChainStockIfNeeded(bool force = false) {
        if (_builtStock && !force) return;
        _builtStock = true;

        _chainStackCache = new Dictionary<int,int>();

        Dictionary<int, int> groundItem = GridControllerSystem._instance.GetWhiteGroundItemNumMap();

        foreach (int itemId in groundItem.Keys) { 
            int stack = groundItem[itemId] * ConvertItemIntoRootCount(itemId);
            int chainId = GetChainRoot(itemId);
            if (_chainStackCache.ContainsKey(chainId))
            {
                _chainStackCache[chainId] += stack;
            }
            else { 
                _chainStackCache[chainId] = stack;
            }
        }
    }

    public static ChainRuntimeInfo GetChainRuntimeInfo(int chainId) {
        ChainRuntimeInfo info = new ChainRuntimeInfo();
        info.chainId = chainId;
        info.stockValue = GetStock(chainId);
        info.supplyWeight = GetSupplyEff(chainId);
        return info;
    }

    public static ItemMeta GetItemMeta(int itemId) {

        ItemMeta itemMeta = new ItemMeta();
        
        itemMeta.itemId = itemId;

        itemMeta.chainId = GetChainRoot(itemId);

        itemMeta.level = GetTier(itemId);

        itemMeta.rarity = 1;

        return itemMeta;
    }

    public static int ConvertItemIntoRootCount(int itemId) {
        int level = GetTier(itemId);

        return 1 << (level - 1);
    }
}

public class ChainRuntimeInfo
{
    public int chainId; // root item id
    public float supplyWeight;  // 来自生成器
    public float stockValue;  // 来自库存/棋盘统计
    public int topLevel;
}

public class ItemMeta
{
    public int itemId;
    public int chainId;       // 同一合成链
    public int level;          // 合成阶层（越大越高级）
    public float rarity = 1f; // 1=普通, 1.2=稀有, 1.5=更稀有等
}

