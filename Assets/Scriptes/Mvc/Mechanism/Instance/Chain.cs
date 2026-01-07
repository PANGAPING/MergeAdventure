using FlyEggFrameWork.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chain : Mechanism
{
    [SerializeField]
    protected Image _chainBottomPanel;
    [SerializeField]
    protected Image _inItemPanel;

    public int _occupyItemId = -1;

    public ChainType _chainType = ChainType.CHAIN;

    public override void MountModel(ItemModel itemModel)
    {
        base.MountModel(itemModel);
        _inItemPanel.gameObject.SetActive(false);
        EnumUtils.TryParseFromStringValue<ChainType>(itemModel.GetItemConfig().Name, out _chainType);
    }

    public void SetOccpyItem(int itemId) { 
        _occupyItemId = itemId;
        _inItemPanel.gameObject.SetActive(true);
        ItemConfig itemConfig = ConfigSystem.GetItemConfig(itemId);
        _inItemPanel.sprite = ResourceHelper.GetItemSprite(itemConfig);
        _inItemPanel.rectTransform.pivot = Vector2.one -   ResourceHelper.ConvertSpritePivotToRectTransform(_inItemPanel.sprite);
        _inItemPanel.SetNativeSize();
        _inItemPanel.rectTransform.localPosition = Vector3.zero;
    }

    public int GetOccupyItem() { 
        return _occupyItemId;
    }


    public override void SetWhiteColor()
    {
        base.SetWhiteColor();

        _inItemPanel.color = _whiteColor;
        _itemImage.color = _whiteColor;
        _chainBottomPanel.color = _whiteColor;
    }

    public override void SetGrayColor()
    {
        base.SetGrayColor();

        _inItemPanel.color = _grayColor;
        _itemImage.color = _grayColor;
        _chainBottomPanel.color = _grayColor;
    }
}
public enum ChainType
{
    [StringValue("CHAIN")]
    CHAIN,
    [StringValue("CHAINSTACKHEAD")]
    CHAINSTACKHEAD
}


