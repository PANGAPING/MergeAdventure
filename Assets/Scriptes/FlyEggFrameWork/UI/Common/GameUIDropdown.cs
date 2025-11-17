using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUIDropdown : GameUIPanel
{
    [SerializeField]
    protected TextMeshProUGUI _labelPanel;

    [SerializeField]
    protected TMP_Dropdown _dropdown;

    public IntEventHandler _onValueChange;

    public override void InitSelf()
    {
        base.InitSelf();

        if (_labelPanel != null)
        {
            _labelPanel = transform.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (_dropdown != null)
        {
            _dropdown = transform.GetComponentInChildren<TMP_Dropdown>();
        }

        _dropdown.onValueChanged.AddListener(ValueChange);
    }


    public virtual void ValueChange(int value)
    {
        if (_onValueChange != null)
        {
            _onValueChange.Invoke(value);
        }
    }

    public virtual void ClearOptions() {
        _dropdown.ClearOptions();
    }

    public virtual void AddOptions(List<string> options) {
        _dropdown.AddOptions(options);

    }

    public virtual void SetValue(int value) { 
        _dropdown.value = value;
        _dropdown.RefreshShownValue();
    }

}
