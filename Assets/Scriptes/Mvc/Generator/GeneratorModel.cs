using FlyEggFrameWork;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorModel : ItemModel
{
    [JsonIgnore]
    public GeneratorConfig Config { get; set; }

    public int ConfigID;

    public int HealthCount;

    public GeneratorModel()
    {
        if (Config == null && ConfigID != 00)
        {
            Config = ConfigSystem.GetGeneratorConfig(ConfigID);
        }
    }

    public GeneratorModel(int configID, int[] tilePos)
    {
        ConfigID = configID;
        TilePos = tilePos;
    }

    public GeneratorModel(GeneratorConfig config, int[] tilePos)
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
    public GeneratorConfig GetConfig()
    {
        if (Config == null)
        {
            Config = ConfigSystem.GetGeneratorConfig(ConfigID);
        }

        return Config;
    }
}
