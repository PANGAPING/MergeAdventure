using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicSettingPanel : GameUIPanel
{
    public static GraphicSettingPanel _instance;
    public override void InitSelf()
    {
        _instance = this;

        _isPathPanel = true;
        base.InitSelf();

        GameUIButton backButton = transform.Find("BackButton").GetComponent<GameUIButton>();
        backButton._onClick += CloseAnimation;

        Transform optionsContainer = transform.Find("Options");

        GameUIDropdown resolutionDropdown = optionsContainer.Find("Resolution").GetComponent<GameUIDropdown>();

        Resolution[] resolutions = GameSettingSystem.Resolutions;

        List<string> resolutionOptions = new List<string>();

        for (int i = 0; i < resolutions.Length; i++) { 
            string resolutionOption = resolutions[i].width.ToString() +"x" +resolutions[i].height.ToString();
            resolutionOptions.Add(resolutionOption);
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.SetValue(GameSettingSystem.PlayerGameSetting.resolutionIndex);

        resolutionDropdown._onValueChange += GameSettingSystem.SetResolution;


        GameUIDropdown fullScreenDropdown = optionsContainer.Find("FullScreen").GetComponent<GameUIDropdown>();
        fullScreenDropdown._onValueChange += (int value) =>
        {
            if (value == 0)
            {
                GameSettingSystem.SetFullScreen(true);
            }
            else
            {
                GameSettingSystem.SetFullScreen(false);
            }
        };

        if (GameSettingSystem.PlayerGameSetting.fullScreen)
        {
            fullScreenDropdown.SetValue(0);
        }
        else { 
            fullScreenDropdown.SetValue(1);
        }

        GameUIDropdown qualityDropdown = optionsContainer.Find("Quality").GetComponent<GameUIDropdown>();
        qualityDropdown._onValueChange += GameSettingSystem.SetGraphicLevel;

        qualityDropdown.SetValue(GameSettingSystem.PlayerGameSetting.graphicLevel);
    }
}
