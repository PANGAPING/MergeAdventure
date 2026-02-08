using DG.Tweening.Core.Easing;
using FlyEggFrameWork;
using FlyEggFrameWork.GameGlobalConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class OrderSystem : GameSystem
{
    public static OrderSystem _instance;

    public delegate void OrderEventHandler(OrderModel orderModel);

    public OrderEventHandler _onOrderFinished;

    public OrderEventHandler _onOrderAdd;

    protected List<OrderModel> activeOrderModels = new List<OrderModel>();

    protected List<OrderModel> targetOrderModels = new List<OrderModel>();

    //Generate Order paras

    private OrderSpawnConfig _orderSpawnConfig;

    private OrderAlgorithmHelper _orderAlgorithmHelper;

    private float _lastOrderPoolFillTime = -9999;

    private int _orderCountInPool = 8;

    private bool _isOrderFormConfig = false;

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();

        targetOrderModels = new List<OrderModel>();
        //targetOrderModels.Add( NewOrder(new List<int>() { 1100904,1100305 }, new List<int>() { 1 },new List<int> {1,2},new List<int> {30,200},true));
        //targetOrderModels.Add( NewOrder(new List<int>() { 1100907,1100306 }, new List<int>() { 1 },new List<int> {1,2},new List<int> {50,200},true));
        //targetOrderModels.Add( NewOrder(new List<int>() { 1100910,1100307 }, new List<int>() { 1 },new List<int> {1,2},new List<int> {100,200},true));

        //  targetOrderModels.Add( NewOrder(new List<int>() { 1100905}, new List<int>() { 1 },new List<int> {1,2},new List<int> {30,200},true));
        //  targetOrderModels.Add( NewOrder(new List<int>() { 1100907}, new List<int>() { 1 },new List<int> {1,2},new List<int> {50,200},true));
        //  targetOrderModels.Add( NewOrder(new List<int>() { 1100910}, new List<int>() { 1 },new List<int> {1,2},new List<int> {100,200},true));




        //  NewOrder(new List<int>() { 1100105 }, new List<int>() { 1 },new List<int> {1,2},new List<int> {100,200},true);
        //  NewOrder(new List<int>() { 1100203,1100304 }, new List<int>() { 1,1 },new List<int> {101,2},new List<int> {1,20},false);
        //  NewOrder(new List<int>() { 1100204,1100305 }, new List<int>() { 1,1 },new List<int> {101,2},new List<int> {2,40},false);

        List<OrderConfig> defaultOrders =  ConfigSystem.GetDefaultOrderOfIsland(MergeAdventureProgressController._instance.GetLevelId());
        _isOrderFormConfig = defaultOrders.Count > 0;

        string orderSpawnConfigPath = Path.Combine(FoldPath.SettingFolderPath, "OrderSpawnConfig");
        _orderSpawnConfig = Resources.Load<OrderSpawnConfig>(orderSpawnConfigPath);
        _orderAlgorithmHelper = new OrderAlgorithmHelper(_orderSpawnConfig);
    }

    protected override void Init()
    {
        base.Init();
        if (_isOrderFormConfig)
        {
            List<OrderConfig> defaultOrders =  ConfigSystem.GetDefaultOrderOfIsland(MergeAdventureProgressController._instance.GetLevelId());
            foreach(OrderConfig orderConfig in defaultOrders)
            {
                NewOrder(OrderModel.FromConfig(orderConfig));
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Time.time - _lastOrderPoolFillTime > _orderSpawnConfig.refillOrderInterval && !_isOrderFormConfig) {
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

    public int GetAllNeedOfItem(int targetItem) {
        int needCount = 0;

        foreach (OrderModel orderModel in activeOrderModels)
        {
            int[] needItemIds = orderModel.NeedItemId;
            int[] needItemCount= orderModel.NeedItemNum;
            if (needItemIds.Contains(targetItem)) {
                needCount += needItemCount[needItemIds.ToList().IndexOf(targetItem)];
            }
        }

        return needCount;
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

        if (orderModel.IsLevelTarget) {
            OrderModel finishedOrder = targetOrderModels[0];

            GridControllerSystem._instance.Drop(new Dictionary<int, int> { { finishedOrder.NeedItemId[0], 1 } },new Vector2Int(4,9),true);

            targetOrderModels.RemoveAt(0);
            if (targetOrderModels.Count > 0) {
                _onOrderAdd.Invoke(targetOrderModels[0]);
            }
        }

        if (activeOrderModels.Contains(orderModel)) { 
            activeOrderModels.Remove(orderModel);
        }

        if (_onOrderFinished != null) {
            _onOrderFinished.Invoke(orderModel);
        }

        if (_isOrderFormConfig)
        {
            int[] nextOrderIds = orderModel.NextOrderIds;
            if(nextOrderIds!=null && nextOrderIds.Length > 0)
            {
                for(int i = 0; i < nextOrderIds.Length; i++)
                {
                    OrderConfig orderConfig = ConfigSystem.GetOrderConfig(nextOrderIds[i]);
                    if (orderConfig != null)
                    {
                        NewOrder(OrderModel.FromConfig(orderConfig));
                    }
                }
            }
        }
        else
        {
            TryCreateNewOrdersFromPool();
        }
    }

    public List<OrderModel> GetOrderModels() {
        return activeOrderModels;
    }

    public OrderModel GetLevelTargetOrder() {
        if(targetOrderModels==null || targetOrderModels.Count == 0)
        {
            return null;
        }
        return targetOrderModels[0];
    }

}
