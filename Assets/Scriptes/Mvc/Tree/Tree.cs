using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : TileItem
{
    private TileItemPanel _bloodSliderPanel = null;

    protected override void InitSelf()
    {
        base.InitSelf();
    }

    public override void MountModel(ItemModel itemModel)
    {
        base.MountModel(itemModel);
        Model.SetIntData(GetMaxBloodCount());
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

    private void ShowBloodSlide() {
        _bloodSliderPanel = GridUISystem._instance.NewBloodSlider(this);

    }

    public int GetNowBloodCount() {
        return Model.IntData;
    }

    public int GetMaxBloodCount() { 
        TreeConfig treeConfig = ConfigSystem.GetTreeConfig(Model.GetItemConfig().ID);
        return treeConfig.MaxHealthCount;
    }
    
}
