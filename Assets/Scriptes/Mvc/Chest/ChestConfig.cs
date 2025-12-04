using FlyEggFrameWork;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestConfig :  FlyEggConfig
{
    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ID = 0;

    public string Name;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ItemID = 0;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] DropItemIds;

    [JsonConverter(typeof(FloatArrayJsonConverter))]
    public float[] DropItemRatios;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ItemCount;

}
