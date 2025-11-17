using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyEggFrameWork;
using Newtonsoft.Json;

public class TreeModel : ItemModel
{
    [JsonIgnore]
    public TreeConfig Config { get; set; }

    public int ConfigID;

    public int HealthCount;

    public TreeModel()
    {
        if (Config == null && ConfigID != 00)
        {
            Config = ConfigSystem.GetTreeConfig(ConfigID);
        }
    }

    public TreeModel(int configID, int[] tilePos)
    {
        ConfigID = configID;
        TilePos = tilePos;
    }

    public TreeModel(TreeConfig config, int[] tilePos)
    {
        Config = config;
        ConfigID = config.ID;
        TilePos = tilePos;
        HealthCount = config.MaxHealthCount;
    }

    public void SetHealthCount(int healthCount)
    {
        HealthCount = healthCount;
    }
    public TreeConfig GetConfig()
    {
        if (Config == null)
        {
            Config = ConfigSystem.GetTreeConfig(ConfigID);
        }
        return Config;
    }
}
