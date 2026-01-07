using Codice.Client.BaseCommands.Merge.Xml;
using Codice.Client.Common.ProcessTree;
using FlyEggFrameWork.GameGlobalConfig;
using FlyEggFrameWork.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditor : EditorWindow
{
    private static LevelEditor Instance;

    private static SceneView SceneView;

    private bool editing = false;

    private int editingLevel = 1;

    private GameObject TileCursor;

    private Vector2Int activeTilePos;

    private TileBase activeTileBase;

    private static Color DefaultGUIBackgroundColor;

    [Header("LevelData")]
    private MapSetting MapSetting;

    //item instances
    private Dictionary<ItemType, Dictionary<Vector2Int, TileItem>> itemMap;

    //item models
    private Dictionary<ItemType, List<ItemModel>> itemModelListMap;


    [Header("Options")]
    private static string[] ItemTypesString = new string[] { };

    private static ItemType[] ItemTypes = new ItemType[] { };

    private static int itemTypeIndex = 0;

    private static ItemType NowItemType;

    private static string filterString = "";

    [Header("Items")]
    [Space]
    private ItemConfig usingItem;

    private TreeConfig usingTree;

    private MechanismConfig usingMapSettingItem;

    private GameObject usingItemObj;

    private int itemIntConfigValue = 0;

    [Header("Scroll Para")]
    [Space]
    protected Vector2 itemScrollVec = Vector2.zero;

    [Header("Node")]
    [Space]
    protected Transform WorldNode;

    protected Transform ItemsNode;

    protected Transform BoardNode;

    protected GridLayoutGroup BoardLayoutGroup;

    public Dictionary<ItemType, Transform> NodeMap;

    public Dictionary<int, Transform> LayerNodeMap;

    [Header("Helper")]
    [Space]
    protected GridHelper _gridHelper;

    [Header("PainterValue")]
    [Space]
    protected int groupValue;



    [MenuItem("Level/LevelEditor")]
    private static void ShowLevelEditorEnter()
    {
        Instance = EditorWindow.GetWindow<LevelEditor>();

        DefaultGUIBackgroundColor = GUI.backgroundColor;

        ItemTypesString = new string[Enum.GetNames(typeof(ItemType)).Length];
        ItemTypes = (ItemType[])Enum.GetValues(typeof(ItemType));
        for (int i = 0; i < ItemTypes.Length; i++)
        {
            ItemTypesString[i] = EnumUtils.GetStringValue(ItemTypes[i]);
        }

        ConfigSystem.LoadConfigs();
        Instance.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        SceneVisibilityManager.instance.DisableAllPicking();
    }


    private void OnDestroy()
    {
        EndEditLevel();
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneVisibilityManager.instance.EnableAllPicking();
    }


    void OnGUI()
    {
        DrawOptions();
    }


    private void StartEditLevel()
    {
        EditorSceneManager.OpenScene(Path.Combine(Application.dataPath, "Scenes", "LevelEditorScene.unity"));
        editing = true;

        GameObject worldNodeObj = GameObject.Find("Canvas");
        WorldNode = worldNodeObj.transform;


        if (worldNodeObj != null)
        {
            CommonTool.DeleteAllChildrenImmediate(WorldNode);
        }
        else {
            Debug.LogError("No Canvas in Level Editor Scene.");
            return;
        }


        GameObject boardNodeObj = GameObject.Find("Canvas/GameBoard");
        if (boardNodeObj != null) {
            DestroyImmediate(boardNodeObj);
        }



        GameObject boardPrefab = Resources.Load<GameObject>(Path.Combine(FoldPath.PrefabFolderPath, "GameBoard"));
        GameObject boardObj = GameObject.Instantiate(boardPrefab, WorldNode);
        boardPrefab.name = "GameBoard";
        BoardNode = boardObj.transform;
        BoardLayoutGroup = BoardNode.GetComponent<GridLayoutGroup>();

        _gridHelper = new GridHelper(BoardLayoutGroup);

        GameObject itemNodeObj = new GameObject("Items");
        itemNodeObj.transform.SetParent(WorldNode, false);
        ItemsNode = itemNodeObj.transform;


        InitTileCursor();

        SceneVisibilityManager.instance.DisableAllPicking();
        LoadMap();
        SortLayer();
    }

    private void InitTileCursor() {
        //Init tile Cursor.
        string tileCursorPath = Path.Combine(FoldPath.PrefabFolderPath, "Tiles", "TileCursor", "TileCursor");
        GameObject tileCursorPrefab = Resources.Load<GameObject>(tileCursorPath);
        TileCursor = GameObject.Instantiate(tileCursorPrefab);
        TileCursor.transform.SetParent(WorldNode);
        TileCursor.transform.localPosition = Vector3.zero;
        TileCursor.transform.localRotation = Quaternion.identity;
        SceneVisibilityManager.instance.DisablePicking(TileCursor, true);
    }

    private void LoadMap() {
        MapSetting = ConfigSystem.GetMapSetting(editingLevel);

        NodeMap = new Dictionary<ItemType, Transform>();
        LayerNodeMap = new Dictionary<int, Transform>();

        itemMap = new Dictionary<ItemType, Dictionary<Vector2Int, TileItem>>();
        itemModelListMap = new Dictionary<ItemType, List<ItemModel>>();

        foreach (ItemType itemType in ItemTypes)
        {
            string itemTypeString = EnumUtils.GetStringValue(itemType);
            //Transform node = new GameObject(itemTypeString).transform;
            //node.SetParent(WorldNode);
            //NodeMap.Add(itemType, node);
            itemModelListMap.Add(itemType, new List<ItemModel>());
        }

        foreach (ItemType itemType in ItemTypes) {
            itemMap.Add(itemType, new Dictionary<Vector2Int, TileItem>());
        }

        for (int i = 0; i < MapSetting.Items.Length; i++)
        {
            ItemModel itemModel = MapSetting.Items[i];
            MountTileItem(itemModel);
        }
    }

    private void SortLayer() {

        LayerNodeMap = CommonTool.RebuildSortedByKey(LayerNodeMap);

        foreach (int layer in LayerNodeMap.Keys) {
            LayerNodeMap[layer].SetSiblingIndex(LayerNodeMap.Keys.ToList().IndexOf(layer)); 
        }

    }


    private TileItem MountTileItem(ItemModel itemModel)
    {

        ItemConfig itemConfig = ConfigSystem.GetItemConfig(itemModel.ItemConfigID);

        GameObject itemPrefab = ResourceHelper.GetItemPrefab(itemConfig);

        int layer = itemConfig.Layer;
        ItemType itemType = itemConfig.Type;
        GameObject itemObject;

        if (!LayerNodeMap.ContainsKey(layer))
        {
            LayerNodeMap.Add(layer, new GameObject(layer.ToString()).transform);
            LayerNodeMap[layer].SetParent(ItemsNode);
        }

        if (!NodeMap.ContainsKey(itemType))
        {
            NodeMap.Add(itemType, new GameObject(EnumUtils.GetStringValue(itemType)).transform);
            NodeMap[itemType].SetParent(LayerNodeMap[layer]);
        }

        itemObject = GameObject.Instantiate(itemPrefab,LayerNodeMap[layer]);

        // itemObject = GameObject.Instantiate(itemPrefab, NodeMap[itemConfig.Type]);

        TileItem item = itemObject.GetComponent<TileItem>();

        item.MountModel(itemModel);

        Vector2Int tilePos = CommonTool.ArrayToVector2Int(itemModel.TilePos);
        Vector2Int[] tilePoses = new Vector2Int[0];

        if (itemModel.TilePoses != null)
        {
            tilePoses = CommonTool.Array2ToVector2IntArray(itemModel.TilePoses);
        }


        itemMap[itemConfig.Type][tilePos] = item;
        itemModelListMap[itemConfig.Type].Add(itemModel);


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

        return item;
    }



    private void TestLevel()
    {
        EditorSceneManager.OpenScene(Path.Combine(Application.dataPath, "Scenes", "GameScene.unity"));
        Close();
    }
    private void EndEditLevel()
    {
        editing = false;
    }

    private void DrawOptions()
    {

        EditorGUILayout.BeginVertical();

        DrawLevelChoose();

        DrawItemFilter();

        DrawItems();

        DrawItemExtraConfigPanel();
        DrawViewHelperPanel();
        DrawSaveButton();
        DrawTestButton();
        EditorGUILayout.EndVertical();
    }



    private void OnSceneGUI(SceneView sceneView)
    {
        string openingSceneName = EditorSceneManager.GetActiveScene().name;

        if (openingSceneName != "LevelEditorScene" || !editing)
        {
            return;
        }

        // Get the current event
        Event currentEvent = Event.current;

        // Check if the event type is a mouse move event
        if (currentEvent.type == EventType.MouseMove)
        {
            // Get the mouse position in the Scene view
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            UpdateTileCursor(ray.origin);
            if (usingItemObj != null)
            {
                UpdateUsingItem(ray.origin);
            }
        }
        else if (currentEvent.type == EventType.MouseDown)
        {
            if (currentEvent.button == 0)
            {
                if (usingItem == null) {
                    return;
                }

                if (usingItem.Type == ItemType.MAPPAINTER)
                {
                    if (usingItem.Name == "GroupPainter")
                    {
                        if (activeTileBase == null)
                        {
                            return;
                        }
                        if (activeTileBase.ExistItemOfType(ItemType.ELFCLOUD) && NodeMap[ItemType.ELFCLOUD].gameObject.activeSelf)
                        {
                            activeTileBase.GetItemOfType(ItemType.ELFCLOUD).SetGroup(groupValue);
                        }
                        else if (activeTileBase.ExistItemOfType(ItemType.ELF) && NodeMap[ItemType.ELF].gameObject.activeSelf)
                        {
                            activeTileBase.GetItemOfType(ItemType.ELF).SetGroup(groupValue);
                        }
                        else if (activeTileBase.ExistItemOfType(ItemType.STACK) && NodeMap[ItemType.STACK].gameObject.activeSelf)
                        {
                            activeTileBase.GetItemOfType(ItemType.STACK).SetGroup(groupValue);
                        }
                        else if (activeTileBase.ExistItemOfType(ItemType.CHAIN) && NodeMap[ItemType.CHAIN].gameObject.activeSelf)
                        {
                            activeTileBase.GetItemOfType(ItemType.CHAIN).SetGroup(groupValue);
                        }

                    }
                }
                else if (activeTileBase != null && usingItem != null && !activeTileBase.ExistItemOfType(usingItem.Type) && !activeTileBase.ExistItemOfLayer(usingItem.Layer))
                {
                    NewItem(usingItem, activeTilePos);
                    SceneVisibilityManager.instance.DisableAllPicking();
                }
                else if (activeTileBase.ExistItemOfType(ItemType.ELF))
                {
                    AddDemandToElf(usingItem, activeTileBase.GetItemOfType(ItemType.ELF) as Elf);
                }
                else if (activeTileBase.ExistItemOfType(ItemType.WONDERSKETCH)) { 
                    AddDemandToWonderSketch(usingItem, activeTileBase.GetItemOfType(ItemType.WONDERSKETCH) as WonderSketch);
                }
            }
            else if (currentEvent.button == 1)
            {
                if (activeTileBase != null && activeTileBase.ExistItemOfType(NowItemType)) {

                    DeleteItem(activeTilePos, NowItemType);
                }
                else if (activeTileBase.ExistItemOfType(ItemType.ELF))
                {
                    DeleteDemandToElf(usingItem,activeTileBase.GetItemOfType(ItemType.ELF) as Elf);
                }

            }
        }
    }

    private void UpdateUsingItem(Vector3 mousePosition)
    {
        mousePosition.z = 10;
        usingItemObj.transform.position = mousePosition;
        usingItemObj.transform.rotation = Quaternion.identity;
    }
    private void UpdateTileCursor(Vector3 mousePosition)
    {
        var mousePos = mousePosition;

        activeTilePos = _gridHelper.GetGridPosition(mousePos);
        GameObject activeTileObj = _gridHelper.GetCellAtPosition(activeTilePos);

        activeTileBase = activeTileObj != null ? activeTileObj.GetComponent<TileBase>() : null;

        if (activeTileBase == null) {
            TileCursor.SetActive(false);
            return;
        }

        TileCursor.SetActive(true);
        Vector2 tileCenterPosition = _gridHelper.GetCellWorldPosition(activeTilePos);
        TileCursor.transform.position = tileCenterPosition;
        TileCursor.transform.rotation = Quaternion.identity;
    }


    private void DrawLevelChoose()
    {
        GUILayout.Space(10);

        GUILayout.BeginHorizontal(GUILayout.Height(40));


        GUILayout.Space(10);

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        GUIStyle labStyle = new GUIStyle(UnityEditor.EditorStyles.label);
        labStyle.fontSize = 18;
        labStyle.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Level:", labStyle, GUILayout.Width(60));

        GUILayout.FlexibleSpace();

        GUIStyle inputAreaStyle = new GUIStyle(UnityEditor.EditorStyles.textField);
        inputAreaStyle.fontSize = 18;
        inputAreaStyle.fixedHeight = 24;

        editingLevel = EditorGUILayout.IntField(editingLevel, inputAreaStyle);

        GUILayout.EndVertical();


        GUILayout.BeginVertical();
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 24;
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.margin = new RectOffset(0, 0, 0, 0);

        bigButtonStyle.stretchWidth = true;
        bigButtonStyle.stretchHeight = true;

        if (GUILayout.Button("Edit Level.", bigButtonStyle, GUILayout.MaxHeight(60), GUILayout.MaxWidth(160)))
        {
            StartEditLevel();
        }
        GUILayout.EndVertical();


        GUILayout.Space(10);

        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void DrawItemFilter()
    {

        GUILayout.Space(10);
        GUILayout.BeginHorizontal(GUILayout.Height(40));
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Item Type:", GUILayout.Width(70));
        itemTypeIndex = EditorGUILayout.Popup(itemTypeIndex, ItemTypesString, GUILayout.Width(125));
        if (ItemTypes[itemTypeIndex] != NowItemType)
        {
            ClearUsingItem();
        }

        NowItemType = ItemTypes[itemTypeIndex];
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
        filterString = EditorGUILayout.TextField(filterString,GUILayout.Width(120));

        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    private void ClearUsingItem() {
        usingItem = null;
        if (usingItemObj != null) {
            DestroyImmediate(usingItemObj);
        }
    }

    private void UsingItem(ItemConfig itemConfig) {
        ClearUsingItem();
        usingItem = itemConfig;

        string tileCursorPath = Path.Combine(FoldPath.PrefabFolderPath, "Tiles", "TileCursor", "TileCursor");
        GameObject tileCursorPrefab = Resources.Load<GameObject>(tileCursorPath);
        usingItemObj = GameObject.Instantiate(tileCursorPrefab);
        usingItemObj.transform.SetParent(WorldNode);
        usingItemObj.transform.localPosition = Vector3.zero;
        usingItemObj.transform.localRotation = Quaternion.identity;

        Image itemImg = usingItemObj.GetComponent<Image>();
        itemImg.sprite = ResourceHelper.GetItemSprite(itemConfig);
        itemImg.SetNativeSize();
        SceneVisibilityManager.instance.DisablePicking(usingItemObj, true);
    }

    private void DrawItems()
    {
        GUILayout.BeginVertical();
        itemScrollVec = GUILayout.BeginScrollView(itemScrollVec, GUILayout.Height(350), GUILayout.Width(500));

        int rowItemCount = 6;

        int itemIndex = 0;
        int rowIndex = 0;


        List<ItemConfig> itemConfigs = ConfigSystem.GetItemConfigs(NowItemType);

        foreach (ItemConfig itemConfig in itemConfigs)
        {
            if (itemIndex % rowItemCount == 0)
            {
                if (rowIndex == Mathf.FloorToInt((float)itemIndex / rowItemCount))
                {
                    if (rowIndex != 0)
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    rowIndex++;
                }
            }
            GUI.backgroundColor = DefaultGUIBackgroundColor;
            ItemEditorCell itemEditorCell = new ItemEditorCell(itemConfig);

            bool selected = itemEditorCell.DrawCell(usingItem == itemConfig);
            if (selected && usingItem != itemConfig)
            {
                UsingItem(itemConfig);
            }
            else if (selected && usingItem == itemConfig) {
                ClearUsingItem();
            }
            itemIndex++;
        }

        GUI.backgroundColor = DefaultGUIBackgroundColor; GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawItemExtraConfigPanel()
    {
        if (usingItem == null ) {
            return;
        }

        string usingItemName = usingItem.Name;

        if (usingItemName == "GroupPainter" || usingItemName =="GroupLocker" || usingItemName.StartsWith("ElfCloud") || usingItem.Type == ItemType.STACK || usingItem.Type == ItemType.CHAIN)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("GroupConfig:", GUILayout.Width(90));
            groupValue = EditorGUILayout.IntField(groupValue,GUILayout.Width(60));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }
    }


    private void DrawViewHelperPanel()
    {
        if (!editing) {
            return;
        }
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("œ‘ æ”Î“˛≤ÿ:", GUILayout.Width(60));

        GUILayout.BeginVertical();

        //  for (int i = 0; i < ItemTypes.Length; i++)
        //  {
        //      if (i % 5 == 0) {
        //          GUILayout.BeginHorizontal();
        //      }
        //      ItemType itemType = ItemTypes[i];
        //      string itemTypeString = EnumUtils.GetStringValue(itemType);
        //      bool active = GUILayout.Toggle(NodeMap[itemType].gameObject.activeSelf, itemTypeString);
        //      if (active)
        //      {
        //          NodeMap[itemType].gameObject.SetActive(true);
        //      }
        //      else { 
        //          NodeMap[itemType].gameObject.SetActive(false);
        //      }
        //      if (i % 5 == 4 || i == ItemTypes.Length -1) {
        //          GUILayout.FlexibleSpace();
        //          GUILayout.EndHorizontal();
        //      }
        //  }

        GUILayout.BeginHorizontal();
        foreach (int layer in LayerNodeMap.Keys)
        {
            bool active = GUILayout.Toggle(LayerNodeMap[layer].gameObject.activeSelf, layer.ToString());
            if (active)
            {
                LayerNodeMap[layer].gameObject.SetActive(true);
            }
            else
            {
                LayerNodeMap[layer].gameObject.SetActive(false);
            }

        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.EndVertical();


        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void DrawSaveButton()
    {
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 24;
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.margin = new RectOffset(0, 0, 0, 0);

        bigButtonStyle.stretchWidth = true;
        bigButtonStyle.stretchHeight = true;

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.FlexibleSpace();


        if (GUILayout.Button("Save Level.", bigButtonStyle, GUILayout.MaxHeight(60), GUILayout.MaxWidth(160)))
        {
            SaveMap();
        }

        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void DrawTestButton()
    {
        GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
        bigButtonStyle.fontSize = 24;
        bigButtonStyle.fontStyle = FontStyle.Bold;
        bigButtonStyle.margin = new RectOffset(0, 0, 0, 0);

        bigButtonStyle.stretchWidth = true;
        bigButtonStyle.stretchHeight = true;

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.FlexibleSpace();


        if (GUILayout.Button("Test Level", bigButtonStyle, GUILayout.MaxHeight(60), GUILayout.MaxWidth(160)))
        {
            TestLevel();
        }

        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void NewItem(ItemConfig itemConfig, Vector2Int tilePos)
    {
        ItemModel itemModel = new ItemModel(itemConfig, CommonTool.Vector2IntToArray(tilePos));
        TileItem item = MountTileItem(itemModel);
        item.SetGroup(groupValue);
    }
    private void AddDemandToWonderSketch(ItemConfig itemConfig, WonderSketch wonderSketch) { 
        wonderSketch.SetDemand(itemConfig.ID);
    }

    private void AddDemandToElf(ItemConfig itemConfig, Elf elf) {
        elf.AddDemand(itemConfig.ID, 1); 
    }


    private void DeleteDemandToElf(ItemConfig itemConfig, Elf elf) {
        elf.DeleteDemand(itemConfig.ID, 1); 
    }

    private void DeleteItem(Vector2Int tilePos, ItemType itemType) {
        TileItem tileItem = itemMap[itemType][tilePos];
        itemMap[itemType].Remove(tilePos);
        itemModelListMap[itemType].Remove(tileItem.Model);
        GameObject.DestroyImmediate(tileItem.gameObject);
        _gridHelper.GetTileBase(tilePos).RemoveOccupyItem(tileItem);
    }

    private void SaveMap() {
        MapSetting.Level = editingLevel;
        MapSetting.StartPos = CommonTool.Vector2IntToArray(Vector2Int.zero);

        List<ItemModel> itemModels = new List<ItemModel>();
        foreach (ItemType itemType in ItemTypes)
        {
            itemModels.AddRange(itemModelListMap[itemType]);
        }

        MapSetting.Items = itemModels.ToArray();
        ConfigSystem.SaveMapSetting(editingLevel,MapSetting);
    }
}
