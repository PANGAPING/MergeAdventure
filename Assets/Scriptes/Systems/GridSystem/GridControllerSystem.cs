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
using UnityEditor.Experimental.GraphView;

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
            if (tapDownPos != activeTilePos && _gridHelper.IsValidTilePos(tapDownPos))
            {
                TileBase dragTile = _gridHelper.GetTileBase(tapDownPos);
                if (dragTile != null) { 
                    StartDragTileItem(dragTile);
                }
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

        MountItemMap(item);

        if (tilePoses.Length > 0)
        {
            _gridHelper.PutObjectOnTile(itemObject, tilePoses, 0);
        }
        else
        {
            _gridHelper.PutObjectOnTile(itemObject, tilePos, 0);
        }
    }

    private void MountItemMap(TileItem item) { 
        ItemType itemType = item.GetItemType();
        Vector2Int itemPos = item.GetPos();
        itemMap[itemType][itemPos] = item;
        TileBase tileBase = _gridHelper.GetTileBase(itemPos);
        tileBase.OccupyItem(item);
    }

    private void UnMountItemMap(TileItem item) {
        ItemType itemType = item.GetItemType();
        Vector2Int itemPos = item.GetPos();
        if (itemMap[itemType].ContainsKey(itemPos)) {
            itemMap[itemType].Remove(itemPos);
        }
        TileBase tileBase = _gridHelper.GetTileBase(itemPos);
        tileBase.RemoveOccupyItem(item);
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
        if (_gridHelper.IsValidTilePos(activeTilePos)) {
            Debug.Log(_gridHelper.GetTileState(activeTilePos));
        }
        TileItem tileItem = tileBase.GetLayerTopItem();
        if (tileItem == null) {
            return;
        }



    }

    private void StartDragTileItem(TileBase tileBase) { 
         TileItem tileItem = tileBase.GetLayerTopItem();
        if (!activeTileBase.IsDragable())
        {
            return;
        }
        dragging = true;
        draggingItem = tileItem;
        UnMountItemMap(tileItem);
    }

    private void EndDragItem() {
        if (!_gridHelper.IsValidTilePos(activeTilePos))
        {
            TileBase closetEmptyTile = _gridHelper.GetClosestEmptyWhiteTile(activeTilePos, false);
            draggingItem.SetPos(closetEmptyTile.GetPos());
        }
        else if (!_gridHelper.IsWhiteTilePos(activeTilePos)) {
            TileBase closetEmptyTile = _gridHelper.GetClosestEmptyWhiteTile(activeTilePos, false);
            draggingItem.SetPos(closetEmptyTile.GetPos());
        }
        else if (activeTileBase.GetLayerTopItem() == null)
        {
            draggingItem.SetPos(activeTilePos);
        }
        else if (activeTileBase.GetLayerTopItem() != null)
        {
            TileItem targetTileItem = activeTileBase.GetLayerTopItem();

            if (CheckMergable(draggingItem, targetTileItem)) {
                Merge(draggingItem, targetTileItem);
            }
            else if (targetTileItem.IsMovable())
            {
                draggingItem.SetPos(activeTilePos);

                UnMountItemMap(targetTileItem);
                TileBase closetEmptyTile = _gridHelper.GetClosestEmptyWhiteTile(activeTilePos, false);
                targetTileItem.SetPos(closetEmptyTile.GetPos());
                MountItemMap(targetTileItem);

                targetTileItem.MoveAnimation(_gridHelper.GetCellWorldPosition(closetEmptyTile.GetPos()));
            }
            else
            {
                draggingItem.SetPos(draggingItem.GetPos());
            }
        }

        MountItemMap(draggingItem);
        draggingItem.MoveAnimation(_gridHelper.GetCellWorldPosition(draggingItem.GetPos()));

        dragging = false;
        draggingItem = null;
    }

    private bool CheckMergable(TileItem tileItem1, TileItem tileItem2) {
        
        ItemConfig itemConfig1 = tileItem1.Model.GetItemConfig();
        ItemConfig itemConfig2 = tileItem2.Model.GetItemConfig();
        if (!(itemConfig1.NextLevelID > 0) || !(itemConfig2.NextLevelID > 0)) {
            return false; 
        }
        if (itemConfig1.ID != itemConfig2.ID) { 
            return false;
        }

        return true;
    }

    private void Merge(TileItem tileItem1, TileItem tileItem2,Vector2Int mergePos) {

        int mergeToId = tileItem1.Model.GetItemConfig().NextLevelID;

        UnMountItemMap(tileItem1);
        UnMountItemMap(tileItem2);

        tileItem1.MoveAnimation(_gridHelper.GetCellWorldPosition(mergePos), AnimationEndActionType.DESTORY);
        tileItem2.MoveAnimation(_gridHelper.GetCellWorldPosition(mergePos), AnimationEndActionType.DESTORY);

        Drop(new Dictionary<int, int> { { mergeToId, 1 } }, mergePos);
    }

    private void Drop(Dictionary<int, int> items, Vector2Int dropFromPos) { 
        
    }

    private void DropItem(int itemId,Vector2Int fromPos, Vector2Int toPos) { 
          
    }
}
