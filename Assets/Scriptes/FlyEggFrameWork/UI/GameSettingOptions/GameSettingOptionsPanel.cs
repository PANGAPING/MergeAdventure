using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettingOptionsPanel : GameUIPanel
{
    public override void InitSelf()
    {
        _isPathPanel = true;
        base.InitSelf();

        GameUIButton backButton = transform.Find("BackButton").GetComponent<GameUIButton>();
        backButton._onClick += CloseAnimation;


    }

    public override void Init()
    {
        base.Init();
        Transform optionsButtonContainer = transform.Find("Options");

        GameUIButton soundButton = optionsButtonContainer.Find("Sound").GetComponent<GameUIButton>();
        soundButton._onClick += SoundSettingUIPanel._instance.Open;
        GameUIButton graphicButton = optionsButtonContainer.Find("Graphic").GetComponent<GameUIButton>();
        graphicButton._onClick += GraphicSettingPanel._instance.Open;
        GameUIButton languageButton = optionsButtonContainer.Find("Language").GetComponent<GameUIButton>();
        languageButton._onClick += LanguageSettingUIPanel._instance.Open;
    }




}
