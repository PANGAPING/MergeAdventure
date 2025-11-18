using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSetting : FlyEggModel
{
    public int Level;

    public int[] StartPos;

    public ItemModel[] Items;


    public MapSetting(int level) {
        Level = level; 
    }
}
