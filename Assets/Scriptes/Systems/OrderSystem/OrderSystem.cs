using DG.Tweening.Core.Easing;
using FlyEggFrameWork;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrderSystem : GameSystem
{
    public static OrderSystem _instance;

    public EventHandler _onOrderChange;

    protected List<OrderModel> orderModels = new List<OrderModel>();

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();

        NewOrder(new List<int>() { 1100105 }, new List<int>() { 1 },new List<int> {1,2},new List<int> {100,200},true);
        NewOrder(new List<int>() { 1100203,1100304 }, new List<int>() { 1,1 },new List<int> {101,2},new List<int> {1,20},false);
        NewOrder(new List<int>() { 1100204,1100305 }, new List<int>() { 1,1 },new List<int> {101,2},new List<int> {2,40},false);
    }

    protected override void Init()
    {
        base.Init();

        GridUISystem._instance.InitOrderDishes();
    }

    public void NewOrder(List<int> needItemIds,List<int> needItemNums,List<int> rewardType,List<int> rewardNum,bool isLevelTarget = false) { 
        OrderModel orderModel = new OrderModel();

        orderModel.IsLevelTarget = isLevelTarget;
        orderModel.NeedItemId = needItemIds.ToArray();
        orderModel.NeedItemNum =needItemNums.ToArray();
        orderModel.RewardItemType = rewardType.ToArray();
        orderModel.RewardItemNum = rewardNum.ToArray();

        orderModels.Add(orderModel);
    }


    public void FinishOrder() { 
    
    }

    public List<OrderModel> GetOrderModels() {
        return orderModels;
    }


    private void OrderChangeEvent()
    {
        if (_onOrderChange != null)
        {
            _onOrderChange.Invoke();
        }
    }
}
