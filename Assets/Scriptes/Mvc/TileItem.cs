using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class TileItem : FlyEggInstance
{
    public ItemModel Model;

    protected override void InitSelf()
    {
        base.InitSelf();
    }

    protected override void Init()
    {
        base.Init();
    }


    public virtual void MountModel(ItemModel itemModel)
    {
        Model =itemModel;
        Transform imgNode = transform.Find("Img");
        Image itemImg =imgNode.GetComponent<Image>();
        itemImg.sprite = ResourceHelper.GetItemSprite(itemModel.GetItemConfig());
        itemImg.rectTransform.pivot = Vector2.one - ResourceHelper.ConvertSpritePivotToRectTransform(itemImg.sprite);
        itemImg.SetNativeSize();

        if (Application.isPlaying)
        {
            ShowInPlayMode();
        }
        else {
            ShowInEditor();
        }
    }

    public virtual ItemType GetItemType() { 
        return Model.GetItemConfig().Type;
    }

    public virtual int GetLayer()
    {
        return Model.GetItemConfig().Layer;
    }

    protected virtual void ShowInEditor() { 
         
    }

    protected virtual void ShowInPlayMode() { 
    
    }

    protected virtual Transform GetUIPivotPoint(bool bottom = false) {
        if (bottom)
        {
            return transform.Find("Points/BottomUiPivot");    
        }
        else { 
            return transform.Find("Points/UiPivot");    
        }
    }
}
