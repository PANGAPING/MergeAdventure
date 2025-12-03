using FlyEggFrameWork.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileItemPanel : GameUIPanel
{
    private TileItem TileItem { get; set; }
    public GameUIButton button = null;
    public override void InitSelf()
    {
        base.InitSelf();
    }
    public virtual void MountTileItem(TileItem tileItem,Action callback) {
        TileItem = tileItem;
        if (button != null) {
            if (callback != null) {
                button._onClick += () => { callback.Invoke();  };
            }
        }
    }
    public TileItem GetTileItem() {
        return TileItem; 
    }
}
