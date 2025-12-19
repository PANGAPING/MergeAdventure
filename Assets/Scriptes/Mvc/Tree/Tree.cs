using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : TileItem
{
    private TileItemPanel _bloodSliderPanel = null;

    private TreeConfig _treeConfig = null;

    protected override void InitSelf()
    {
        base.InitSelf();
    }

    public override void MountModel(ItemModel itemModel)
    {
        base.MountModel(itemModel);
        Model.SetIntData(GetMaxBloodCount());
        _treeConfig = ConfigSystem.GetTreeConfig(Model.GetItemConfig().ID);
    }

    public virtual bool GetCut(int count =1) {
        bool die = false;
        int beforeBloodCount = GetNowBloodCount();
        int nowBloodCount = beforeBloodCount - count;

        if (nowBloodCount <= 0) {
            nowBloodCount = 0;
            die = true;
        }
        Model.SetIntData(nowBloodCount);

        if (_bloodSliderPanel == null)
        {
            ShowBloodSlide();
        }
        else { 
            _bloodSliderPanel.UpdateView();
        }

        return die;
    }

    public virtual int GetCutCost() {
        int cutIndex = _treeConfig.MaxHealthCount - GetNowBloodCount();
        return _treeConfig.EnergyCost[cutIndex];
    }

    private void ShowBloodSlide() {
        _bloodSliderPanel = GridUISystem._instance.NewBloodSlider(this);

    }

    public int GetNowBloodCount() {
        return Model.IntData;
    }

    public int GetMaxBloodCount() { 

        if(_treeConfig ==null)
        _treeConfig = ConfigSystem.GetTreeConfig(Model.GetItemConfig().ID);
        return _treeConfig.MaxHealthCount;
    }
    
}
