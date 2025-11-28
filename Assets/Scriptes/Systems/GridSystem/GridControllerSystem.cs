using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlyEggFrameWork;
using System.IO;
using FlyEggFrameWork.GameGlobalConfig;
using UnityEngine.UI;
using System;
using FlyEggFrameWork.Tools;
using UnityEditor;

public class GridControllerSystem : GameSystem
{
    [Header("Node")]
    [Space]
    protected Transform WorldNode;

    [Header("Helper")]
    [Space]
    protected GridHelper _gridHelper;

    public Dictionary<ItemType, Transform> NodeMap;

    [Header("LevelData")]
    private MapSetting MapSetting;

    //item instances
    private Dictionary<ItemType, Dictionary<Vector2Int, TileItem>> itemMap;

    private GameObject TileCursor;

    private Vector2Int activeTilePos;

    private TileBase activeTileBase;

    private static string[] ItemTypesString = new string[] { };

    private static ItemType[] ItemTypes = new ItemType[] { };

    //Tap Section
    private Vector2Int tapDownPos = new Vector2Int(0, 0);

    private bool dragging = false;

    private bool tapDowning = false;

    private TileItem draggingItem = null;



    protected override void InitSelf()
    {
        base.InitSelf();
        ConfigSystem.LoadConfigs();

        ItemTypesString = new string[Enum.GetNames(typeof(ItemType)).Length];
        ItemTypes = (ItemType[])Enum.GetValues(typeof(ItemType));
        for (int i = 0; i < ItemTypes.Length; i++)
        {
            ItemTypesString[i] = EnumUtils.GetStringValue(ItemTypes[i]);
        }

        GameObject worldNodeObj = GameObject.Find("Canvas");
        WorldNode = worldNodeObj.transform;

        GameObject boardNodeObj = GameObject.Find("Canvas/GameBoard");
        if (boardNodeObj != null)
        {
            DestroyImmediate(boardNodeObj);
        }

        GameObject boardPrefab = Resources.Load<GameObject>(Path.Combine(FoldPath.PrefabFolderPath, "GameBoard"));
        GameObject boardObj = GameObject.Instantiate(boardPrefab, WorldNode);
        boardPrefab.name = "GameBoard";
        _gridHelper = new GridHelper(boardObj.GetComponent<GridLayoutGroup>());

        InitTileCursor();
        LoadMap();
        RefreshMap();
    }


    protected override void Init()
    {
        base.Init();
        GamepadSystem._instance._onClickDown += OnTapDown;
        GamepadSystem._instance._onClickUp += OnTapUp;
    }


    protected override void Update()
    {
        base.Update();

        UpdateTileCursor(Input.mousePosition);

        UpdateDraggingItem();

    }

    private void InitTileCursor()
    {
        //Init tile Cursor.
        string tileCursorPath = Path.Combine(FoldPath.PrefabFolderPath, "Tiles", "TileCursor", "TileCursor");
        GameObject tileCursorPrefab = Resources.Load<GameObject>(tileCursorPath);
        TileCursor = GameObject.Instantiate(tileCursorPrefab);
        TileCursor.transform.SetParent(WorldNode);
        TileCursor.transform.localPosition = Vector3.zero;
        TileCursor.transform.localRotation = Quaternion.identity;
    }

    private void UpdateTileCursor(Vector3 mousePosition)
    {
        var mousePos = mousePosition;

        activeTilePos = _gridHelper.GetGridPosition(mousePos);
        GameObject activeTileObj = _gridHelper.GetCellAtPosition(activeTilePos);

        activeTileBase = activeTileObj != null ? activeTileObj.GetComponent<TileBase>() : null;

        if (activeTileBase == null)
        {
            TileCursor.SetActive(false);
            return;
        }

        TileCursor.SetActive(true);
        Vector2 tileCenterPosition = _gridHelper.GetCellWorldPosition(activeTilePos);
        TileCursor.transform.position = tileCenterPosition;
        TileCursor.transform.rotation = Quaternion.identity;
    }

