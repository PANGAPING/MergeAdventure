using DG.Tweening;
using FlyEggFrameWork;
using FlyEggFrameWork.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridUISystem : GameSystem
{
    public static GridUISystem _instance;

    public List<TileItemPanel> tileItemPanels = new List<TileItemPanel>();

    public List<DemandsPanel> demandPanels = new List<DemandsPanel>();

    public List<CharacterDishPanel> characterDishPanels = new List<CharacterDishPanel>();

    private Transform WorldNode;

    public Dictionary<ASSETTYPE, RewardItemPanel> _assetBarsMap;

    public GroupProgressPanel _groupProgressPanel;

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();

        GameObject worldNodeObj = GameObject.Find("Canvas");
        WorldNode = worldNodeObj.transform;
    }

    protected override void Init()
    {
        base.Init();

        OrderSystem._instance._onOrderFinished += RemoveOrderDishes;
        OrderSystem._instance._onOrderAdd += AddOrderDishes;
        GridControllerSystem._instance._onGroundItemChange += UpdateDemandsPanels;
        GridControllerSystem._instance._onGroundItemChange += UpdateOrderDishes;
        InventorySystem._instance._onInventoryChange += UpdateAssetBars;
        InventorySystem._instance._onInventoryChange += UpdateGroupProgress;
        GridControllerSystem._instance._onUnlockGroup += NewGroupProgress;

        InitOrderDishes();
        InitAssetBars();
        InitGroupProgress();
    }

    public void OpenButtonTips(TileItem tileItem, Action<TileItem> callback) {
        GameObject tipsObj = GameObject.Instantiate(ResourceHelper.GetUIPrefab("ButtonTips"), WorldNode);
        tipsObj.transform.position = tileItem.GetUIPivotPoint().position;
        TileItemPanel tileItemPanel = tipsObj.GetComponent<TileItemPanel>();
        tileItemPanel.MountTileItem(tileItem, () => { callback?.Invoke(tileItem); CloseButtonTips(tileItem); });


        tileItemPanels.Add(tileItemPanel);
    }

    public void CloseButtonTips(TileItem tileItem) {
        TileItemPanel tileItemPanel = tileItemPanels.Find(x => x.GetTileItem() == tileItem);
        if (tileItemPanel != null) {
            tileItemPanel.CloseAnimation();
            tileItemPanels.Remove(tileItemPanel);
        }
    }

    public TileItemPanel NewBloodSlider(TileItem tileItem) {
        GameObject tipsObj = GameObject.Instantiate(ResourceHelper.GetUIPrefab("BloodSlider"), WorldNode);
        tipsObj.transform.position = tileItem.GetUIPivotPoint(true).position;
        BloodSliderPanel tileItemPanel = tipsObj.GetComponent<BloodSliderPanel>();
        tileItemPanel.MountTileItem(tileItem);

        return tileItemPanel;
    }
    public DemandsPanel NewElfTips(TileItem tileItem)
    {
        GameObject tipsObj = GameObject.Instantiate(ResourceHelper.GetUIPrefab("ElfTips"), WorldNode);
        tipsObj.transform.position = tileItem.GetUIPivotPoint().position;
        DemandsPanel tileItemPanel = tipsObj.GetComponent<DemandsPanel>();
        demandPanels.Add(tileItemPanel);
        tileItemPanel.MountTileItem(tileItem, () => { GridControllerSystem._instance.TryFeedElf((Elf)tileItem); });


        UpdateDemandsPanels();

        tileItem._onDie += () =>
        {
            demandPanels.Remove(tileItemPanel);
            GameObject.Destroy(tipsObj);
        };

        return tileItemPanel;
    }

    public void UpdateDemandsPanels()
    {
        Dictionary<int, int> groundItemMap = GridControllerSystem._instance.GetWhiteGroundItemNumMap();
        foreach (DemandsPanel demandsPanel in demandPanels)
        {
            demandsPanel.UpdateView(groundItemMap);
        }
    }

    public Vector3 GetOrderDishFlytarget(OrderModel orderModel) { 
        CharacterDishPanel characterDishPanel = characterDishPanels.Find(x => x.GetOrderModel() == orderModel);
        return characterDishPanel.GetDishFlytarget();
    }

    public void InitOrderDishes() {
        Transform dishesContainer = WorldNode.Find("Scrollbar/Viewport/Content/CharacterDishes");
        List<OrderModel> orderModels = OrderSystem._instance.GetOrderModels();

        characterDishPanels = new List<CharacterDishPanel>();
        CommonTool.DeleteAllChildren(dishesContainer);

        GameObject orderDishPrefab = ResourceHelper.GetUIPrefab("CharacterDish");
        GameObject wonderDishPrefab = ResourceHelper.GetUIPrefab("WonderCharacterDish");

        foreach (OrderModel orderModel in orderModels)
        {
            GameObject dishObj;
            if (orderModel.IsLevelTarget)
            {
                dishObj = GameObject.Instantiate(wonderDishPrefab,dishesContainer);
            }
            else { 
                dishObj = GameObject.Instantiate(orderDishPrefab,dishesContainer);
            }
            CharacterDishPanel  characterDishPanel= dishObj.GetComponent<CharacterDishPanel>();
            characterDishPanel.MountOrderModel(orderModel);
            characterDishPanels.Add(characterDishPanel);
        }

        UpdateOrderDishes();
    }

    public void UpdateOrderDishes()
    {
        Dictionary<int, int> groundItemMap = GridControllerSystem._instance.GetWhiteGroundItemNumMap();

        foreach (CharacterDishPanel characterDishPanel in characterDishPanels) {
            characterDishPanel.UpdateView(groundItemMap);
        }
    }


    public void InitAssetBars() {
        _assetBarsMap = new Dictionary<ASSETTYPE, RewardItemPanel>();
        Transform barContainer = WorldNode.Find("TopUIPanel/Content");

        RewardItemPanel engergyBar = barContainer.Find("EnergyBar").GetComponent<RewardItemPanel>();
        engergyBar.transform.GetComponent<GameUIButton>()._onClick += () =>
        {
            InventorySystem._instance.AddAsset(ASSETTYPE.ENERGY, 100);
        };

        _assetBarsMap.Add(ASSETTYPE.ENERGY, barContainer.Find("EnergyBar").GetComponent<RewardItemPanel>());
        _assetBarsMap.Add(ASSETTYPE.COIN, barContainer.Find("CoinBar").GetComponent<RewardItemPanel>());
        _assetBarsMap.Add(ASSETTYPE.DIAMOND, barContainer.Find("DiamondBar").GetComponent<RewardItemPanel>());

        UpdateAssetBars();

    }

    public GroupProgressPanel GetGroupProgressPanel() {
        return _groupProgressPanel;
    }
    public void InitGroupProgress() {
        NewGroupProgress();
    }

    public void UpdateGroupProgress()
    {
        if (_groupProgressPanel != null)
        {
            _groupProgressPanel.UpdateView(InventorySystem._instance.GetAssetNum(ASSETTYPE.KEY));
        }
    }
    public void NewGroupProgress() {
        if (_groupProgressPanel != null) { 
            Destroy(_groupProgressPanel.gameObject);
            _groupProgressPanel = null;
        }

        int groupNow = GridControllerSystem._instance.GetCurrentTargetGroupId();
        if (!GridControllerSystem._instance.IsGroupUnlockByKey(groupNow) || groupNow == -1) {
            return; 
        }

        GameObject progressPanelObject = GameObject.Instantiate(ResourceHelper.GetUIPrefab("GroupProgressPanel"),WorldNode);
        _groupProgressPanel = progressPanelObject.GetComponent<GroupProgressPanel>();

        _groupProgressPanel.Mount(GridControllerSystem._instance.GetGroupNeedKey(groupNow),groupNow);
        progressPanelObject.transform.position = GridControllerSystem._instance.GetGroupCenterPosition(groupNow);
        UpdateGroupProgress();
    }


    public void UpdateAssetBars() {
        foreach (ASSETTYPE assetType in _assetBarsMap.Keys) {
            RewardItemPanel rewardItemPanel = _assetBarsMap[assetType];
            int itemNum =  InventorySystem._instance.GetAssetNum(assetType);
            rewardItemPanel.UpdateView(null,itemNum.ToString()); 
        }
    }

    public Vector3 GetAssetBarPosition(ASSETTYPE assetType) {
        return _assetBarsMap[assetType].transform.position;
    }

    public Vector3 GetAssetBarPosition(int assetId) {
        return _assetBarsMap[InventorySystem._instance.ConvertIdToAssetType(assetId)].transform.position;
    }

    public RewardItemPanel GetAssetBar(int assetId) {
        ASSETTYPE assT = InventorySystem._instance.ConvertIdToAssetType(assetId);
        if (!_assetBarsMap.ContainsKey(assT))
        {
            return null;
        }
        return _assetBarsMap[assT];
    }


    public void AddOrderDishes(OrderModel orderModel)
    {
    }

    public CharacterDishPanel GetOrderDish(OrderModel orderModel) { 
        CharacterDishPanel characterDishPanel = characterDishPanels.Find(x => x.GetOrderModel() == orderModel);
        return characterDishPanel;
    }

    public void RemoveOrderDishes(OrderModel orderModel)
    {
        CharacterDishPanel characterDishPanel = characterDishPanels.Find(x => x.GetOrderModel() == orderModel);
        characterDishPanel.FinishAnimation();
        characterDishPanels.Remove(characterDishPanel);
    }

    /// <summary>
    /// 在指定位置弹出一条幸运提示文字（如“Lucky Drop!”）
    /// </summary>
    public void ShowPopup(Vector3 worldPosition, string text, Color color, float delay = 0.1f)
    {
        GameObject obj = GameObject.Instantiate(
    ResourceHelper.GetUIPrefab("TextPopup"),
    WorldNode
);

        RectTransform rt = obj.GetComponent<RectTransform>();
        TextMeshProUGUI t = obj.GetComponent<TextMeshProUGUI>();

        t.text = text;
        t.color = color;

        Vector2 anchoredPos = worldPosition;

        rt.localScale = Vector3.one * 0.6f;
        rt.anchoredPosition = anchoredPos;

        // 先设为完全透明，避免延迟期间闪一下
        t.alpha = 0f;

        Sequence seq = DOTween.Sequence();

        // ⭐ 延迟
        if (delay > 0f)
            seq.AppendInterval(delay);

        // 第一阶段：弹出
        seq.Append(rt.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack))
           .Join(rt.DOAnchorPos(anchoredPos, 0.2f).SetEase(Ease.OutQuad))
           .Join(t.DOFade(1f, 0.15f));

        // 第二阶段：回弹
        seq.Append(rt.DOScale(1f, 0.15f).SetEase(Ease.OutBack));

        // 第三阶段：上飘消失
        seq.Append(rt.DOAnchorPos(anchoredPos + new Vector2(0, 40), 0.6f).SetEase(Ease.OutQuad))
           .Join(t.DOFade(0f, 0.6f));

        seq.OnComplete(() => GameObject.Destroy(obj));
    }



    public void FlyRewardIcon(
    Sprite icon,
    float amount01,
    Vector3 startPos,
    Vector3 endPos,
    RectTransform targetUI
)
    {
        if (WorldNode == null || icon == null)
            return;

        amount01 = Mathf.Clamp01(amount01);

        int flyCount = Mathf.RoundToInt(Mathf.Lerp(1, 6, amount01));

        // ⭐ 防止多枚同时撞击，弹动被无限叠加
        bool targetPunched = false;

        for (int i = 0; i < flyCount; i++)
        {
            CreateSingleFly(
                icon,
                amount01,
                startPos,
                endPos,
                targetUI,
                i * 0.05f,
                () =>
                {
                    if (!targetPunched && targetUI != null)
                    {
                        targetPunched = true;
                        PlayTargetPunch(targetUI, amount01);
                    }
                }
            );
        }
    }
    private void CreateSingleFly(
    Sprite icon,
    float amount01,
    Vector3 startPos,
    Vector3 endPos,
    RectTransform targetUI,
    float delay,
    System.Action onHitTarget
)
    {
        GameObject go = new GameObject("FlyRewardIcon");
        go.transform.SetParent(WorldNode, false);

        Image img = go.AddComponent<Image>();
        img.sprite = icon;
        img.raycastTarget = false;

        RectTransform rt = img.rectTransform;
        rt.sizeDelta = new Vector2(72, 72);
        rt.position = startPos;
        rt.localScale = Vector3.one * 0.8f;

        Vector3 midOffset = new Vector3(
            Random.Range(-80f, 80f),
            Random.Range(100f, 180f),
            0
        ) * Mathf.Lerp(0.6f, 1.2f, amount01);

        Vector3 midPos = (startPos + endPos) * 0.5f + midOffset;
        float duration = Mathf.Lerp(0.8f, 0.45f, amount01);

        Sequence seq = DOTween.Sequence();
        seq.SetDelay(delay);

        seq.Append(rt.DOScale(1.05f, 0.12f).SetEase(Ease.OutBack));

        seq.Append(
            rt.DOPath(
                new Vector3[] { startPos, midPos, endPos },
                duration,
                PathType.CatmullRom
            ).SetEase(Ease.InQuad)
        );

        // 落点解压（飞行物自身）
        seq.Append(rt.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.06f));
        seq.Append(rt.DOScale(1f, 0.1f).SetEase(Ease.OutBack));

        seq.OnComplete(() =>
        {
            onHitTarget?.Invoke();
            GameObject.Destroy(go);
        });
    }

    private void PlayTargetPunch(RectTransform targetUI, float amount01)
    {
        targetUI.DOKill();

        float punchStrength = Mathf.Lerp(0.08f, 0.18f, amount01);
        float punchDuration = Mathf.Lerp(0.25f, 0.35f, amount01);

        targetUI.DOPunchScale(
            new Vector3(punchStrength, punchStrength, 0),
            punchDuration,
            vibrato: 6,
            elasticity: 0.8f
        );
    }

}
