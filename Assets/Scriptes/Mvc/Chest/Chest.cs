using FlyEggFrameWork;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : TileItem
{
    int remainCount = 0;
    public override void MountModel(ItemModel itemModel)
    {
        base.MountModel(itemModel);
        ChestConfig chestConfig = ConfigSystem.GetChestConfig(itemModel.GetItemConfig().ID);
        remainCount = chestConfig.ItemCount;
    }

    public void GetTaped() {
        remainCount -= 1;
    }

    public int GetRemainCount() { 
        return remainCount; 
    }

}
