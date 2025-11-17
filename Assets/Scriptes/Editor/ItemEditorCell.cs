using FlyEggFrameWork.GameGlobalConfig;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[System.Serializable]
public class ItemEditorCell : MonoBehaviour
{
    public ItemConfig _itemConfig;

    public bool select;

    public ItemEditorCell(ItemConfig itemConfig) { 
        _itemConfig = itemConfig;
    }

    public void Select() {
        select = true; 
    }

    public void DeSelect() {
        select = false;
    }

    public bool DrawCell() { 
        ItemConfig itemConfig = _itemConfig;
        GUILayout.BeginVertical();
        Texture itemIcon = Resources.Load<Texture>(Path.Combine(FoldPath.SpriteFolderPath,itemConfig.SpritePath));

        bool clicked = GUILayout.Button(itemIcon, GUILayout.Width(60), GUILayout.Height(60));
        if (select)
        {
            GUI.backgroundColor = Color.green;
        }

        GUI.backgroundColor = GUI.backgroundColor;

        EditorGUILayout.LabelField(itemConfig.Name, GUILayout.Width(60));
        GUILayout.EndVertical();

        return clicked;
    }
    
}
