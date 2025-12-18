using DG.Tweening;
using FlyEggFrameWork.Tools;
using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDishPanel : GameUIPanel
{
    protected OrderModel _orderModel;

    [SerializeField]
    protected Image _characterImg;

    [SerializeField]
    protected OrderRewardPanel _rewardPanel;

    [SerializeField]
    protected Transform _dishItemContain;

    protected List<DishNeedItem> _dishNeedItems;

    [SerializeField]
    protected GameUIButton _serveButton;

    public virtual void MountOrderModel(OrderModel orderModel) {
        _orderModel = orderModel;
        _rewardPanel.Mount(orderModel);

        _dishNeedItems = new List<DishNeedItem>();
        CommonTool.DeleteAllChildren(_dishItemContain);
        GameObject orderDishPrefab = ResourceHelper.GetUIPrefab("DishNeedItem");
        if (orderModel.IsLevelTarget) {
            orderDishPrefab = ResourceHelper.GetUIPrefab("DishWonderItem");
        }

        for (int i = 0; i < orderModel.NeedItemId.Length; i++) {
            GameObject dishItemObj = GameObject.Instantiate(orderDishPrefab, _dishItemContain);
            DishNeedItem dishNeedItem;

            if (orderModel.IsLevelTarget)
            {
                dishNeedItem = (DishNeedItem)dishItemObj.GetComponent<DishWonderItem>();
            }
            else
            {
                dishNeedItem = dishItemObj.GetComponent<DishNeedItem>();
            }
            dishNeedItem.MountItemId(orderModel.NeedItemId[i]);
            _dishNeedItems.Add(dishNeedItem);
        }

        _serveButton._onClick += () =>
        {
            _serveButton.gameObject.SetActive(false);
            OrderSystem._instance.FinishOrder(_orderModel);
        };
    }
    public virtual OrderModel GetOrderModel() {
        return _orderModel;
    }

    public void UpdateView(Dictionary<int, int> groundItemMap)
    {
        base.UpdateView();

        bool satisfied = true;

        foreach (DishNeedItem needItem in _dishNeedItems) {
            needItem.UpdateView(groundItemMap);
            if (!needItem.IsSatisied()) {
                satisfied = false;
            }
        }

        _serveButton.gameObject.SetActive(satisfied);
    }

    public Vector3 GetDishFlytarget() {
        return transform.position;
    }


    public void FinishAnimation(
                float popUpY = 40f,          // 向上弹高度
        float popDur = 0.22f,
        Vector2 squashScale = default, // 挤压比例 (x > 1, y < 1)
        float squashDur = 0.08f,

        float stretchDur = 0.1f,     // 回弹时间
        float downY = 350f,           // 向下离场距离
        float downDur = 0.28f
   )
    {
        RectTransform rect = transform.GetComponent<RectTransform>();
        if (rect == null) return;

        rect.DOKill();

        // 默认挤压比例
        if (squashScale == default)
            squashScale = new Vector2(1.08f, 0.92f);

        Vector2 startPos = rect.anchoredPosition;
        Vector3 startScale = rect.localScale;

        Sequence seq = DOTween.Sequence()
            .SetUpdate(true) // UI 对话通常不受 TimeScale 影响
            .SetTarget(rect);

        seq.AppendInterval(0.8f);


        // 1️⃣ 向上弹（位移）
        seq.Append(
            rect.DOAnchorPosY(startPos.y + popUpY, popDur)
                .SetEase(Ease.OutQuad)
        );

        // 2️⃣ 解压弹动（挤压）
        seq.Append(
            rect.DOScale(
                new Vector3(
                    startScale.x * squashScale.x,
                    startScale.y * squashScale.y,
                    1f
                ),
                squashDur
            ).SetEase(Ease.OutQuad)
        );

        // 3️⃣ 回弹恢复（Stretch back）
        seq.Append(
            rect.DOScale(startScale, stretchDur)
                .SetEase(Ease.OutBack, 1.6f)
        );

        // 4️⃣ 向下离场（重力感）
        seq.Append(
            rect.DOAnchorPosY(startPos.y - downY, downDur)
                .SetEase(Ease.InCubic)
        );

        // 5️⃣ 完成后消耗
        seq.OnComplete(() =>
        {
            AnimationEndAction(AnimationEndActionType.DESTORY);
        });
    }
}
