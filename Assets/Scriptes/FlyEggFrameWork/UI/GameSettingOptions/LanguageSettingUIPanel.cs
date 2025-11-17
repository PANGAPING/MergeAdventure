using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSettingUIPanel : GameUIPanel
{
    public static LanguageSettingUIPanel _instance;

    protected List<SystemLanguage> _availableLanguages;

    protected GameUIDropdown _languageDropdown;
    public override void InitSelf()
    {
        _instance = this;

        _isPathPanel = true;
        base.InitSelf();

        GameUIButton backButton = transform.Find("BackButton").GetComponent<GameUIButton>();
        backButton._onClick += CloseAnimation;

        InitLanguageDropdown();

        GameUIButton systemButton = transform.Find("SystemButton").GetComponent<GameUIButton>();
        systemButton._onClick += SetToSystemLanguage;
    }

    protected virtual void SetToSystemLanguage()
    {
        SystemLanguage systemLanguage = Application.systemLanguage;

        if (_availableLanguages.Contains(systemLanguage))
        {
            _languageDropdown.SetValue(_availableLanguages.IndexOf(systemLanguage));
        }
        else { 
            _languageDropdown.SetValue(_availableLanguages.IndexOf(SystemLanguage.English));
        }

    }


    protected virtual void InitLanguageDropdown()
    {
        Transform optionsContainer = transform.Find("Options");

        GameUIDropdown languageDropdown = optionsContainer.Find("LanguageDropdown").GetComponent<GameUIDropdown>();


        languageDropdown.ClearOptions();

        _availableLanguages = LocalizationManager.GetAvailableLanguages();

        List<string> options = new List<string>();
        foreach (SystemLanguage language in _availableLanguages)
        {
            options.Add(language.ToString());
        }
        languageDropdown.AddOptions(options);

        languageDropdown.SetValue(_availableLanguages.IndexOf(GameSettingSystem.PlayerGameSetting.language));

        languageDropdown._onValueChange += (int index) => { GameSettingSystem.SetLanguage(_availableLanguages[index]); };

        _languageDropdown = languageDropdown;
    }
}
