using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UIElements;

public class Elf : Mechanism
{
    protected DemandsPanel _demandsPanel;

    public int GetGroup() {
        return Model.IntData;
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

    protected override void ShowInEditor()
    {
        base.ShowInEditor();
        transform.Find("GroupPanel").GetComponent<TextMeshProUGUI>().text = GetGroup().ToString();

        if (_demandsPanel == null)
        {
            GameObject demandItemsPanel = GameObject.Instantiate(ResourceHelper.GetUIPrefab("DemandsPanel"), transform);
            _demandsPanel = demandItemsPanel.GetComponent<DemandsPanel>();
        }

        _demandsPanel.Show(GetDemandItems());
    }
}
