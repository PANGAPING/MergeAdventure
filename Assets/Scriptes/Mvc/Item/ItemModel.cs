using FlyEggFrameWork;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class ItemModel : FlyEggModel
{
    [JsonIgnore]
    public ItemConfig ItemConfig { get; set; }

    public int ItemConfigID;

    public int[] TilePos;

    public int[,] TilePoses;


    public ItemModel()
    {
        if (ItemConfig == null &&  ItemConfigID!= 0)
        {
            ItemConfig = ConfigSystem.GetItemConfig(ItemConfigID);
        }
    }

    public ItemModel(int itemConfigID, int[] tilePos) { 
        ItemConfigID = itemConfigID;
        TilePos = tilePos;
        TilePoses = new int[0, 0];
    }

    public ItemModel(ItemConfig config, int[] tilePos) {
        ItemConfig = config;
        ItemConfigID = config.ID;
        TilePos = tilePos;
        TilePoses = new int[0, 0];
    }

    public ItemModel(ItemConfig config, int[] tilePos, int[,] tilePoses)
    {
        ItemConfig = config;
        ItemConfigID = config.ID;
        TilePos = tilePos;
        TilePoses = tilePoses;
    }

    public ItemConfig GetItemConfig() {
        if (ItemConfig == null) {
            ItemConfig = ConfigSystem.GetItemConfig(ItemConfigID);
        }
        return ItemConfig;
    }

    public ItemType GetItemType()
    {
        return GetItemConfig().Type;
    }
}
