using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishWonderItem : DishNeedItem
{
    [SerializeField]
    private GameObject WhatSymbolObj;
    public void UpdateView(int itemId, Dictionary<int, int> groundItemMap)
    {
        base.UpdateView();

        if (groundItemMap.ContainsKey(itemId) && groundItemMap[itemId] > 0)
        {
            _itemImgPanel.material = null;
            WhatSymbolObj.SetActive(false);
        }
        else {
            WhatSymbolObj.SetActive(true);
        }
    }

}
