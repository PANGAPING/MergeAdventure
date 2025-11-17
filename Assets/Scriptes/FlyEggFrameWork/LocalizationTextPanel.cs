using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationTextPanel : MonoBehaviour
{
    public string _localizationKey;

    public bool _awakeUpdate = false;

    protected TextMeshProUGUI _tmTextPanel;

    protected Text _textPanel;

    public virtual void Awake()
    {
        InitSelf();

        _textPanel = GetComponent<Text>();
        _tmTextPanel = GetComponent<TextMeshProUGUI>();
        if (_awakeUpdate) { 
            UpdateTextPanel();
        }
    }

    public virtual void SetKey(string key)
    {
        _localizationKey = key;
        UpdateTextPanel();
    }

    public virtual void Start()
    {
        Init();
    }

    protected virtual void InitSelf()
    {

    }

    protected virtual void Init()
    {
        LocalizationManager.OnLanguageChange += UpdateTextPanel;
    }

    protected virtual void UpdateTextPanel()
    {
        if (_tmTextPanel != null)
        {
            _tmTextPanel.text = LocalizationManager.GetLocalizationValue(_localizationKey);
        }

        if (_textPanel != null)
        {
            _textPanel.text = LocalizationManager.GetLocalizationValue(_localizationKey);
        }
    }
}
