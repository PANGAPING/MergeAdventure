using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSettingUIPanel : GameUIPanel
{
    public static SoundSettingUIPanel _instance;

    public override void InitSelf()
    {
        _instance = this;

        _isPathPanel = true;
        base.InitSelf();

        GameUIButton backButton = transform.Find("BackButton").GetComponent<GameUIButton>();
        backButton._onClick += CloseAnimation;

        Transform optionsContainer = transform.Find("Options");

        GameUISlider globalVolumeSlider = optionsContainer.Find("Global").GetComponent<GameUISlider>();
        globalVolumeSlider._onValueChange += GameSettingSystem.SetGlobalVolume;
        globalVolumeSlider.SetValue(GameSettingSystem.PlayerGameSetting.globalVolume);

        GameUISlider bgmVolumeSlider = optionsContainer.Find("BGM").GetComponent<GameUISlider>();
        bgmVolumeSlider._onValueChange += GameSettingSystem.SetBGMVolume;
        bgmVolumeSlider.SetValue(GameSettingSystem.PlayerGameSetting.bgmVolume);


        GameUISlider sfxVolumeSlider = optionsContainer.Find("SFX").GetComponent<GameUISlider>();
        sfxVolumeSlider._onValueChange += GameSettingSystem.SetSFXVolume;
        sfxVolumeSlider.SetValue(GameSettingSystem.PlayerGameSetting.sfxVolume);
    }
}
