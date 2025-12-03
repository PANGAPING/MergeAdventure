using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileItemPanel : GameUIPanel
{
    TileItem TileItem { get; set; }
    public virtual void MountTileItem(TileItem tileItem) {
        tileItem = TileItem;
    }

}
