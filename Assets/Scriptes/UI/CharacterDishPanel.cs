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

    public virtual void MountOrderModel(OrderModel orderModel) {
        _orderModel = orderModel;
        UpdateView();
    }

    public override void UpdateView()
    {
        base.UpdateView();
        
    }

}
