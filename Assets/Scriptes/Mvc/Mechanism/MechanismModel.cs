using FlyEggFrameWork;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanismModel : ItemModel
{
    [JsonIgnore]
    public MechanismConfig Config { get; set; }

    public int ConfigID;

    public MechanismModel()
    {
        if (Config == null && ConfigID != 00)
        {
            Config = ConfigSystem.GetMechanismConfig(ConfigID);
        }
    }

    public MechanismModel(int configID, int[] tilePos)
    {
        ConfigID = configID;
        TilePos = tilePos;
    }

    public MechanismModel(MechanismConfig config, int[] tilePos)
    {
        Config = config;
        ConfigID = config.ID;
        TilePos = tilePos;
    }
    public MechanismConfig GetConfig()
    {
        if (Config == null)
        {
            Config = ConfigSystem.GetMechanismConfig(ConfigID);
        }

        return Config;
    }
}
