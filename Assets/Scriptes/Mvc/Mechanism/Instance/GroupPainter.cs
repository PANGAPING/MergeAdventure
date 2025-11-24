using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GroupPainter : TileItem
{
    protected int _group;
    public int GetGroup() {
        return _group;
    }

    public void SetGroup(int group) { 
        _group = group;
        transform.Find("GroupPanel").GetComponent<TextMeshProUGUI>().text = _group.ToString();
    }
}
