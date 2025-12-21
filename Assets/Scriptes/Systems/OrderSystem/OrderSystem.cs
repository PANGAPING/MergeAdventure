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

    public delegate void OrderEventHandler(OrderModel orderModel);

    public OrderEventHandler _onOrderFinished;

    public OrderEventHandler _onOrderAdd;

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

    }

    public void NewOrder(List<int> needItemIds,List<int> needItemNums,List<int> rewardType,List<int> rewardNum,bool isLevelTarget = false) { 
        OrderModel orderModel = new OrderModel();

        orderModel.IsLevelTarget = isLevelTarget;
        orderModel.NeedItemId = needItemIds.ToArray();
        orderModel.NeedItemNum =needItemNums.ToArray();
        orderModel.RewardItemType = rewardType.ToArray();
        orderModel.RewardItemNum = rewardNum.ToArray();

        orderModels.Add(orderModel);


        if (_onOrderAdd != null) {
            _onOrderAdd.Invoke(orderModel);
        }
    }


    public void FinishOrder(OrderModel orderModel) {

        int[] needItemId = orderModel.NeedItemId;

        for (int i = 0; i < needItemId.Length; i++) { 
            GridControllerSystem._instance.DisappearTargetItem(needItemId[i],GridUISystem._instance.GetOrderDishFlytarget(orderModel));
        }

        int[] rewardItemTypes = orderModel.RewardItemType;
        int[] rewardItemNums = orderModel.RewardItemNum;

        CharacterDishPanel characterDishPanel = GridUISystem._instance.GetOrderDish(orderModel);

        for (int i = 0; i < rewardItemTypes.Length; i++) {
            int itemType = rewardItemTypes[i];
            int itemNum = rewardItemNums[i];

            InventorySystem._instance.AddAsset(itemType, itemNum);

            Sprite assetIcon = ResourceHelper.GetAssetSprite(itemType);
            RewardItemPanel panel = GridUISystem._instance.GetAssetBar(itemType);
            if (panel != null)
            {
                GridUISystem._instance.FlyRewardIcon(assetIcon, 1, characterDishPanel.transform.position, panel.transform.position, panel.gameObject.GetComponent<RectTransform>());
            }
        }

        if (_onOrderFinished != null) {
            _onOrderFinished.Invoke(orderModel);
        }
    }

    public List<OrderModel> GetOrderModels() {
        return orderModels;
    }


}
