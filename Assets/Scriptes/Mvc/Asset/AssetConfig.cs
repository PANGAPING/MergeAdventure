using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetConfig
{
    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ID = 0;

    public string Name;

    public string SpritePath;
}
