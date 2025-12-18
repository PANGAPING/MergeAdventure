using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishWonderItem : DishNeedItem
{
    [SerializeField]
    private GameObject WhatSymbolObj;
    public override void UpdateView(Dictionary<int, int> groundItemMap)
    {
        base.UpdateView(groundItemMap);

        if (groundItemMap.ContainsKey(_itemId) && groundItemMap[_itemId] > 0)
        {
            _itemImgPanel.material = null;
            WhatSymbolObj.SetActive(false);
        }
        else {
            WhatSymbolObj.SetActive(true);
        }
    }

}
