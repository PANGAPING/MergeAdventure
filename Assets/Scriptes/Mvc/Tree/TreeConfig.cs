using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyEggFrameWork;
using Newtonsoft.Json;

public class TreeConfig : FlyEggConfig
{
    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ID = 0;

    public string Name;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ItemID = 0;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int DropItemCount;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] DropItemIds;

    [JsonConverter(typeof(FloatArrayJsonConverter))]
    public float[] DropItemRatios;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int MaxHealthCount;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] EnergyCost;
}

