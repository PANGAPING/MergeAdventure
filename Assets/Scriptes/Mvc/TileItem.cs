using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItem : FlyEggInstance
{
    public ItemModel Model;

    public virtual void MountModel(ItemModel itemModel)
    {
        Model =itemModel;
    }
}
