using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyEggFrameWork;
using Newtonsoft.Json;

public class MechanismConfig :  FlyEggConfig
{
    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ID = 0;

    public string Name;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ItemID = 0;

    public ItemType Type;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] IntData;
}
