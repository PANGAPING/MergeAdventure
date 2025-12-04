using FlyEggFrameWork.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UIElements;

public class Elf : Mechanism
{
    protected DemandsPanel _demandsPanel;

    protected Dictionary<int, int> _demands;


    public override void MountModel(ItemModel itemModel)
    {
        base.MountModel(itemModel);

        _demands = CommonTool.Array2ToDictionary(itemModel.IntArray2Data);
    }

    public Dictionary<int, int> GetDemandItems() { 
        Dictionary<int,int>  demands = new Dictionary<int, int>();
        if(Model.IntArray2Data == null) {
            return demands; 
        }

        for (int i = 0; i < Model.IntArray2Data.GetLength(0); i++)
        {
            demands.Add(Model.IntArray2Data[i, 0], Model.IntArray2Data[i, 1]);
        }

        return demands;
    }

    public override void SetWhiteColor()
    {
        base.SetWhiteColor();

        if (_demandsPanel == null)
        {
            _demandsPanel = GridUISystem._instance.NewElfTips(this);
        }

    }

    protected override void ShowInEditor()
    {
        base.ShowInEditor();
        transform.Find("GroupPanel").GetComponent<TextMeshProUGUI>().text = GetGroup().ToString();
        transform.Find("GroupPanel").gameObject.SetActive(true);

        if (_demandsPanel == null)
        {
            GameObject demandItemsPanel = GameObject.Instantiate(ResourceHelper.GetUIPrefab("DemandsPanel"),GetUIPivotPoint(true));
            _demandsPanel = demandItemsPanel.GetComponent<DemandsPanel>();
        }

        _demandsPanel.Init(GetDemandItems());
    }

    public void AddDemand(int itemId, int count) {
        if (_demands.ContainsKey(itemId))
        {
            _demands[itemId] += count;
        }
        else {
            _demands.Add(itemId, count);    
        }

        Model.SetIntArray2Data(CommonTool.DictionaryToArray2(_demands));
        ShowInEditor();
    }


    public void DeleteDemand(int itemId, int count) {
        if (_demands.ContainsKey(itemId))
        {
            _demands[itemId] -= count;
            if (_demands[itemId] <= 0) { 
                _demands.Remove(itemId);
            }
        }
        Model.SetIntArray2Data(CommonTool.DictionaryToArray2(_demands));
        ShowInEditor();

    }
}
