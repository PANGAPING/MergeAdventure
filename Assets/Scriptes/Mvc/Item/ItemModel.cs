using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemModel : MonoBehaviour
{
    [JsonIgnore]
    public ItemConfig Config { get; set; }

    public int ConfigID;

    public int[] TilePos;

    public ItemModel() {
        if (Config == null && ConfigID != 0) {
            Config = ConfigSystem.GetItemConfig(ConfigID);
        }
    }

    public ItemModel(int configID, int[] tilePos) { 
        ConfigID = configID;
        TilePos = tilePos;
    }

    public ItemModel(ItemConfig config, int[] tilePos) {
        Config = config;
        ConfigID = Config.ID;
        TilePos = tilePos;
    }

    public ItemConfig GetConfig() {
        if (Config == null) {
            Config = ConfigSystem.GetItemConfig(ConfigID);
        }
        return Config;
    }

    public ItemType GetItemType()
    {
        return GetConfig().Type;
    }
}
