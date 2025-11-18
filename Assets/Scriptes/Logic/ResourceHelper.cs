using FlyEggFrameWork.GameGlobalConfig;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ResourceHelper
{
    public static GameObject GetItemPrefab(ItemConfig itemConfig) {

        GameObject itemPrefab = Resources.Load<GameObject>(Path.Combine(FoldPath.PrefabFolderPath, "TileItem/DefaultTileItem"));
        if (itemConfig.PrefabPath!=null && itemConfig.PrefabPath.Length > 0) {
            itemPrefab = Resources.Load<GameObject>(Path.Combine(FoldPath.PrefabFolderPath, itemConfig.PrefabPath));
        }
        return itemPrefab;
    }

    public static Texture2D GetItemTexture(ItemConfig itemConfig) { 
        return Resources.Load<Texture2D>(Path.Combine(FoldPath.SpriteFolderPath,itemConfig.SpritePath));
    }

    public static Sprite GetItemSprite(ItemConfig itemConfig) {
        return ConvertToSprite(GetItemTexture(itemConfig));
    }

    public static Sprite ConvertToSprite(Texture2D texture,
    SpriteMeshType meshType = SpriteMeshType.Tight)
    {
        if (texture == null) return null;

        // 使用可控制的参数创建
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), // 枢轴点
            100,                    // Pixels Per Unit
            0,                      // 额外细节级别
            meshType,               // Tight 或 FullRect 网格
            Vector4.zero            // 边框(九宫格用)
        );
    }
}
