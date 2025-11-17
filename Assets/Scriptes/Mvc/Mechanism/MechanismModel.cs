using FlyEggFrameWork;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanismModel : FlyEggModel
{
    [JsonIgnore]
    public MechanismConfig Config { get; set; }

    [JsonIgnore]
    public ItemConfig ItemConfig { get; set; }

    public int ConfigID;

    public int[] TilePos;

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
    public ItemConfig GetItemConfig()
    {
        if (ItemConfig == null)
        {
            ItemConfig = ConfigSystem.GetItemConfig(ConfigID);
        }
        return ItemConfig;
    }
}
