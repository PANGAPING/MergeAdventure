using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeAdventureProgressController : GameProgressController
{

    private GridHelper _gridHelper;

    [SerializeField]
    private int _level;

    public static MergeAdventureProgressController _instance;

    protected override void InitProgress()
    {
        _instance = this;
        base.InitProgress();
    }

    public int GetLevel() {
        return _level;
    }
    
}
