using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class OrderModel : FlyEggModel
{
    public int GuestId;

    public bool IsLevelTarget;

    public int[] NeedItemId;

    public int[] NeedItemNum;

    public int[] RewardItemType;

    public int[] RewardItemId;

    public int[] RewardItemNum;

    public int[] NextOrderIds;

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

    public static OrderModel FromConfig(OrderConfig orderConfig)
    {
        OrderModel orderModel = new OrderModel();

        orderModel.IsLevelTarget = false;
        orderModel.NeedItemId = orderConfig.NeedItemIds;
        orderModel.NeedItemNum = orderConfig.NeedItemNums;
        orderModel.RewardItemType = orderConfig.RewardIds;
        orderModel.RewardItemNum = orderConfig.RewardNums;
        orderModel.NextOrderIds = orderConfig.NextOrderIds;
        return orderModel;

    }
}
