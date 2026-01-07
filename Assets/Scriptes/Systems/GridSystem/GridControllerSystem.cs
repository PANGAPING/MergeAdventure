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
using System.Linq;

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

    public EventHandler  _onGroundItemChange;

    public EventHandler  _onUnlockGroup;

    private Dictionary<int, int> _groundWhiteItemNumMap = new Dictionary<int, int>();

    public List<ShelterTileBase> _shelterTiles = new List<ShelterTileBase>();

    public List<int> _shelterTileItemIds = new List<int>() {-1,-1,-1,-1,-1,-1 };

    //Group
    public Dictionary<int,List<TileItem>> _groupElfCloudMap= new Dictionary<int,List<TileItem>>();

    public Dictionary<int,bool> _groupUnlockedMap = new Dictionary<int,bool>();

    public Vector2Int _testStartPos = new Vector2Int(4,5);

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();

        //Test code of shelterTile
        if (MergeAdventureProgressController._instance.GetLevelId() > 2) {
            _shelterTileItemIds = new List<int>() { 2000105, -1, -1, -1, -1, -1 };
        }

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

        boardObj.transform.SetSiblingIndex(4);
        
        boardPrefab.name = "GameBoard";
        _gridHelper = new GridHelper(boardObj.GetComponent<GridLayoutGroup>());

    }


    protected override void Init()
    {
        base.Init();
        GamepadSystem._instance._onClickDown += OnTapDown;
        GamepadSystem._instance._onClickUp += OnTapUp;

        InitTileCursor();
        LoadMap();
        InitCloudGroup();
        RefreshMap();
        InitShelterTiles();
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

    private void InitShelterTiles() {
        _shelterTiles = WorldNode.Find("BottomUIPanel/Content/GeneratorShelter/Content").GetComponentsInChildren<ShelterTileBase>().ToList();

        for (int i = 0; i < _shelterTiles.Count; i++) {
            ItemConfig itemConfig = ConfigSystem.GetItemConfig(_shelterTileItemIds[i]);
            if (itemConfig != null) {
                TileItem generatorItem = MountTileItem(new ItemModel(itemConfig.ID, CommonTool.Vector2IntToArray(new Vector2Int(3,0))),false);
                _shelterTiles[i].OccupyItem(generatorItem);
                _shelterTiles[i]._onClick += () =>
                {
                    TapGenerator(generatorItem);
                };
            }
            _shelterTiles[i].Refresh();
        }

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
        MapSetting =MergeAdventureProgressController._instance.GetMapSetting();

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

            if (itemModel.GetItemConfig().Type != ItemType.MAPSETTING) { 
                MountTileItem(itemModel);
            }
        }
    }

    public void InitCloudGroup() { 
         _groupElfCloudMap = new Dictionary<int, List<TileItem>>();
        _groupUnlockedMap = new Dictionary<int,bool>();

        List<int> groups = new List<int>();
        Dictionary<Vector2Int, TileItem> cloudItems = itemMap[ItemType.ELFCLOUD];
        foreach (TileItem item in cloudItems.Values) {
            if (!groups.Contains(item.GetGroup())) {
                groups.Add(item.GetGroup()); 
            }
        }
        groups.Sort();

        foreach (int groupId in groups) {
            _groupUnlockedMap.Add(groupId, false);
            _groupElfCloudMap.Add(groupId, cloudItems.Values.ToList().FindAll(x => x.GetGroup() == groupId).ToList());
        }

    }

    public MapSetting GetCurMapSetting() {
        MapSetting mapSetting = new MapSetting(MergeAdventureProgressController._instance.GetLevelId());
        mapSetting.StartPos = MapSetting.StartPos;

        List<ItemModel> items = new List<ItemModel>();

        foreach (ItemType itemType in ItemTypes)
        {
            foreach (TileItem tileItem in itemMap[itemType].Values) {
                items.Add(tileItem.Model);
            }
        }

        mapSetting.Items = items.ToArray();
        return mapSetting;
    }
    private void RefreshMap() {
        _gridHelper.RefreshTilesState(_testStartPos);
        _gridHelper.RefreshTiles();
        UpdateWhiteGroundItemNumMap();
    }

    private void RefreshShelterTiles() {
    }



    private TileItem MountTileItem(ItemModel itemModel,bool inboard = true)
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

        if (inboard)
        {

            if (tilePoses.Length > 0)
            {
                _gridHelper.PutObjectOnTile(itemObject, tilePoses, 0);
            }
            else
            {
                _gridHelper.PutObjectOnTile(itemObject, tilePos, 0);
            }
            MountItemMap(item);
        }
        else { 
        
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

        if (!_gridHelper.IsWhiteTilePos(tileBase.GetPos())) {
            return;
        }

        TileItem tileItem = tileBase.GetLayerTopItem();
        if (tileItem == null) {
            return;
        }

        TapItem(tileItem);
        activeTileBase = tileBase;
    }

    private void TapItem(TileItem tileItem) {
        tileItem.GetTaped();
        ItemType itemType = tileItem.GetItemType();

        if (tileItem.IsActive())
        {
            DeActiveItem(tileItem); 
        }
        else {
            ActiveItem(tileItem);
        }

        if (itemType == ItemType.GENERATOR)
        {
            TapGenerator(tileItem);
        }
        else if (itemType == ItemType.CHEST)
        {
            TapChest(tileItem);
        }
        else if (itemType == ItemType.ASSET) {
            TapAsset(tileItem);
        }
    }

    private void ActiveItem(TileItem tileItem)
    {
        tileItem.Active();
        ItemType itemType = tileItem.GetItemType();
        if (itemType == ItemType.STACK)
        {
            GridUISystem._instance.OpenButtonTips(tileItem, TryOpenStack);
        }
        else if (itemType == ItemType.TREE)
        {
            GridUISystem._instance.OpenButtonTips(tileItem, TryCutTree);
        }
    }

    private void DeActiveItem(TileItem tileItem)
    {
        tileItem.DeActive();
        ItemType itemType = tileItem.GetItemType();
        if (itemType == ItemType.STACK)
        {
            GridUISystem._instance.CloseButtonTips(tileItem);
        }
        else if (itemType == ItemType.TREE)
        {
            GridUISystem._instance.CloseButtonTips(tileItem);
        }
    }

    private void TryOpenStack(TileItem tileItem) {
        DestroySpecialTileItems(new List<TileItem> { tileItem });
    }

    private void TryCutTree(TileItem tileItem) {
        Tree tree = (Tree)tileItem;

        Dictionary<int,int> items =  GetCutDropItemMap(tree);

        int energyCost = tree.GetCutCost();

        bool success = InventorySystem._instance.HaveAsset(ASSETTYPE.ENERGY, energyCost);
        if (!success)
        {
            GridUISystem._instance.ShowPopup(_gridHelper.GetCellWorldPosition(tileItem.GetPos()), "No Energy!", Color.white);
            DeActiveItem(tileItem);
            return;
        }

        InventorySystem._instance.RemoveAsset(ASSETTYPE.ENERGY, energyCost);

        Drop(items, tileItem.GetPos());
        bool die = tree.GetCut(1);
        if (die) {
            DestroySpecialTileItems(new List<TileItem> { tileItem });
        }
        else
        {
            DeActiveItem(tileItem);
        }
    }
    public void TryFeedWonderSketch(TileItem tileItem, WonderSketch wonderSketch) {
        UnMountItemMap(tileItem);
        tileItem.MoveAnimation(_gridHelper.GetCellWorldPosition(wonderSketch.GetPos()), AnimationEndActionType.DESTORY);
        wonderSketch.GetCompleted();
    }

    public void TryFeedElf(Elf elf) { 
        Dictionary<int,int> needItem = elf.GetDemandItems();

        List<TileItem> tileItems = _gridHelper.GetDemandItems(needItem, elf.GetPos());

        foreach (TileItem tileItem in tileItems) { 
            UnMountItemMap(tileItem);
            tileItem.MoveAnimation(_gridHelper.GetCellWorldPosition(tileItem.GetPos()), AnimationEndActionType.DESTORY);
        }

        List<TileItem> tileItemsToDestroy = new List<TileItem>();
        tileItemsToDestroy.Add(elf);
        DestroySpecialTileItems(tileItemsToDestroy);

        int group = elf.GetGroup();
        UnlockGroup(group);
    }

    public int GetCurrentTargetGroupId() {
        foreach (int group in _groupUnlockedMap.Keys) {
            if (!_groupUnlockedMap[group]) { 
                return group; 
            }
        }
        return -1;
    }

    public List<int> GetGeneratorItemIds() { 
        List<int> generatorIds = new List<int>();

        foreach (int itemId in _groundWhiteItemNumMap.Keys) { 
            ItemConfig itemConfig = ConfigSystem.GetItemConfig(itemId);
            if (itemConfig.Type == ItemType.GENERATOR) { 
                generatorIds.Add(itemId);
            }
        }
        return generatorIds;
    }

    public Vector2 GetGroupCenterPosition(int group) { 
        List<TileItem> elfClouds = _gridHelper.GetElfClouds(group);
        List<Vector2Int> vector2Ints = elfClouds.Select(x => x.GetPos()).ToList();
        Vector2 center = Vector2.zero;
        foreach (Vector2Int pos in vector2Ints) { 
            center += _gridHelper.GetCellWorldPosition(pos);
        }

        center = center / vector2Ints.Count;

        return center;
    }

    public MapSetting GetOriMapSetting() {
        return ConfigSystem.GetMapSetting(MergeAdventureProgressController._instance.GetLevelId());
    }

    public int GetGroupNeedKey(int group) {
        int keyNum = 1;

        MapSetting oriMapSetting = GetOriMapSetting();
        List<ItemModel> elfCloudItems = oriMapSetting.Items.ToList().FindAll(x => x.GetItemType() == ItemType.ELFCLOUD && x.IntData == group);

        ItemModel groupLocker = oriMapSetting.Items.ToList().Find(x => x.GetItemConfig().Name == "GroupLocker" && elfCloudItems.Exists(y => CommonTool.ArrayToVector2Int(y.TilePos) == CommonTool.ArrayToVector2Int(x.TilePos)));

        if (groupLocker != null) {
            keyNum = groupLocker.IntData;
        }

        return keyNum;
    }

    public bool IsGroupUnlockByKey(int group) {
        return !itemMap[ItemType.ELF].ToList().Exists(x => (x.Value.GetGroup() == group));
    }

    public void UnlockGroup(int group) {
        List<TileItem> elfClouds = _gridHelper.GetElfClouds(group);
        DestroySpecialTileItems(elfClouds);

        _groupUnlockedMap[group] = true;

        int keyCount = GetGroupNeedKey(group);
        InventorySystem._instance.RemoveAsset(ASSETTYPE.KEY, keyCount);

        UnlockGroupEvent();
        GroundItemChangeEvent();
    }

    public void UnlockGroupEvent() {
        if (_onUnlockGroup != null)
        {
            _onUnlockGroup.Invoke();
        }
    }

    private void DestroySpecialTileItems(List<TileItem> tileItems) {
        foreach (TileItem tileItem in tileItems) {
            DestroySpecialTileItem(tileItem);
        }
        RefreshMap();
    }

    private void DestroySpecialTileItem(TileItem tileItem) { 
        UnMountItemMap(tileItem);
        tileItem.Die();
        TileBase tileBase = _gridHelper.GetTileBase(tileItem.GetPos());
        tileBase.SetDirty();
    }

    private void TapGenerator(TileItem tileItem) {
        Generator generator = (Generator)tileItem;
        tileItem.GetTaped();

        GeneratorConfig generatorConfig = ConfigSystem.GetGeneratorConfig(tileItem.Model.GetItemConfig().ID);
        bool luck = false;

        Dictionary<int,int> dropMap = DropAlgorithmHelper.GetGeneratorDropResult(generator,out luck);

        bool success = InventorySystem._instance.HaveAsset(ASSETTYPE.ENERGY, 1);
        if (!success)
        {
            GridUISystem._instance.ShowPopup(_gridHelper.GetCellWorldPosition(tileItem.GetPos()), "No Energy!", Color.white);
            return;
        }

        success = Drop(dropMap, tileItem.GetPos(), luck);
        if (!success) {
            GridUISystem._instance.ShowPopup(_gridHelper.GetCellWorldPosition(tileItem.GetPos()), "Board Full!", Color.white);
            return;
        }

        InventorySystem._instance.RemoveAsset(ASSETTYPE.ENERGY, 1);


        bool die = generator.GetCut(1);
        if (die)
        {
            DestroySpecialTileItems(new List<TileItem> { tileItem });
        }
    }

    private void TapChest(TileItem tileItem) {
        Chest chest = (Chest)tileItem;
        chest.GetTaped();

        ChestConfig  chestConfig= ConfigSystem.GetChestConfig(tileItem.Model.GetItemConfig().ID);

        Dictionary<int,int> dropMap = DropAlgorithmHelper.GetDropResult(chestConfig.DropItemIds,chestConfig.DropItemRatios,1);

        bool success = Drop(dropMap, tileItem.GetPos());

        if (!success)
        {
            GridUISystem._instance.ShowPopup(_gridHelper.GetCellWorldPosition(tileItem.GetPos()), "Board Full!", Color.white);
        }

        int remainCount = chest.GetRemainCount();
        if (remainCount <= 0) { 
            UnMountItemMap(tileItem);
            tileItem.Die();
        }
    }

    private void TapAsset(TileItem tileItem)
    {
        UnMountItemMap(tileItem);
        tileItem.Die();
        
        int itemId = tileItem.GetItemId();
        AssetItemConfig assetItemConfig = ConfigSystem.GetAssetItemConfig(itemId);
        InventorySystem._instance.AddAsset(assetItemConfig.AssetId,assetItemConfig.Num);
        Sprite assetSprite = ResourceHelper.GetAssetSprite(ConfigSystem.GetAssetConfig(assetItemConfig.AssetId));

        RewardItemPanel rewardItemPanel = GridUISystem._instance.GetAssetBar(assetItemConfig.AssetId);

        GridUISystem._instance.FlyRewardIcon(assetSprite, (float)assetItemConfig.Num / 100,_gridHelper.GetCellWorldPosition(tileItem.GetPos()),rewardItemPanel.transform.position,rewardItemPanel.gameObject.GetComponent<RectTransform>());
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
            if (targetTileItem.GetItemType() == ItemType.WONDERSKETCH && CheckCanFeedWonderSketch(draggingItem, (WonderSketch)targetTileItem)) {
                TryFeedWonderSketch(draggingItem, (WonderSketch)targetTileItem); 
            }
            else if (targetTileItem.IsMovable())
            {
                draggingItem.SetPos(highlightTilePos);

                UnMountItemMap(targetTileItem);
                TileBase closetEmptyTile = _gridHelper.GetClosestEmptyWhiteTile(highlightTilePos, false);
                targetTileItem.SetPos(closetEmptyTile.GetPos());
                MountItemMap(targetTileItem);

                targetTileItem.MoveAnimation(_gridHelper.GetCellWorldPosition(closetEmptyTile.GetPos()),AnimationEndActionType.NONE,0.2f);
            }
            else
            {
                draggingItem.SetPos(draggingItem.GetPos());
            }
        }

        MountItemMap(draggingItem);
        draggingItem.MoveAnimation(_gridHelper.GetCellWorldPosition(draggingItem.GetPos()),AnimationEndActionType.NONE,0.1f);

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

        tileItem1.AnimationEndAction(AnimationEndActionType.DESTORY);
        tileItem2.AnimationEndAction(AnimationEndActionType.DESTORY);

        //tileItem1.MoveAnimation(_gridHelper.GetCellWorldPosition(mergePos), AnimationEndActionType.DESTORY,8000);
        //tileItem2.MoveAnimation(_gridHelper.GetCellWorldPosition(mergePos), AnimationEndActionType.DESTORY,8000);

        Drop(new Dictionary<int, int> { { mergeToId, 1 } }, mergePos);
    }

    public void DisappearTargetItem(int targetItemId,Vector3 animationTarget) { 

        _groundWhiteItemNumMap = new Dictionary<int, int>();

        foreach (TileBase tileBase in _gridHelper.GetTileBases()) {
            if (tileBase.GetState() == TileState.WHITE) {
                TileItem tileItem = tileBase.GetLayerTopItem();
                if (tileItem != null) {
                    int itemId = tileItem.Model.GetItemConfig().ID;
                    if (itemId == targetItemId) {
                        UnMountItemMap(tileItem);
                        tileItem.MoveAnimation(animationTarget,AnimationEndActionType.DESTORY);
                        break;
                    }
                }
            }
        }

        GroundItemChangeEvent();
    }

    private bool  Drop(Dictionary<int, int> items, Vector2Int dropFromPos , bool luck = false) {
        List<int> dropItemIdList = new List<int>();
        foreach (var itemId in items.Keys) {
            for (int i = 0; i < items[itemId]; i++) {
                dropItemIdList.Add(itemId);
            }
        }
        List<TileBase> availBases =  _gridHelper.GetEmptyPoses(dropFromPos, dropItemIdList.Count);

        for (int dropIndex = 0; dropIndex < availBases.Count; dropIndex++) {
            DropItem(dropItemIdList[dropIndex], dropFromPos, availBases[dropIndex].GetPos());
            //Temp
            if (luck)
            {
                GridUISystem._instance.ShowPopup( _gridHelper.GetCellWorldPosition(availBases[dropIndex].GetPos()), "Lucky!!", CommonTool.HexToColor("F5FF00"),0.3f);
            }
        }

        GroundItemChangeEvent();

        if (availBases.Count == dropItemIdList.Count)
        {
            return true;
        }
        else {
            return false;
        }
    }

    public int GetAvailableItemFromBoard(int itemId,bool includeCloud = false) {

        float lv1Count = 0;
        int chainId = ChainHelper.GetChainRoot(itemId);
        int mergeNeedRootCount = ChainHelper.ConvertItemIntoRootCount(itemId);

        Dictionary<int,int> treeNumMap = new Dictionary<int,int>();
        Dictionary<Vector2Int, TileItem> treeMap = itemMap[ItemType.TREE];
        foreach (Vector2Int pos in treeMap.Keys) {
            if (!includeCloud && _gridHelper.IsCloudTilePos(pos)) continue;

            int treeId = treeMap[pos].GetItemId();
            if (treeNumMap.ContainsKey(treeId))
                treeNumMap[treeId]++;
            else treeNumMap.Add(treeId, 1);
        }

        foreach (int treeId in treeNumMap.Keys)
        {
            TreeConfig treeConfig = ConfigSystem.GetTreeConfig(treeId);
            lv1Count += DropAlgorithmHelper.GetTreeDropOfItem(treeConfig, chainId) * treeNumMap[treeId];
        }



        Dictionary<int, int> normalNumMap = new Dictionary<int, int>();
        Dictionary<Vector2Int, TileItem> normalMap = itemMap[ItemType.NORMAL];
        foreach (Vector2Int pos in normalMap.Keys)
        {
            if (!includeCloud && _gridHelper.IsCloudTilePos(pos)) continue;

            int normalId = normalMap[pos].GetItemId();
            if (normalNumMap.ContainsKey(normalId))
                normalNumMap[normalId]++;
            else normalNumMap.Add(normalId, 1);
        }

        foreach (int normalId in normalNumMap.Keys) {
            int normalChainId = ChainHelper.GetChainRoot(normalId);
            if (normalChainId == chainId)
            {
                lv1Count += ChainHelper.ConvertItemIntoRootCount(normalId) * normalNumMap[normalId];
            }
        }

        return (int)(lv1Count / mergeNeedRootCount);
    }

    private void GroundItemChangeEvent() {
        UpdateWhiteGroundItemNumMap();
        if (_onGroundItemChange != null) {
            _onGroundItemChange.Invoke();
        }
    }


    public Dictionary<int, int> GetWhiteGroundItemNumMap() {
        return _groundWhiteItemNumMap;
    }

    public bool CheckCanFeedElf(Elf elf) {
        Dictionary<int, int> needItemMap = elf.GetDemandItems();

        bool satisfy = true;
        foreach (int itemId in needItemMap.Keys) {
            if (!_groundWhiteItemNumMap.ContainsKey(itemId) || _groundWhiteItemNumMap[itemId] < needItemMap[itemId]) {
                satisfy = false;
                break;
            } 
        }

        return satisfy;
    }

    public bool CheckCanFeedWonderSketch(TileItem tileItem, WonderSketch wonderSketch) {
        Dictionary<int, int> needItemMap = wonderSketch.GetDemandItems();
        bool satisfy = true;
        if (wonderSketch.IsCompleted()) {
            satisfy = false;
        }
        if (!needItemMap.ContainsKey(tileItem.GetItemId())) {
            satisfy = false; 
        }
        return satisfy;
    }

    private void UpdateWhiteGroundItemNumMap() { 
        _groundWhiteItemNumMap = new Dictionary<int, int>();

        foreach (TileBase tileBase in _gridHelper.GetTileBases()) {
            if (tileBase.GetState() == TileState.WHITE) {
                TileItem tileItem = tileBase.GetLayerTopItem();
                if (tileItem != null) {
                    int itemId = tileItem.Model.GetItemConfig().ID;
                    if (_groundWhiteItemNumMap.ContainsKey(itemId))
                    {
                        _groundWhiteItemNumMap[itemId] += 1;
                    }
                    else { 
                        _groundWhiteItemNumMap[itemId] = 1;
                    }
                }
            }
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

    private Dictionary<int,int> GetCutDropItemMap(Tree tree) {
        TreeConfig treeConfig = ConfigSystem.GetTreeConfig(tree.Model.GetItemConfig().ID);
        int cutCount = tree.GetMaxBloodCount() - tree.GetNowBloodCount() + 1;

        int[] energyCost= treeConfig.EnergyCost;
        int energyCostAll = DropAlgorithmHelper.Sum(energyCost);
        float[] ratioFromCutIndex = new float[energyCost.Length];
        for (int i = 0; i < energyCost.Length; i++) {
            ratioFromCutIndex[i] = (float)energyCost[i] / (float)energyCostAll;
        }

        return DropAlgorithmHelper.GetDropResultOnce(treeConfig.DropItemCount, treeConfig.DropItemIds, treeConfig.DropItemRatios, ratioFromCutIndex, cutCount - 1);

    }
}
