using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardItemPanel : GameUIPanel
{
    [SerializeField]
    protected Image _iconPanel;

    [SerializeField]
    protected TextMeshProUGUI _textPanel;

    public override void InitSelf()
    {
        base.InitSelf();
    }

    public override void Init()
    {
        base.Init();
    }

    public void UpdateView(Sprite sprite,string text)
    {
        base.UpdateView();
        if (_iconPanel != null) { 
            _iconPanel.sprite = sprite;
        }
        _textPanel.text = text;
    }

}
