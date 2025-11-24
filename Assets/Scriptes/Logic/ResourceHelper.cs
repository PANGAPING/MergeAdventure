using FlyEggFrameWork.GameGlobalConfig;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public static class ResourceHelper
{
    public static GameObject GetItemPrefab(ItemConfig itemConfig) {

        GameObject itemPrefab = Resources.Load<GameObject>(Path.Combine(FoldPath.PrefabFolderPath, "TileItem/DefaultTileItem"));
        if (itemConfig.PrefabPath != null && itemConfig.PrefabPath.Length > 0) {
            itemPrefab = Resources.Load<GameObject>(Path.Combine(FoldPath.PrefabFolderPath, itemConfig.PrefabPath));
        }
        return itemPrefab;
    }

    public static GameObject GetUIPrefab(string PanelName) { 
        GameObject panelPrefab = Resources.Load<GameObject>(Path.Combine(FoldPath.PrefabFolderPath, "UI/"+PanelName));
        return panelPrefab;
    }

    public static Texture2D GetItemTexture(ItemConfig itemConfig) { 
        return Resources.Load<Texture2D>(Path.Combine(FoldPath.SpriteFolderPath,itemConfig.SpritePath));
    }

    public static Sprite GetItemSprite(ItemConfig itemConfig) {
        return Resources.Load<Sprite>(Path.Combine(FoldPath.SpriteFolderPath,itemConfig.SpritePath));

    }

    public static Vector2 ConvertSpritePivotToRectTransform(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.LogError("Sprite不能为空");
            return new Vector2(0.5f, 0.5f); // 默认中心点
        }

        // 获取精灵纹理尺寸
        int textureWidth = sprite.texture.width;
        int textureHeight = sprite.texture.height;

        // 获取精灵pivot像素坐标（左下角为原点）
        Vector2 pixelPivot = sprite.pivot;

        // 转换为归一化坐标（RectTransform的坐标系左下为(0,0)，右上为(1,1)）
        Vector2 normalizedPivot = new Vector2(
            x: pixelPivot.x / textureWidth,
            y: 1.0f - (pixelPivot.y / textureHeight) // Y轴方向需要翻转
        );

        return normalizedPivot;
    }
}
