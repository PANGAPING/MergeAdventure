using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chain : Mechanism
{
    [SerializeField]
    protected Image _chainPanel;
    [SerializeField]
    protected Image _chainBottomPanel;
    [SerializeField]
    protected Image _itemPanel;


    public override void SetWhiteColor()
    {
        base.SetWhiteColor();

        _itemPanel.color = _whiteColor;
        _chainPanel.color = _whiteColor;
    }

    public override void SetGrayColor()
    {
        base.SetGrayColor();

        _itemPanel.color = _grayColor;
        _chainPanel.color = _grayColor;
    }
}
