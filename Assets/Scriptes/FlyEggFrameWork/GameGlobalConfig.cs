using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FlyEggFrameWork.GameGlobalConfig
{


    public static class FoldPath
    {
        public static string PrefabFolderPath = Path.Combine("Prefab");

        public static string SpriteFolderPath = Path.Combine("Sprite");

        public static string UIPrefabFolderPath = Path.Combine(PrefabFolderPath, "UI");

        public static string GameSystemUIFolderPath = Path.Combine(PrefabFolderPath, "UI/GameSystemUI");

        public static string ClientPrfabFolderPath = Path.Combine(PrefabFolderPath, "Client");

        public static string EffectPrefabFolderPath = Path.Combine(PrefabFolderPath, "Effect");

        public static string SettingAssetFolderPath = Path.Combine("Setting");

        public static string WeatherSettingAssetFolderPath = Path.Combine(SettingAssetFolderPath, "Weather");

        public static string UIBubbleFolderPath = Path.Combine(UIPrefabFolderPath, "Bubble");

        public static string InventoryUIFolderPath = Path.Combine(UIPrefabFolderPath, "Inventory");

        public static string FishPrefabFolderPath = Path.Combine(PrefabFolderPath, "Fish");

        public static string DefaultExcelConfigFoloder = Path.Combine("Config", "Excel");

        public static string DefaultJsonConfigFolder = Path.Combine("Json");

        public static string ResourcesFolderPath = Path.Combine(Application.dataPath, "Resources");

        public static string LevelTileMapTileFolderPath = Path.Combine("Tiles/Level");

        public static string StorageFolderPath = Path.Combine(Application.persistentDataPath, "Storage");

        public static string LocalizationFolderPath = Path.Combine("Localization");

        public static string LanguageFilesFolderPath = Path.Combine(LocalizationFolderPath, "LanguageFiles");

        public static string MapGridFolderPath = Path.Combine("MapGrid");

        public static string MapSettingFolderPath = Path.Combine(Application.dataPath, "MapSetting");

        public static string MaterialFolderPath = Path.Combine("Material");
    }

    public static class ConfigPath
    {
        public static string TreeConfig = Path.Combine(FoldPath.DefaultJsonConfigFolder, "TreeConfig");

        public static string DropItemConfig = Path.Combine(FoldPath.DefaultJsonConfigFolder, "DropItemConfig");

        public static string MapSettingItemConfig = Path.Combine(FoldPath.DefaultJsonConfigFolder, "MapSettingItemConfig");

        public static string LocalizationExcelFile = Path.Combine(FoldPath.LocalizationFolderPath, "Localization.xlsx");

    }



}

