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

    public virtual void UpdateView(int itemId,Dictionary<int,int> groundItemMap)
    {
        base.UpdateView();
    }
}
