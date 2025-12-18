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
                dishNeedItem =(DishNeedItem) dishItemObj.GetComponent<DishWonderItem>();
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
            OrderSystem._instance.FinishOrder(_orderModel);
        };
    }
    public virtual OrderModel GetOrderModel() {
        return _orderModel; 
    }

    public void UpdateView(Dictionary<int,int> groundItemMap)
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

}
