using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.XR.OpenVR;
using UnityEngine;

public class InventorySystem :  GameSystem
{
    public static InventorySystem _instance;
    public EventHandler  _onInventoryChange;
    public UserData _userData;

    public Dictionary<ASSETTYPE, int> _assetTypeToIDMap = new Dictionary<ASSETTYPE, int>() {
        { ASSETTYPE.ENERGY,1},
        {ASSETTYPE.COIN,2},
        {ASSETTYPE.DIAMOND,3},
        {ASSETTYPE.KEY,101}
    };

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();
        _userData = MergeAdventureProgressController._instance.GetUserData();
    }

    protected override void Init()
    {
        base.Init();
    }

    public bool AddAsset(ASSETTYPE assetType, int count)
    {
        bool suc = true;

        int assetId = _assetTypeToIDMap[assetType];

        List<AssetData> assetDatas = _userData.AssetDatas.ToList();

        if (assetDatas.Exists(x => x.AssetId == assetId))
        {
            assetDatas.Find(x => x.AssetId == assetId).AssetNum += count;
        }
        else
        {
            assetDatas.Add(new AssetData(assetId,count));
        }

        _userData.AssetDatas = assetDatas.ToArray();
        InventoryChangeEvent();
        return suc;
    }
    public bool AddAsset(int assetId, int count)
    {
        ASSETTYPE assetType = ConvertIdToAssetType(assetId);


        return AddAsset(assetType, count);
    }
    public bool RemoveAsset(int assetId, int count)
    {
        ASSETTYPE assetType = ConvertIdToAssetType(assetId);

        return RemoveAsset(assetType, count);
    }

    public bool HaveAsset(int assetId, int count) { 
    
        ASSETTYPE assetType = ConvertIdToAssetType(assetId);;
        return HaveAsset(assetType, count);
    }

    public bool HaveAsset(ASSETTYPE assetType, int count) {

        int assetId = _assetTypeToIDMap[assetType];

        List<AssetData> assetDatas = _userData.AssetDatas.ToList();

        if (assetDatas.Exists(x => x.AssetId == assetId))
        {
            AssetData have = assetDatas.Find(x => x.AssetId == assetId);
            if (have.AssetNum >= count)
            {
                return true;
            }
        }
        return false;
    }



    public bool RemoveAsset(ASSETTYPE assetType, int count)
    {
        bool suc = true;
        int assetId = _assetTypeToIDMap[assetType];

        List<AssetData> assetDatas = _userData.AssetDatas.ToList();

        if (assetDatas.Exists(x => x.AssetId == assetId))
        {
            AssetData have = assetDatas.Find(x => x.AssetId == assetId);
            if (have.AssetNum < count) {
                return false;
            }
            have.AssetNum -= count;
        }
        else
        {
            return false;
        }

        _userData.AssetDatas = assetDatas.ToArray();
        InventoryChangeEvent();
        return suc;
    }

    public int GetAssetNum(ASSETTYPE assetType)
    {
        int assetId = _assetTypeToIDMap[assetType];
        List<AssetData> assetDatas = _userData.AssetDatas.ToList();

        if (assetDatas.Exists(x => x.AssetId == assetId))
        {
            AssetData have = assetDatas.Find(x => x.AssetId == assetId);
            return have.AssetNum;
        }
        else
        {
            return 0;
        }

    }
    private void InventoryChangeEvent() {
        if (_onInventoryChange != null) {
            _onInventoryChange.Invoke(); 
        }
    }

    private ASSETTYPE ConvertIdToAssetType(int assetId)
    {
        ASSETTYPE ass = ASSETTYPE.COIN;
        ass = _assetTypeToIDMap.Keys.ElementAt(_assetTypeToIDMap.Values.ToList().IndexOf(assetId));
        return ass;
    }
}

public enum ASSETTYPE { 
    ENERGY,
    COIN,
    DIAMOND,
    KEY
}
