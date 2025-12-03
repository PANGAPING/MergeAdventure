using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class BloodSliderPanel : TileItemPanel
{
    GameObject _cell;

    protected Color _fillColor = new Color(53f / 255, 201f / 255, 255f / 255);

    protected Color _emptyColor = new Color(132f / 255, 132f / 255, 132f / 255);


    public override void MountTileItem(TileItem tileItem, Action callback = null)
    {
        base.MountTileItem(tileItem, callback);
        Tree tree = (Tree)_tileItem;
        int nowBloodCount = tree.GetNowBloodCount();
        int maxBloodCount = tree.GetMaxBloodCount();

        _cell = transform.Find("cell").gameObject;
        for (int i = 0; i < maxBloodCount; i++) {
            Debug.Log(i);
            GameObject cell = GameObject.Instantiate(_cell, transform);
            cell.SetActive(true);
            if (i < nowBloodCount)
            {
                cell.GetComponent<Image>().color = _fillColor;
            }
            else { 
                cell.GetComponent<Image>().color = _emptyColor;
            }
        }
        _cell.SetActive(false);

        tileItem._onDie += () => { GameObject.Destroy(gameObject); };
        UpdateView();
    }

    public override void UpdateView()
    {
        base.UpdateView();
        Tree tree = (Tree)_tileItem;
        int nowBloodCount = tree.GetNowBloodCount();
        int maxBloodCount = tree.GetMaxBloodCount();

        for (int i = 0; i < maxBloodCount; i++) {
            if (i < nowBloodCount)
            {
                transform.GetChild(i+1).GetComponent<Image>().color = _fillColor;
            }
            else { 
                transform.GetChild(i+1).GetComponent<Image>().color = _emptyColor;
            }
        }
    }


}
