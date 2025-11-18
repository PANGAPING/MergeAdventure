using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileItem : FlyEggInstance
{
    public ItemModel Model;

    public virtual void MountModel(ItemModel itemModel)
    {
        Model =itemModel;
        Transform imgNode = transform.Find("Img");
        Image itemImg =imgNode.GetComponent<Image>();
        itemImg.sprite = ResourceHelper.GetItemSprite(itemModel.GetItemConfig());
        itemImg.rectTransform.pivot = Vector2.one - ResourceHelper.ConvertSpritePivotToRectTransform(itemImg.sprite);
        itemImg.SetNativeSize();
    }

    public virtual ItemType GetItemType() { 
        return Model.GetItemConfig().Type;
    }
}
