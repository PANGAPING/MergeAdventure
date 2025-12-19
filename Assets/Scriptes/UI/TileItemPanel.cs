using FlyEggFrameWork.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileItemPanel : GameUIPanel
{
    protected TileItem _tileItem { get; set; }
    public GameUIButton button = null;
    public override void InitSelf()
    {
        base.InitSelf();
    }

    public override void UpdateView()
    {
        base.UpdateView();
    }

    public virtual void MountTileItem(TileItem tileItem,Action callback = null) {
        _tileItem = tileItem;
        if (button != null) {
            if (callback != null) {
                button._onClick += () => { callback.Invoke();  };
            }
        }

        if (tileItem is Tree) { 
            Tree tree = (Tree)tileItem;
            if (button != null) { 
                button.Mount(tree.GetCutCost().ToString());
            }
        }
    }
    public TileItem GetTileItem() {
        return _tileItem; 
    }
}
