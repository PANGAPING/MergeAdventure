using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InquiryUIPanel : GameUIPanel
{

    [Header("Panel")]
    [Space]

    [SerializeField]
    protected TextMeshProUGUI _inquiryTextPanel;

    [SerializeField]
    protected GameUIButton _sureButton;

    [SerializeField]
    protected GameUIButton _cancelButton;

    public override void InitSelf()
    {
        _isPathPanel = true;
        base.InitSelf();
    }


    public override void Open()
    {
        base.Open();
    }

    public override void Close()
    {
        base.Close();
    }


    public void Inquiry(string inquiryText = "", EventHandler sureCallback = null, EventHandler cancelCallback = null, string sureBtnText = "", string cancelBtnText = "")
    {

        _sureButton.ClearClickCallback();
        _cancelButton.ClearClickCallback();

        _sureButton._onClick += sureCallback;
        _cancelButton._onClick += cancelCallback;
        _cancelButton._onClick += CloseAnimation;

        _inquiryTextPanel.text = inquiryText;

        if (sureBtnText.Length > 0)
        {
            _sureButton.Mount(sureBtnText);
        }
        else { 
            _sureButton.Mount("Sure");
        }

        if (cancelBtnText.Length > 0)
        {
            _cancelButton.Mount(cancelBtnText);
        }
        else { 
            _cancelButton.Mount("Cancel");
        }

    }
}
