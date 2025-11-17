using FlyEggFrameWork.GameGlobalConfig;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public static class LocalizationManager
{

    public static Dictionary<string, string> LocalizationMap;

    public static SystemLanguage CurLanguage;

    public static bool LanguageSeted = false;

    public delegate void EventHandler();

    public static EventHandler OnLanguageChange;

    public static void SetLanguage(string languageName)
    {
        SystemLanguage language;

        if (System.Enum.TryParse<SystemLanguage>(languageName, out language))
        {
            SetLanguage(language);
        }
        else
        {
            LocalizationMap = new Dictionary<string, string>();
        }

        LanguageSeted = true;
    }

    public static void SetLanguage(SystemLanguage language)
    {
        LocalizationMap = new Dictionary<string, string>();

        CurLanguage = language;

        string languageName = language.ToString();

        string languageFilePath = Path.Combine(FoldPath.LanguageFilesFolderPath, languageName);

        TextAsset languageTextAsset = Resources.Load(languageFilePath) as TextAsset;
        string languageText = languageTextAsset.text;

        string[] lines = languageText.Split('\n');
        foreach (string line in lines)
        {
            string row = line.Trim();
            string[] split = row.Split('&');
            if (split.Length > 1)
            {
                string key = split[0].Trim();
                string value = split[1];
                LocalizationMap[key] = value;
            }
        }

        if (OnLanguageChange != null) {
            OnLanguageChange.Invoke(); 
        }
    }


    public static string GetLocalizationValue(string key)
    {
        string value = LocalizationMap[key];

        if (value == null)
        {
            value = "{" + key + "}";
        }

        return value;
    }

    public static List<SystemLanguage> GetAvailableLanguages()
    {
        List<SystemLanguage> availableLanguages = new List<SystemLanguage>();

        string languageFilesFold = Path.Combine(Application.dataPath, "Resources", "Localization", "LanguageFiles");
        DirectoryInfo di = new DirectoryInfo(languageFilesFold);
        foreach (FileInfo file in di.GetFiles())
        {
            string fileName = file.Name.Replace(file.Extension, "");
            SystemLanguage language;

            if (System.Enum.TryParse<SystemLanguage>(fileName, out language))
            {
                availableLanguages.Add(language);
            }
        }

        return availableLanguages;
    }
}
