using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishWonderItem : DishNeedItem
{
    [SerializeField]
    private GameObject WhatSymbolObj;
    public override void UpdateView(Dictionary<int, int> groundItemMap,int sameItemIndex = 0)
    {
        base.UpdateView(groundItemMap);

        if (groundItemMap.ContainsKey(_itemId) && groundItemMap[_itemId] > sameItemIndex)
        {
            _itemImgPanel.material = null;
            WhatSymbolObj.SetActive(false);
        }
        else {
            WhatSymbolObj.SetActive(true);
        }

        WhatSymbolObj?.SetActive(false);
    }

}
