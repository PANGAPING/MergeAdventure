using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : TileItem
{
    private GeneratorConfig _generatorConfig = null;

    public override void MountModel(ItemModel itemModel)
    {
        base.MountModel(itemModel);
        Model.SetIntData(GetMaxBloodCount());
        _generatorConfig = ConfigSystem.GetGeneratorConfig(Model.GetItemConfig().ID);
    }
    public virtual bool GetCut(int count = 1)
    {
        bool die = false;
        int beforeBloodCount = GetNowBloodCount();
        int nowBloodCount = beforeBloodCount - count;

        if (nowBloodCount == 0)
        {
            nowBloodCount = 0;
            die = true;
        }
        Model.SetIntData(nowBloodCount);

        return die;
    }


    public int GetNowBloodCount() {
        return Model.IntData;
    }

    public int GetMaxBloodCount() { 

        if(_generatorConfig==null)
        _generatorConfig = ConfigSystem.GetGeneratorConfig(Model.GetItemConfig().ID);
        return _generatorConfig.MaxHealthCount;
    }

}
