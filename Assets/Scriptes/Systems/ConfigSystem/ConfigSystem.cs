using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using FlyEggFrameWork.GameGlobalConfig;
using System;
using FlyEggFrameWork.Tools;
using System.IO;
using System.Linq;

public static class ConfigSystem
{
    public static Dictionary<int, ItemConfig> ItemConfigs { get; set; }
    public static Dictionary<int,TreeConfig> TreeConfigs { get; set; }
    public static Dictionary<int,GeneratorConfig> GeneratorConfigs { get; set; }
    public static Dictionary<int,ChestConfig> ChestConfigs { get; set; }
    public static Dictionary<int,MechanismConfig> MechanismConfigs { get; set; }
    public static Dictionary<int,AssetConfig> AssetConfigs{ get; set; }
    public static Dictionary<int,AssetItemConfig> AssetItemConfigs{ get; set; }


    public static void LoadConfigs()
    {
        InitItemConfig();
        InitTreeConfig();
        InitGeneratorConfig();
        InitMechanismConfig();
        InitChestConfig();
        InitAssetConfig();
        InitAssetItemConfig();
    }

    private static void InitItemConfig()
    {
        ItemConfigs = new Dictionary<int, ItemConfig>();

        ItemConfig[] itemConfigs = LoadJsonConfigArray<ItemConfig>(ConfigPath.ItemConfig);

        for (int i = 0; i < itemConfigs.Length; i++)
        {
            ItemConfig itemConfig = itemConfigs[i];
            ItemConfigs[itemConfig.ID] = itemConfig;
        }
    }

    public static ItemConfig GetItemConfig(int itemID)
    {
        try
        {
            return ItemConfigs[itemID];
        }
        catch (Exception e)
        {
            Debug.LogError("There is no item id:" + itemID.ToString());
            return null;
        }
    }
    public static List<ItemConfig> GetItemConfigs() {

        return ItemConfigs.Values.ToList();
    }

    public static List<ItemConfig> GetItemConfigs(ItemType itemType) {

        return ItemConfigs.Values.ToList().FindAll(x => x.Type == itemType);
    }

    private static void InitTreeConfig()
    {
        TreeConfigs = new Dictionary<int, TreeConfig>();

        TreeConfig[] itemConfigs = LoadJsonConfigArray<TreeConfig>(ConfigPath.TreeConfig);

        for (int i = 0; i < itemConfigs.Length; i++)
        {
            TreeConfig itemConfig = itemConfigs[i];
            TreeConfigs[itemConfig.ID] = itemConfig;
        }
    }

    public static TreeConfig GetTreeConfig(int itemID)
    {
        try
        {
            return TreeConfigs[itemID];
        }
        catch (Exception e)
        {
            Debug.LogError("There is no item id:" + itemID.ToString());
            return null;
        }
    }
    public static ChestConfig GetChestConfig(int itemID)
    {
        try
        {
            return ChestConfigs[itemID];
        }
        catch (Exception e)
        {
            Debug.LogError("There is no chest id:" + itemID.ToString());
            return null;
        }
    }

    

    public static AssetConfig GetAssetConfig(int assetID) {
        try
        {
            return AssetConfigs[assetID];
        }
        catch (Exception e)
        {
            Debug.LogError("There is no asset id:" + assetID.ToString());
            return null;
        }
    }
    public static AssetConfig[] GetAssetConfigs()
    {
        return AssetConfigs.Values.ToArray();
    }

    private static void InitGeneratorConfig()
    {
        GeneratorConfigs = new Dictionary<int, GeneratorConfig>();

        GeneratorConfig[] itemConfigs = LoadJsonConfigArray<GeneratorConfig>(ConfigPath.GeneratorConfig);

        for (int i = 0; i < itemConfigs.Length; i++)
        {
            GeneratorConfig itemConfig = itemConfigs[i];
            GeneratorConfigs[itemConfig.ID] = itemConfig;
        }
    }
    private static void InitChestConfig()
    {
        ChestConfigs = new Dictionary<int, ChestConfig>();

        ChestConfig[] chestConfigs = LoadJsonConfigArray<ChestConfig>(ConfigPath.ChestConfig);

        for (int i = 0; i < chestConfigs.Length; i++)
        {
            ChestConfig chestConfig = chestConfigs[i];
            ChestConfigs[chestConfig.ID] = chestConfig;
        }
    }

    private static void InitAssetConfig() {
        AssetConfigs = new Dictionary<int,AssetConfig>();

        AssetConfig[] assetConfigs = LoadJsonConfigArray<AssetConfig>(ConfigPath.AssetConfig);

        for (int i = 0; i < assetConfigs.Length; i++)
        {
            AssetConfig assetConfig= assetConfigs[i];
            AssetConfigs[assetConfig.ID] = assetConfig;
        }
    }

