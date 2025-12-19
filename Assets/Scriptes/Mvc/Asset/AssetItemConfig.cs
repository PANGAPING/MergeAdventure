using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetItemConfig 
{

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ItemID = 0;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int AssetId = 0;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int Num;
}
