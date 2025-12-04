using FlyEggFrameWork.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DemandsPanel : TileItemPanel
{
    [SerializeField]
    protected GameObject _demandItemPrefab;

    public List<DemandItemPanel> demandItemPanels = new List<DemandItemPanel>();

    protected Dictionary<int,int> demandMap = new Dictionary<int,int>();

    public override void MountTileItem(TileItem tileItem, Action callback = null)
    {
        base.MountTileItem(tileItem, callback);
        Elf elf = (Elf)tileItem;
       
        button.gameObject.SetActive(false);
    }
    public void Init(Dictionary<int,int> demands) {
        demandItemPanels = new List<DemandItemPanel>();

        CommonTool.DeleteAllChildrenImmediate(transform.Find("items"));

        GameObject itemPanelPrefab = _demandItemPrefab;

        foreach (var itemId in demands.Keys) {
            GameObject itemPanelObj = GameObject.Instantiate(itemPanelPrefab,transform.Find("items")); 
            DemandItemPanel itemPanel = itemPanelObj.GetComponent<DemandItemPanel>();
            itemPanel.Init(itemId, demands[itemId]);
            demandItemPanels.Add(itemPanel);
        }
    }

    public void UpdateView(Dictionary<int,int> have)
    {
        foreach (DemandItemPanel demandItemPanel in demandItemPanels) {
            int needItemId = demandItemPanel.GetNeedItemId();
            int haveNum = have.ContainsKey(needItemId) ? have[needItemId] : 0;
            demandItemPanel.UpdateView(haveNum); 
        }
        if (GridControllerSystem._instance.CheckCanFeedElf((Elf)_tileItem))
        {
            button.gameObject.SetActive(true); 
        }
        else {
            button.gameObject.SetActive(false); 
        }
    }

}
