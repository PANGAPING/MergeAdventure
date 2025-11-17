using FlyEggFrameWork.GameGlobalConfig;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public static class GameSettingSystem
{
    public static GameSetting DefaultGameSetting;

    public static GameSetting PlayerGameSetting;

    public static Resolution[] Resolutions;

    public static AudioMixer _audioMixer;

    public static bool Inited = false;

    public static void InitSetting()
    {
        DefaultGameSetting = Resources.Load<GameSetting>(Path.Combine(FoldPath.SettingAssetFolderPath, "PlayerSetting", "DefaultSetting"));
        PlayerGameSetting = Resources.Load<GameSetting>(Path.Combine(FoldPath.SettingAssetFolderPath, "PlayerSetting", "PlayerSetting"));

        Resolutions = Screen.resolutions;
        if (PlayerGameSetting.resolutionIndex < 0)
        {
            for (int i = 0; i < Resolutions.Length; i++)
            {
                Resolution resolution = Resolutions[i];

                if (resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
                {
                    PlayerGameSetting.resolutionIndex = i;
                }
            }
        }

        _audioMixer = Resources.Load<AudioMixer>(Path.Combine(FoldPath.SettingAssetFolderPath,"Sound","MainAudioMixer"));


        SetGlobalVolume(PlayerGameSetting.globalVolume);
        SetBGMVolume(PlayerGameSetting.bgmVolume);
        SetSFXVolume(PlayerGameSetting.sfxVolume);
        SetGraphicLevel(PlayerGameSetting.graphicLevel);
        SetResolution(PlayerGameSetting.resolutionIndex);
        SetFullScreen(PlayerGameSetting.fullScreen);

        Inited = true;
    }

    public static void SetGlobalVolume(float value)
    {
        _audioMixer.SetFloat("MasterVolume", value);
        PlayerGameSetting.sfxVolume = value;
    }

    public static void SetBGMVolume(float value)
    {
        _audioMixer.SetFloat("BGMVolume", value);
        PlayerGameSetting.bgmVolume = value;
    }

    public static void SetSFXVolume(float value)
    {
        _audioMixer.SetFloat("SFXVolume", value);
        PlayerGameSetting.sfxVolume = value;
    }

    public static void SetGraphicLevel(int level)
    {
        QualitySettings.SetQualityLevel(level);
        PlayerGameSetting.graphicLevel = level;
    }

    public static void SetResolution(int resolutionIndex)
    {
        Screen.SetResolution(Resolutions[resolutionIndex].width, Resolutions[resolutionIndex].height, PlayerGameSetting.fullScreen);
        PlayerGameSetting.resolutionIndex = resolutionIndex;
    }

    public static void SetFullScreen(bool fullScreen)
    {
        Screen.fullScreen = fullScreen; 
        PlayerGameSetting.fullScreen = fullScreen;
    }

    public static void SetLanguage(SystemLanguage language) { 
        LocalizationManager.SetLanguage(language);
    }








}
