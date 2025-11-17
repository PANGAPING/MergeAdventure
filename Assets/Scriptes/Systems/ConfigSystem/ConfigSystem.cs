using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using FlyEggFrameWork.GameGlobalConfig;
using System;
using FlyEggFrameWork.Tools;

public static class ConfigSystem
{


    public static void LoadConfigs()
    {
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






