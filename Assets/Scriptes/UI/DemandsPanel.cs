using FlyEggFrameWork.Tools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DemandsPanel : MonoBehaviour
{
    public List<DemandItemPanel> demandItemPanels = new List<DemandItemPanel>();
    public void Show(Dictionary<int,int> demands) {
        foreach (var panel in demandItemPanels) { 
            GameObject.DestroyImmediate(panel.gameObject);
        }

        demandItemPanels = new List<DemandItemPanel>();

        GameObject itemPanelPrefab = ResourceHelper.GetUIPrefab("DemandItemPanel");

        foreach (var itemId in demands.Keys) {
            GameObject itemPanelObj = GameObject.Instantiate(itemPanelPrefab,transform); 
            DemandItemPanel itemPanel = itemPanelObj.GetComponent<DemandItemPanel>();
            itemPanel.Show(itemId, demands[itemId]);
            demandItemPanels.Add(itemPanel);
        }
    }
}