    private static void InitAssetItemConfig() { 
         AssetItemConfigs = new Dictionary<int,AssetItemConfig>();

        AssetItemConfig[] assetItemConfigs = LoadJsonConfigArray<AssetItemConfig>(ConfigPath.AssetItemConfig);

        for (int i = 0; i < assetItemConfigs.Length; i++)
        {
            AssetItemConfig assetItemConfig= assetItemConfigs[i];
            AssetItemConfigs[assetItemConfig.ItemID] = assetItemConfig;
        } 
    }

    public static AssetItemConfig GetAssetItemConfig(int itemId)
    {
        try
        {
            return AssetItemConfigs[itemId];
        }
        catch (Exception e)
        {
            Debug.LogError("There is no itemAssetConfig id:" + itemId.ToString());
            return null;
        }
    }

    public static GeneratorConfig GetGeneratorConfig(int itemID)
    {
        try
        {
            return GeneratorConfigs[itemID];
        }
        catch (Exception e)
        {
            Debug.LogError("There is no item id:" + itemID.ToString());
            return null;
        }
    }
    private static void InitMechanismConfig()
    {
        MechanismConfigs = new Dictionary<int, MechanismConfig>();

        MechanismConfig[] itemConfigs = LoadJsonConfigArray<MechanismConfig>(ConfigPath.MechanismConfig);

        for (int i = 0; i < itemConfigs.Length; i++)
        {
            MechanismConfig itemConfig = itemConfigs[i];
            MechanismConfigs[itemConfig.ID] = itemConfig;
        }
    }

    public static MechanismConfig GetMechanismConfig(int itemID)
    {
        try
        {
            return MechanismConfigs[itemID];
        }
        catch (Exception e)
        {
            Debug.LogError("There is no item id:" + itemID.ToString());
            return null;
        }
    }


    public static MapSetting GetMapSetting(int level = 1)
    {

        string mapSettingPath = Path.Combine(FoldPath.ResourcesFolderPath, FoldPath.MapSettingFolderPath, "MapSetting_" + level.ToString() +".json");
        MapSetting mapSetting = new MapSetting(level);
        mapSetting.Items = new ItemModel[0];
        mapSetting.StartPos = new int[2] { 0, 0 };

        if (!File.Exists(mapSettingPath))
        {
            File.WriteAllText(mapSettingPath, JsonConvert.SerializeObject(mapSetting));
        }
        else
        {
            string json = Resources.Load<TextAsset>(Path.Combine(FoldPath.MapSettingFolderPath, "MapSetting_" + level.ToString())).text;
            mapSetting = JsonConvert.DeserializeObject<MapSetting>(json);
            if (mapSetting.StartPos == null || mapSetting.StartPos.Length != 2)
            {
                mapSetting.StartPos = new int[2] { 0, 0 };
            }
        }
        return mapSetting;
    }

    public static void SaveMapSetting(int level = 1, MapSetting mapSetting = null)
    {
        string mapSettingPath = Path.Combine(FoldPath.ResourcesFolderPath, FoldPath.MapSettingFolderPath, "MapSetting_" + level.ToString()+".json");

        if (File.Exists(mapSettingPath))
        {
            File.Delete(mapSettingPath);
            Debug.Log("Delete");
        }

        File.WriteAllText(mapSettingPath, JsonConvert.SerializeObject(mapSetting));
    }

    public static string GetConfigJsonText(string path)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        string jsonStr = textAsset.text;
        return jsonStr;
    }

    public static T[] LoadJsonConfigArray<T>(string path)
    {
        string jsonStr = GetConfigJsonText(path);
        return JsonConvert.DeserializeObject<T[]>(jsonStr);
    }

}


//Converter...
public class IntegerJsonConverter : JsonConverter
{

    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return Convert.ToInt32(reader.Value);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(Convert.ToInt32(value));
    }
}



public class IntegerArrayJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        int[] integerArray = new int[0];

        if (reader.Value != null)
        {
            string integerString = reader.Value.ToString();
            integerArray = JsonConvert.DeserializeObject<int[]>(integerString);
        }

        return integerArray;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(JsonConvert.SerializeObject(value));
    }
}

public class IntegerArray2JsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        int[,] integerArray = new int[0, 0];
        if (reader.Value != null)
        {
            string integerString = reader.Value.ToString();
            integerArray = JsonConvert.DeserializeObject<int[,]>(integerString);
        }


        return integerArray;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(JsonConvert.SerializeObject(value));
    }
}

public class IntegerArray3JsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        int[,,] integerArray = new int[0, 0, 0];
        if (reader.Value != null)
        {
            string integerString = reader.Value.ToString();
            integerArray = JsonConvert.DeserializeObject<int[,,]>(integerString);
        }
        return integerArray;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(JsonConvert.SerializeObject(value));
    }

}

public class FloatArrayJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(string);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {

        string floatString = reader.Value.ToString();

        float[] floatArray = JsonConvert.DeserializeObject<float[]>(floatString);

        return floatArray;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteValue(JsonConvert.SerializeObject(value));
    }


}






