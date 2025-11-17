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
    public static Dictionary<int,MechanismConfig> MechanismConfigs { get; set; }


    public static void LoadConfigs()
    {
        InitItemConfig();
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
        string mapSettingPath = Path.Combine(FoldPath.MapSettingFolderPath, "MapSetting_" + level.ToString());
        MapSetting mapSetting = new MapSetting(level);
        mapSetting.Items = new ItemModel[0];
        mapSetting.Trees = new TreeModel[0];
        mapSetting.Generators = new GeneratorModel[0];
        mapSetting.Mechanisms = new MechanismModel[0];
        mapSetting.StartPos = new int[2] { 0, 0 };

        if (!File.Exists(mapSettingPath))
        {
            File.WriteAllText(mapSettingPath, JsonConvert.SerializeObject(mapSetting));
        }
        else
        {
            string json = File.ReadAllText(mapSettingPath);
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
        string mapSettingPath = Path.Combine(FoldPath.MapSettingFolderPath, "MapSetting_" + level.ToString());

        if (File.Exists(mapSettingPath))
        {
            File.Delete(mapSettingPath);
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






