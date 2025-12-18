using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DishNeedItem : GameUIPanel
{
    [SerializeField]
    protected Image _itemImgPanel;

    [SerializeField]
    protected GameObject _checkObj;

    protected int _itemId;

    protected bool satisfied = false;

    public virtual void MountItemId(int itemId)
    {
        _itemId = itemId;
        ItemConfig config = ConfigSystem.GetItemConfig(itemId);
        if (config != null)
        {
            _itemImgPanel.sprite = ResourceHelper.GetItemSprite(config);
        }
    }

    public virtual void UpdateView(Dictionary<int,int> groundItemMap)
    {
        base.UpdateView();

        if (groundItemMap.ContainsKey(_itemId) && groundItemMap[_itemId] > 0)
        {
            _checkObj.SetActive(true);
            satisfied = true;
        }
        else {
            _checkObj.SetActive(false);
            satisfied = false;
        }
    }

    public virtual bool IsSatisied() {
        return satisfied;
    }
}
