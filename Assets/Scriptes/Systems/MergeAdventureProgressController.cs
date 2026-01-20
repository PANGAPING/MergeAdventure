using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MergeAdventureProgressController : GameProgressController
{

    [SerializeField]
    private int _userId;

    private UserData _userData;

    public static MergeAdventureProgressController _instance;

    public const int _testLevel = 101;

    protected override void InitProgress()
    {
        _instance = this;
        ConfigSystem.LoadConfigs();
        _userData = SaveSystem.GetUserData(_userId);
        base.InitProgress();
        GridControllerSystem._instance._onGroundItemChange += SaveProgress;
    }

    public int GetLevelId() {
        if (_testLevel > 0) {
            return _testLevel; 
        }
        return _userData.CurrentLevelId;
    }

    public UserData GetUserData() {
        return _userData;
    }

    public MapSetting GetMapSetting()
    {
        MapSetting usedMapSetting = _userData.MapDatas.ToList().Find(x => x.Level == GetLevelId());
        if (usedMapSetting == null) {
            return ConfigSystem.GetMapSetting(GetLevelId());     
        }
        return usedMapSetting;
    }    

    public void SaveProgress() {
        MapSetting curMapSetting = GridControllerSystem._instance.GetCurMapSetting();

        for (int i = 0; i < _userData.MapDatas.Length; i++) {
            if (_userData.MapDatas[i].Level == curMapSetting.Level) {
                _userData.MapDatas[i] = curMapSetting; 
            }
        }

        SaveSystem.SaveUserData(_userData);
    }
}
