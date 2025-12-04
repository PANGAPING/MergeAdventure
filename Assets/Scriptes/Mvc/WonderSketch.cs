using FlyEggFrameWork.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WonderSketch : TileItem
{
    protected DemandsPanel _demandsPanel;

    protected Dictionary<int, int> _demands = new Dictionary<int, int>();

    public override void MountModel(ItemModel itemModel)
    {
        _demands = new Dictionary<int, int>() { {itemModel.IntData, 1 } };
        base.MountModel(itemModel);

        UpdateView();
    }

    protected override void UpdateView()
    {
        base.UpdateView();
        Transform imgNode = transform.Find("Img");
        Image itemImg = imgNode.GetComponent<Image>();
        ItemConfig needItemConfig = ConfigSystem.GetItemConfig(Model.IntData);
        if (needItemConfig == null)
        {
            return;
        }
        itemImg.sprite = ResourceHelper.GetItemSprite(needItemConfig);
        itemImg.rectTransform.pivot = Vector2.one - ResourceHelper.ConvertSpritePivotToRectTransform(itemImg.sprite);
        itemImg.SetNativeSize();
        itemImg.rectTransform.localPosition = Vector3.zero;


    }

    public Dictionary<int, int> GetDemandItems() { 
        return _demands;

    }

    protected override void ShowInEditor()
    {
        base.ShowInEditor();

        if (_demandsPanel == null)
        {
            GameObject demandItemsPanel = GameObject.Instantiate(ResourceHelper.GetUIPrefab("DemandsPanel"),GetUIPivotPoint(true));
            _demandsPanel = demandItemsPanel.GetComponent<DemandsPanel>();
        }

        _demandsPanel.Init(GetDemandItems());
        UpdateView();
    }
    public void SetDemand(int itemId) {
       _demands = new Dictionary<int, int>() { {itemId, 1 } };

        Model.SetIntData(itemId);
        ShowInEditor();
    }


}
