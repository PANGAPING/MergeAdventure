using DG.Tweening;
using FlyEggFrameWork;
using FlyEggFrameWork.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class TileItem : FlyEggInstance
{
    public ItemModel Model;

    protected bool inAnimation = false;

    protected override void InitSelf()
    {
        base.InitSelf();
    }

    protected override void Init()
    {
        base.Init();
    }

    public virtual void Show() {
        gameObject.SetActive(true); 
    }

    public virtual void Hide() { 
        gameObject.SetActive(false); 
    }
    public int GetGroup() {
        return Model.IntData;
    }

    public void SetGroup(int value) { 
        Model.IntData = value;
        ShowInEditor();
    }


    public virtual void MountModel(ItemModel itemModel)
    {
        Model =itemModel;
        Transform imgNode = transform.Find("Img");
        Image itemImg =imgNode.GetComponent<Image>();
        itemImg.sprite = ResourceHelper.GetItemSprite(itemModel.GetItemConfig());
        itemImg.rectTransform.pivot = Vector2.one -   ResourceHelper.ConvertSpritePivotToRectTransform(itemImg.sprite);
        itemImg.SetNativeSize();
        itemImg.rectTransform.localPosition = Vector3.zero;

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

    public virtual Vector2Int GetPos() {
        return CommonTool.ArrayToVector2Int(Model.TilePos);
    }

    public virtual void SetPos(Vector2Int pos) {
        this.Model.TilePos = CommonTool.Vector2IntToArray(pos);
    }

    public virtual int GetLayer()
    {
        return Model.GetItemConfig().Layer;
    }

    public virtual bool IsMovable() {
        return Model.GetItemConfig().Movable == 1; 
    }

    public virtual void MoveAnimation(Vector3 targetPostion, float speed = 1000f)
    {
        inAnimation = true;

        float span = (targetPostion - transform.position).magnitude/speed;
        transform.DOPath(new Vector3[] { transform.position, targetPostion }, span, PathType.Linear).onComplete += () =>
         {
             inAnimation = false;
         };
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
