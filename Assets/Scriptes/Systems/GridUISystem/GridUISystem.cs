using FlyEggFrameWork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridUISystem :  GameSystem
{
    public static GridUISystem _instance;

    public List<TileItemPanel> tileItemPanels = new List<TileItemPanel>();

    private Transform WorldNode;

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();

        GameObject worldNodeObj = GameObject.Find("Canvas");
        WorldNode = worldNodeObj.transform;

    }

    public void OpenButtonTips(TileItem tileItem,Action<TileItem> callback) {
        GameObject tipsObj = GameObject.Instantiate(ResourceHelper.GetUIPrefab("ButtonTips"),WorldNode);
        tipsObj.transform.position = tileItem.GetUIPivotPoint().position;
        TileItemPanel tileItemPanel = tipsObj.GetComponent<TileItemPanel>();
        tileItemPanel.MountTileItem(tileItem, () => { callback?.Invoke(tileItem);CloseButtonTips(tileItem);});

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

}