    private void UpdateDraggingItem() {
        if (!_gridHelper.IsValidTilePos(activeTilePos)) {
            return;
        }

        if (!dragging && tapDowning)
        {
            if (tapDownPos != activeTilePos && activeTileBase != null)
            {
                StartDragTileItem(activeTileBase);
            }
        }
        else if (dragging) {
            Vector3 justifyedPosition = _gridHelper.GetCellWorldPosition(activeTilePos);
            Vector3 beforePosition = draggingItem.transform.position;
            draggingItem.transform.position = Vector3.Lerp(beforePosition, justifyedPosition, Time.deltaTime * 10);
        }
    }


    private void LoadMap() {
        MapSetting = ConfigSystem.GetMapSetting(MergeAdventureProgressController._instance.GetLevel());

        NodeMap = new Dictionary<ItemType, Transform>();
        itemMap = new Dictionary<ItemType, Dictionary<Vector2Int, TileItem>>();

        foreach (ItemType itemType in ItemTypes)
        {
            string itemTypeString = EnumUtils.GetStringValue(itemType);
            Transform node = new GameObject(itemTypeString).transform;
            node.SetParent(WorldNode);
            NodeMap.Add(itemType, node);
        }

        foreach (ItemType itemType in ItemTypes)
        {
            itemMap.Add(itemType, new Dictionary<Vector2Int, TileItem>());
        }

        for (int i = 0; i < MapSetting.Items.Length; i++)
        {
            ItemModel itemModel = MapSetting.Items[i];
            MountTileItem(itemModel);
        }
    }

    private void RefreshMap() {

        _gridHelper.RefreshTilesState(new Vector2Int(3, 0));
        _gridHelper.RefreshTiles();
    }

    private void MountTileItem(ItemModel itemModel)
    {

        ItemConfig itemConfig = ConfigSystem.GetItemConfig(itemModel.ItemConfigID);

        GameObject itemPrefab = ResourceHelper.GetItemPrefab(itemConfig);

        GameObject itemObject = GameObject.Instantiate(itemPrefab, NodeMap[itemConfig.Type]);

        TileItem item = itemObject.GetComponent<TileItem>();

        item.MountModel(itemModel);

        Vector2Int tilePos = CommonTool.ArrayToVector2Int(itemModel.TilePos);
        Vector2Int[] tilePoses = new Vector2Int[0];

        if (itemModel.TilePoses != null)
        {
            tilePoses = CommonTool.Array2ToVector2IntArray(itemModel.TilePoses);
        }


        itemMap[itemConfig.Type][tilePos] = item;

        if (tilePoses.Length > 0)
        {
            _gridHelper.PutObjectOnTile(itemObject, tilePoses, 0);
        }
        else
        {
            _gridHelper.PutObjectOnTile(itemObject, tilePos, 0);
        }
        TileBase tileBase = _gridHelper.GetTileBase(tilePos);
        tileBase.OccupyItem(item);
    }

    protected virtual void OnTapDown()
    {
        tapDownPos = activeTilePos;
        dragging = false;
        tapDowning = true;
    }

    protected virtual void OnTapUp()
    {
        Vector2Int tapUpPos = activeTilePos;

        if (tapUpPos == tapDownPos && !dragging)
        {
            if (activeTileBase != null) { 
                TapOnTile(activeTileBase);
            }
        }
        else if (dragging) {
            EndDragItem();
        }

        tapDowning = false;
    }


    private void TapOnTile(TileBase tileBase)
    {
        Debug.Log(activeTilePos);
        TileItem tileItem = tileBase.GetLayerTopItem();
        if (tileItem == null) {
            return;
        }



    }

    private void StartDragTileItem(TileBase tileBase) { 
         TileItem tileItem = tileBase.GetLayerTopItem();
        if (tileItem == null && !activeTileBase.IsDragable())
        {
            return;
        }
        dragging = true;

        draggingItem = tileItem;
    }

    private void EndDragItem() {
        dragging = false;
    }
}
