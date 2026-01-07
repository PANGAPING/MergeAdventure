using DG.Tweening;
using FlyEggFrameWork;
using FlyEggFrameWork.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    protected static Color _whiteColor = new Color(255f / 255, 255f / 255, 255f / 255);

    protected static Color _grayColor = new Color(113f / 255, 113f / 255, 113f / 255);

    protected Image _itemImage;

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

    protected virtual void UpdateView() { 
    
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

    public void SetGroup(int value)
    {
        Model.IntData = value;
        Transform groupPanel = transform.Find("GroupPanel");
        if (groupPanel != null)
        {
            groupPanel.GetComponent<TextMeshProUGUI>().text = value.ToString();
        }
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

    public virtual void GetTaped() { 
        BounceAnimation();
    }

    public virtual void MoveAnimation(
    Vector3 targetPostion,
    AnimationEndActionType animationEndActionType = AnimationEndActionType.NONE,
    float duration= 0.4f)
    {
        inAnimation = true;

        // 结束旧动画，防止叠加
        transform.DOKill();

        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        float distance = (targetPostion - startPos).magnitude;

        // ===== 可调手感参数 =====
        float liftHeight = Mathf.Clamp(distance * 0.03f, 0f, 0.25f); // 下落前微抬（世界单位）
        float liftDur = Mathf.Min(0.10f, duration * 0.25f);

        // 解压（落地挤压）比例：越大越Q
        Vector3 squashScale = new Vector3(startScale.x * 1.12f, startScale.y * 0.88f, startScale.z * 1.12f);
        float squashDur = 0.07f;
        float reboundDur = 0.12f;

        // 下落时长占比
        float fallDur = Mathf.Max(0.05f, duration - liftDur);

        // ===== 组合动画 =====
        Sequence seq = DOTween.Sequence().SetTarget(transform);

        // 1) 轻微上提（可选：让“松手掉落”更真实）
        if (liftHeight > 0.0001f)
        {
            seq.Append(transform.DOMoveY(startPos.y + liftHeight, liftDur).SetEase(Ease.OutQuad));
        }

        // 2) 下坠到目标点（重力感：InQuad / InCubic）
        seq.Append(transform.DOMove(targetPostion, fallDur).SetEase(Ease.InQuad));

        // 3) 落地“解压”：挤压 -> 回弹（只做 scale，不用透明）
        seq.Append(transform.DOScale(squashScale, squashDur).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(startScale, reboundDur).SetEase(Ease.OutBack, 1.6f));

        // 4) 完成回调
        seq.OnComplete(() =>
        {
            inAnimation = false;
            AnimationEndAction(animationEndActionType);
        });
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
        Transform groupPanel = transform.Find("GroupPanel");
        if (groupPanel != null)
        {
            groupPanel.gameObject.SetActive(false);
        }

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

    public int GetItemId() {
        return Model.GetItemConfig().ID;
    }
}

public enum AnimationEndActionType { 
    NONE,
    DESTORY
}
