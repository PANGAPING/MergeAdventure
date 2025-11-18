using FlyEggFrameWork.GameGlobalConfig;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[System.Serializable]
public class ItemEditorCell
{
    public ItemConfig _itemConfig;


    public ItemEditorCell(ItemConfig itemConfig) { 
        _itemConfig = itemConfig;
    }


    public bool DrawCell(bool select = false) { 
        ItemConfig itemConfig = _itemConfig;
        GUILayout.BeginVertical();
        Texture2D itemIcon = ResourceHelper.GetItemTexture(itemConfig);
        if (select)
        {
            GUI.backgroundColor = Color.green;
        }

        bool clicked = GUILayout.Button(itemIcon, GUILayout.Width(60), GUILayout.Height(60));

        EditorGUILayout.LabelField(itemConfig.Name, GUILayout.Width(60));
        GUILayout.EndVertical();

        return clicked;
    }
    
}
