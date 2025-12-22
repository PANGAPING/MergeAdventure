using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "OrderSpawnConfig",
    menuName = "Setting/OrderSpawnConfig"
)]
public class OrderSpawnConfig : ScriptableObject
{
    [Header("同时存在订单数")]
    public int maxActiveOrders = 4;

    [Header("订单出现频率配置")]
    public int refillOrderInterval = 3000;
    public int orderStorageCapacity = 12;

    [Header("订单难度类型 权重占比")]
    public int easyOrderWeight = 30;
    public int coreOrderWeight = 40;
    public int stretchOrderWeight = 30;

    [Header("各难度类型订单 难度范围")]
    public Vector2 easyDifficultyDomain = new Vector2(10,20);
    public Vector2 coreDifficultyDomain = new Vector2(40,80);
    public Vector2 stretchDifficultyDomain = new Vector2(120,200);
    public float aveDiffculty = 50;

    [Header("订单需要物品种类数量 权重占比")]
    public int oneItemOrderWeight = 30;
    public int twoItemOrderWeight = 50;
    public int threeItemOrderWeight = 20;

    [Header("选择目标订单链时的 倾向|惩罚参数")]
    public int orderItemTypeLoss = 1; //订单物品种类不匹配损失

    public int supplyPowerLoss = 1; //供应能力不匹配损失

    public int stackLoss = 1; //堆积损失

    public int recentLoss = 1; //近期出现订单种类损失

    [Header("采样")]
    public int candidateSampleCount = 20;     // 生成候选订单采样次数

}
