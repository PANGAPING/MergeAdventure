using FlyEggFrameWork;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemConfig : FlyEggConfig
{
    [JsonConverter(typeof(IntegerJsonConverter))]
    public int ID = 0;

    public string Name;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] ItemBaseSize;

    public string SpritePath;

    public string PrefabPath;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int NextLevelID = 0;

    public ItemType Type;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int Movable = 0;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int Obstacle= 0;

    [JsonConverter(typeof(IntegerJsonConverter))]
    public int Layer = 0;

    [JsonConverter(typeof(IntegerArrayJsonConverter))]
    public int[] IntData;
}
public enum ItemType
{
    [StringValue("FOG")]
    FOG,
    [StringValue("CHAIN")]
    CHAIN,
    [StringValue("STACK")]
    STACK,
    [StringValue("ELFCLOUD")]
    ELFCLOUD,
    [StringValue("ELF")]
    ELF,
    [StringValue("NORMAL")]
    NORMAL,
    [StringValue("GENERATOR")]
    GENERATOR,
    [StringValue("TREE")]
    TREE,
    [StringValue("MAPPAINTER")]
    MAPPAINTER,
    [StringValue("DIEDTILE")]
    DIEDTILE,
    [StringValue("CHEST")]
    CHEST,
    [StringValue("ASSET")]
    ASSET,
    [StringValue("WONDERSKETCH")]
    WONDERSKETCH
}


