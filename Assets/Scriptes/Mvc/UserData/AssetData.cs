using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetData :  FlyEggModel
{
    public int AssetId;

    public int AssetNum;

    public AssetData(int id, int num) {
        AssetId = id;
        AssetNum = num;
    }
}
