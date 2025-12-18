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

public class GridUISystem : GameSystem
{
    public static GridUISystem _instance;

    public List<TileItemPanel> tileItemPanels = new List<TileItemPanel>();

    public List<DemandsPanel> demandPanels = new List<DemandsPanel>();

    public List<CharacterDishPanel>  characterDishPanels= new List<CharacterDishPanel>();

    private Transform WorldNode;

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

    public void AddOrderDishes(OrderModel orderModel)
    {
    }

    public void RemoveOrderDishes(OrderModel orderModel)
    {
        CharacterDishPanel characterDishPanel = characterDishPanels.Find(x => x.GetOrderModel() == orderModel);
        GameObject.Destroy(characterDishPanel.gameObject);

        characterDishPanels.Remove(characterDishPanel);
    }

    /// <summary>
    /// 在指定位置弹出一条幸运提示文字（如“Lucky Drop!”）
    /// </summary>
    public void ShowPopup( Vector3 worldPosition, string text, Color color)
    {
        GameObject obj= GameObject.Instantiate(ResourceHelper.GetUIPrefab("TextPopup"), WorldNode);
        RectTransform rt = obj.GetComponent<RectTransform>();
        TextMeshProUGUI t = obj.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.color = color;

        transform.position = worldPosition;
        Vector2 anchoredPos = worldPosition;
        // 初始缩放
        rt.localScale = Vector3.one * 0.6f;
        rt.anchoredPosition = anchoredPos;

        // 动画序列
        Sequence seq = DOTween.Sequence();

        // 第一阶段：向上 + 淡入 + 放大
        seq.Append(rt.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack))
           .Join(rt.DOAnchorPos(anchoredPos, 0.2f).SetEase(Ease.OutQuad))
           .Join(t.DOFade(1f, 0.15f));

        // 第二阶段：轻微回弹到 1.0
        seq.Append(rt.DOScale(1f, 0.15f).SetEase(Ease.OutBack));

        // 第三阶段：往上飘 + 淡出
        seq.Append(rt.DOAnchorPos(anchoredPos + new Vector2(0, 40), 0.6f).SetEase(Ease.OutQuad))
           .Join(t.DOFade(0f, 0.6f));

        // 完成后销毁
        seq.OnComplete(() => GameObject.Destroy(obj));
    }


}
