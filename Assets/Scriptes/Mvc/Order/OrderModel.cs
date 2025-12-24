using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderModel : FlyEggModel
{
    public int GuestId;

    public bool IsLevelTarget;

    public int[] NeedItemId;

    public int[] NeedItemNum;

    public int[] RewardItemType;

    public int[] RewardItemId;

    public int[] RewardItemNum;

    public void AddNeedItem(int itemId, int count) {
        List<int> newItemId = NeedItemId.ToList();
        List<int> newItemNum =NeedItemNum.ToList();

        if (newItemId.Contains(itemId))
        {
            newItemNum[newItemId.IndexOf(itemId)] += count;
        }
        else {
            newItemId.Add(itemId); 
            newItemNum.Add(count); 
        }
        NeedItemId = newItemId.ToArray();
        NeedItemNum = newItemNum.ToArray();
    }
}
