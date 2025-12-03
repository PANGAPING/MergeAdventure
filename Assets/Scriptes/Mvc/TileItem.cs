using DG.Tweening;
using FlyEggFrameWork;
using FlyEggFrameWork.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class TileItem : FlyEggInstance
{
    public ItemModel Model;

    protected bool inAnimation = false;

    protected bool active = false;

    private Vector3 originalScale;

    private static Color _whiteColor = new Color(255f / 255, 255f / 255, 255f / 255);

    private static Color _grayColor = new Color(113f / 255, 113f / 255, 113f / 255);

    private Image _itemImage;

    public delegate void EventHandler();

    public EventHandler  _onDie;

    protected override void InitSelf()
    {
        base.InitSelf();
        _itemImage = transform.Find("Img").GetComponent<Image>();
    }

    protected override void Init()
    {
        base.Init();
        originalScale = Vector3.one;
    }

    public virtual void Show() {
        gameObject.SetActive(true);
    }

    public virtual void Hide() {
        gameObject.SetActive(false);
    }
    public virtual bool IsActive() {
        return active;
    }

    public virtual void Active() {
        active = true;
    }

    public virtual void DeActive() {
        active = false;
    }

    public virtual void SetWhiteColor() {
        if (_itemImage == null) {
            _itemImage = transform.Find("Img").GetComponent<Image>();
        }
        _itemImage.color = _whiteColor;
        
        

    }
    public virtual void SetGrayColor() {
        if (_itemImage == null)
        {
            _itemImage = transform.Find("Img").GetComponent<Image>();
        }
        _itemImage.color = _grayColor;
    }

    public int GetGroup() {
        return Model.IntData;
    }

    public void SetGroup(int value) {
        Model.IntData = value;
        ShowInEditor();
    }


    public virtual void Die() { 
        BounceAnimation(AnimationEndActionType.DESTORY, 0.1f);
        if (_onDie != null) {
            _onDie.Invoke();
        }
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

    public virtual void MoveAnimation(Vector3 targetPostion, AnimationEndActionType animationEndActionType = AnimationEndActionType.NONE, float speed = 2000f)
    {
        inAnimation = true;

        float span = (targetPostion - transform.position).magnitude/speed;
        transform.DOPath(new Vector3[] { transform.position, targetPostion }, span, PathType.Linear).onComplete += () =>
         {
             inAnimation = false;
             AnimationEndAction(animationEndActionType);
         };
    }
    public virtual void AppearAnimation(AnimationEndActionType animationEndActionType = AnimationEndActionType.NONE)
    {
        inAnimation = true;
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).onComplete += () =>
         {
             inAnimation = false;
             AnimationEndAction(animationEndActionType);
         };
    }

    public virtual void BounceAnimation(AnimationEndActionType animationEndActionType = AnimationEndActionType.NONE,float duration = 0.6f) {
        inAnimation = true;

        float scaleDown = 0.8f;      // 缩小比例
        float overshoot = 1.2f;
        // 重置 scale（避免短时间内多次调用累积）
        transform.localScale = originalScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(originalScale * scaleDown, duration * 0.4f))
           .Append(transform.DOScale(originalScale * overshoot, duration * 0.3f))
           .Append(transform.DOScale(originalScale, duration * 0.3f))
           .SetEase(Ease.OutBack).onComplete += () => { 
            inAnimation = false; AnimationEndAction(animationEndActionType);
           };
    }


    public virtual void AnimationEndAction(AnimationEndActionType animationEndActionType) {
        if (animationEndActionType == AnimationEndActionType.NONE) { 
        
        }
        else if(animationEndActionType == AnimationEndActionType.DESTORY)
        {
            GameObject.Destroy(gameObject);
        }
    }

    protected virtual void ShowInEditor() { 
         
    }

    protected virtual void ShowInPlayMode() { 
    
    }

    public virtual Transform GetUIPivotPoint(bool bottom = false) {
        if (bottom)
        {
            return transform.Find("Points/BottomUiPivot");    
        }
        else { 
            return transform.Find("Points/UiPivot");    
        }
    }

}

public enum AnimationEndActionType { 
    NONE,
    DESTORY
}
