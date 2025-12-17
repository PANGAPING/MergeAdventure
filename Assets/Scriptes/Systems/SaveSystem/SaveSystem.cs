using FlyEggFrameWork;
using FlyEggFrameWork.GameGlobalConfig;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public static class SaveSystem 
{
    public static void SaveUserData(UserData userData)
    {
        string userDataPath = Path.Combine(FoldPath.UserDataFolderPath, "User_" + userData.ToString());

        if (File.Exists(userDataPath))
        {
            File.Delete(userDataPath);
        }

        File.WriteAllText(userDataPath, JsonConvert.SerializeObject(userData));

    }

    public static UserData GetUserData(int userId = 0) {

        string userDataPath = Path.Combine(FoldPath.UserDataFolderPath, "User_" + userId.ToString());
        UserData userData = new UserData(userId);

        if (!File.Exists(userDataPath))
        {
            userData = NewUserData(userId);
        }
        else
        {
            string json = File.ReadAllText(userDataPath);
            userData = JsonConvert.DeserializeObject<UserData>(json);
        }
        return userData;
    }

    public static UserData NewUserData(int userId = 0) { 
        UserData userData = new UserData(userId);

        userData.CurrentLevelId = 1;
        userData.MapDatas = new MapSetting[1];
        userData.MapDatas[0] = ConfigSystem.GetMapSetting(userData.CurrentLevelId);

        AssetConfig[] assetConfigs = ConfigSystem.GetAssetConfigs();
        userData.AssetDatas = new AssetData[assetConfigs.Length];

        for (int i = 0; i < assetConfigs.Length; i++) {
            AssetData assetData = new AssetData(assetConfigs[i].ID, 0);
            if (assetConfigs[i].Name == "Energy") {
                assetData.AssetNum = 80;
            }
            userData.AssetDatas[i] = assetData;
        }

        return userData;
    }


}
