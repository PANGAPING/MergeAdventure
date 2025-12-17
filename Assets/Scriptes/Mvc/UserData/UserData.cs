using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class UserData :  FlyEggModel
{
    public int UserId;

    public int CurrentLevelId;

    public MapSetting[] MapDatas;

    public AssetData[] AssetDatas;

    public OrderModel[] OrderDatas;
    public UserData(int userId){
        UserId = userId;
        CurrentLevelId = 1;
        MapDatas = new MapSetting[0];
        AssetDatas = new AssetData[0];
        OrderDatas = new OrderModel[0];
    }
}
