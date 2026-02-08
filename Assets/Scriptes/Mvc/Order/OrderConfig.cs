using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyEggFrameWork;
using Newtonsoft.Json;

public class OrderConfig : FlyEggConfig
{
    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ID = 0;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int IslandID;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] NeedItemIds;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] NeedItemNums;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] RewardIds;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] RewardNums;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int DefaultUnlock;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] NextOrderIds;
}
