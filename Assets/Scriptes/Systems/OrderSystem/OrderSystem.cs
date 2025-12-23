using DG.Tweening.Core.Easing;
using FlyEggFrameWork;
using FlyEggFrameWork.GameGlobalConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class OrderSystem : GameSystem
{
    public static OrderSystem _instance;

    public delegate void OrderEventHandler(OrderModel orderModel);

    public OrderEventHandler _onOrderFinished;

    public OrderEventHandler _onOrderAdd;

    protected List<OrderModel> activeOrderModels = new List<OrderModel>();

    protected OrderModel targetOrderModel;

    //Generate Order paras

    private OrderSpawnConfig _orderSpawnConfig;

    private OrderAlgorithmHelper _orderAlgorithmHelper;

    private float _lastOrderPoolFillTime = -9999;

    private int _orderCountInPool = 4;

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();

        targetOrderModel = NewOrder(new List<int>() { 1100105 }, new List<int>() { 1 },new List<int> {1,2},new List<int> {100,200},true);


        //  NewOrder(new List<int>() { 1100105 }, new List<int>() { 1 },new List<int> {1,2},new List<int> {100,200},true);
        //  NewOrder(new List<int>() { 1100203,1100304 }, new List<int>() { 1,1 },new List<int> {101,2},new List<int> {1,20},false);
        //  NewOrder(new List<int>() { 1100204,1100305 }, new List<int>() { 1,1 },new List<int> {101,2},new List<int> {2,40},false);

        string orderSpawnConfigPath = Path.Combine(FoldPath.SettingFolderPath, "OrderSpawnConfig");
        _orderSpawnConfig = Resources.Load<OrderSpawnConfig>(orderSpawnConfigPath);
        _orderAlgorithmHelper = new OrderAlgorithmHelper(_orderSpawnConfig);
    }

    protected override void Init()
    {
        base.Init();
    }

    protected override void Update()
    {
        base.Update();

        if (Time.time - _lastOrderPoolFillTime > _orderSpawnConfig.refillOrderInterval) {
            AddOrderCountToPool(1); 
        }
    }

    private void AddOrderCountToPool(int count = 1)
    {
        if (activeOrderModels.Count + _orderCountInPool < _orderSpawnConfig.orderStorageCapacity)
        {
            _orderCountInPool += 1;
            _lastOrderPoolFillTime = Time.time;
        }

        TryCreateNewOrdersFromPool();
    }

    private void TryCreateNewOrdersFromPool()
    {
        int newOrderCount = Mathf.Min(_orderCountInPool, _orderSpawnConfig.maxActiveOrders - activeOrderModels.Count);
        for (int i = 0; i < newOrderCount; i++)
        {
            OrderModel newOrder = _orderAlgorithmHelper.GenerateNewOrder();
            if (newOrder != null)
            {
                NewOrder(newOrder);
                _orderCountInPool -= 1;
            }
        }
    }

    public void NewOrder(OrderModel orderModel)
    {
        activeOrderModels.Add(orderModel);
        if (_onOrderAdd != null)
        {
            _onOrderAdd.Invoke(orderModel);
        }
    }

    public OrderModel NewOrder(List<int> needItemIds, List<int> needItemNums, List<int> rewardType, List<int> rewardNum, bool isLevelTarget = false)
    {
        OrderModel orderModel = new OrderModel();

        orderModel.IsLevelTarget = isLevelTarget;
        orderModel.NeedItemId = needItemIds.ToArray();
        orderModel.NeedItemNum =needItemNums.ToArray();
        orderModel.RewardItemType = rewardType.ToArray();
        orderModel.RewardItemNum = rewardNum.ToArray();

        return orderModel;
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
            if (InventorySystem._instance.ConvertIdToAssetType(itemType) == ASSETTYPE.KEY) {
                GroupProgressPanel groupProgressPanel = GridUISystem._instance.GetGroupProgressPanel();
                if (groupProgressPanel != null) { 
                    GridUISystem._instance.FlyRewardIcon(assetIcon, 1, characterDishPanel.transform.position,groupProgressPanel.transform.position, groupProgressPanel.gameObject.GetComponent<RectTransform>());
                }
            }
        }

        if (_onOrderFinished != null) {
            _onOrderFinished.Invoke(orderModel);
        }
    }

    public List<OrderModel> GetOrderModels() {
        return activeOrderModels;
    }

    public OrderModel GetLevelTargetOrder() {
        return targetOrderModel;
    }

}
