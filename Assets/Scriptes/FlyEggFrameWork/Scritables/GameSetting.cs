using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New GameSetting", menuName = "GameSetting")]
public class GameSetting : ScriptableObject
{
    public float globalVolume = 0f;

    public float bgmVolume = 0f;

    public float sfxVolume = 0f;

    public int graphicLevel = -1;

    public int resolutionIndex = 0;

    public bool fullScreen= true;

    public SystemLanguage language = SystemLanguage.English;
}
