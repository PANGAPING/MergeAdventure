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

    public static GridControllerSystem _instance;

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

    private Vector2Int highlightTilePos;

    private TileBase highlightTileBase;

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
        _instance = this;
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
        RefreshMap(true);
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
        if (GamepadSystem._instance.mouseIsOverUI) {
            return;
        }

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

        highlightTilePos = _gridHelper.GetGridPosition(mousePos);
        GameObject activeTileObj = _gridHelper.GetCellAtPosition(highlightTilePos);

        highlightTileBase = activeTileObj != null ? activeTileObj.GetComponent<TileBase>() : null;

        if (highlightTileBase == null)
        {
            TileCursor.SetActive(false);
            return;
        }

        TileCursor.SetActive(true);
        Vector2 tileCenterPosition = _gridHelper.GetCellWorldPosition(highlightTilePos);
        TileCursor.transform.position = tileCenterPosition;
        TileCursor.transform.rotation = Quaternion.identity;
    }

    private void UpdateDraggingItem() {
        if (!_gridHelper.IsValidTilePos(highlightTilePos)) {
            return;
        }

        if (!dragging && tapDowning)
        {
            if (tapDownPos != highlightTilePos && _gridHelper.IsValidTilePos(tapDownPos))
            {
                TileBase dragTile = _gridHelper.GetTileBase(tapDownPos);
                if (dragTile != null) { 
                    StartDragTileItem(dragTile);
                }
            }
        }
        else if (dragging) {
            Vector3 justifyedPosition = _gridHelper.GetCellWorldPosition(highlightTilePos);
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

    private void RefreshMap(bool init = false) {
        _gridHelper.RefreshTilesState(new Vector2Int(3, 0));
        _gridHelper.RefreshTiles(init);
    }

    private TileItem MountTileItem(ItemModel itemModel)
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

        return item;
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
        if (GamepadSystem._instance.mouseIsOverUI) {
            return;
        }

        tapDownPos = highlightTilePos;
        dragging = false;
        tapDowning = true;
    }

    protected virtual void OnTapUp()
    {
        if (GamepadSystem._instance.mouseIsOverUI) {
            return;
        }

        Vector2Int tapUpPos = highlightTilePos;

        if (tapUpPos == tapDownPos && !dragging)
        {
            if (highlightTileBase != null) { 
                TapOnTile(highlightTileBase);
            }
        }
        else if (dragging) {
            EndDragItem();
        }

        tapDowning = false;
    }


    private void TapOnTile(TileBase tileBase)
    {
        Debug.Log(highlightTilePos);
        if (_gridHelper.IsValidTilePos(highlightTilePos)) {
            Debug.Log(_gridHelper.GetTileState(highlightTilePos));
        }
        TileItem tileItem = tileBase.GetLayerTopItem();
        if (tileItem == null) {
            return;
        }

        TapItem(tileItem);
        activeTileBase = tileBase;
    }

    private void TapItem(TileItem tileItem) {
        tileItem.BounceAnimation();

        ItemType itemType = tileItem.GetItemType();

        if (tileItem.IsActive())
        {
            tileItem.DeActive();
            if (itemType == ItemType.STACK) { 
                GridUISystem._instance.CloseButtonTips(tileItem); 
            }
             
        }
        else {
            tileItem.Active();
            if (itemType == ItemType.STACK) { 
                GridUISystem._instance.OpenButtonTips(tileItem,TryOpenStack); 
            }
        }

        if (itemType== ItemType.GENERATOR) {
            TapGenerator(tileItem);     
        }
    }

    private void TryOpenStack(TileItem tileItem) {
        DestroyTileItem(tileItem);
    }

    private void DestroyTileItem(TileItem tileItem) { 
        UnMountItemMap(tileItem);
        tileItem.Die();
        RefreshMap();
    }

    private void TapGenerator(TileItem tileItem) {
        Generator generator = (Generator)tileItem;
        GeneratorConfig generatorConfig = ConfigSystem.GetGeneratorConfig(tileItem.Model.GetItemConfig().ID);
        Dictionary<int,int> dropMap = CommonTool.GetDropResult(generatorConfig.DropItemIds,generatorConfig.DropItemRatios,1);
        Drop(dropMap, tileItem.GetPos());
    }

    private void StartDragTileItem(TileBase tileBase) { 
         TileItem tileItem = tileBase.GetLayerTopItem();
        if (!tileBase.IsDragable())
        {
            return;
        }
        dragging = true;
        draggingItem = tileItem;
        UnMountItemMap(tileItem);
    }

    private void EndDragItem() {
        if (!_gridHelper.IsValidTilePos(highlightTilePos))
        {
            TileBase closetEmptyTile = _gridHelper.GetClosestEmptyWhiteTile(highlightTilePos, false);
            draggingItem.SetPos(closetEmptyTile.GetPos());
        }
        else if (!_gridHelper.IsWhiteTilePos(highlightTilePos)) {
            TileBase closetEmptyTile = _gridHelper.GetClosestEmptyWhiteTile(highlightTilePos, false);
            draggingItem.SetPos(closetEmptyTile.GetPos());
        }
        else if (highlightTileBase.GetLayerTopItem() == null)
        {
            draggingItem.SetPos(highlightTilePos);
        }
        else if (highlightTileBase.GetLayerTopItem() != null)
        {
            TileItem targetTileItem = highlightTileBase.GetLayerTopItem();

            if (CheckMergable(draggingItem, targetTileItem)) {
                Merge(draggingItem, targetTileItem,highlightTilePos);
                dragging = false;
                draggingItem = null;
                return;
            }
            else if (targetTileItem.IsMovable())
            {
                draggingItem.SetPos(highlightTilePos);

                UnMountItemMap(targetTileItem);
                TileBase closetEmptyTile = _gridHelper.GetClosestEmptyWhiteTile(highlightTilePos, false);
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
        List<int> dropItemIdList = new List<int>();
        foreach (var itemId in items.Keys) {
            for (int i = 0; i < items[itemId]; i++) {
                dropItemIdList.Add(itemId);
            }
        }

        List<TileBase> availBases =  _gridHelper.GetEmptyPoses(dropFromPos, dropItemIdList.Count);

        for (int dropIndex = 0; dropIndex < dropItemIdList.Count; dropIndex++) {
            DropItem(dropItemIdList[dropIndex], dropFromPos, availBases[dropIndex].GetPos());
        }
    }

    private void DropItem(int itemId,Vector2Int fromPos, Vector2Int toPos) {
        ItemConfig itemConfig = ConfigSystem.GetItemConfig(itemId);
        ItemModel itemModel = new ItemModel(itemConfig, CommonTool.Vector2IntToArray(toPos));
        TileItem item = MountTileItem(itemModel);
        item.transform.position = _gridHelper.GetCellWorldPosition(fromPos);
        item.MoveAnimation(_gridHelper.GetCellWorldPosition(toPos));
        item.AppearAnimation();
    }
}
