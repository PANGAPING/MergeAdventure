using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DropAlgorithmHelper
{

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
    /// </summary>
    /// <param name="totalCount">总掉落数量</param>
    /// <param name="itemIds">物品ID数组</param>
    /// <param name="ratios">物品对应掉落比例</param>
    /// <param name="dropWeights">每次掉落占总数量的权重数组</param>
    /// <param name="dropIndex">第几次掉落（0开始）</param>
    /// <returns>Dictionary(物品ID → 掉落数量)</returns>
    public static Dictionary<int, int> GetDropResultOnce(
        int totalCount,
        int[] itemIds,
        float[] ratios,
        float[] dropWeights,
        int dropIndex)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();

        // 初始化返回结构
        for (int i = 0; i < itemIds.Length; i++)
            result[itemIds[i]] = 0;

        // 如果 dropIndex 超过长度，直接返回空
        if (dropIndex < 0 || dropIndex >= dropWeights.Length)
            return result;

        // 算出本次掉落数量
        int dropThisTime = Mathf.RoundToInt(totalCount * dropWeights[dropIndex]);

        // 求比例总和
        float ratioSum = 0f;
        for (int i = 0; i < ratios.Length; i++)
            ratioSum += ratios[i];

        // 根据比例随机掉落
        for (int n = 0; n < dropThisTime; n++)
        {
            float rand = Random.value * ratioSum;
            float cur = 0;

            for (int i = 0; i < itemIds.Length; i++)
            {
                cur += ratios[i];
                if (rand <= cur)
                {
                    result[itemIds[i]]++;
                    break;
                }
            }
        }

        return result;
    }


    public static int Sum(int[] ints) { 
        int sum = 0;
        for (int i = 0; i < ints.Length; i++) { 
            sum += ints[i];
        }
        return sum; 
    }

}
