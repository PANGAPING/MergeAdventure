using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeAdventureProgressController : GameProgressController
{

    [SerializeField]
    private int _userId;

    private UserData _userData;

    public static MergeAdventureProgressController _instance;

    protected override void InitProgress()
    {
        _instance = this;
        ConfigSystem.LoadConfigs();
        _userData = SaveSystem.GetUserData(_userId);
        base.InitProgress();
    }

    public int GetLevelId() {
        return _userData.CurrentLevelId;
    }
    
}
