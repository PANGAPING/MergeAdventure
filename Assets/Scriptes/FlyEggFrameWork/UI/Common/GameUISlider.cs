using FlyEggFrameWork.Tools;
using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUISlider : GameUIPanel
{
    [SerializeField]
    protected TextMeshProUGUI _labelPanel;

    [SerializeField]
    protected Slider _slider;

    public FloatEventHandler _onValueChange;

    public override void InitSelf()
    {
        base.InitSelf();

        if (_labelPanel != null) {
            _labelPanel = transform.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (_slider != null) {
            _slider = transform.GetComponentInChildren<Slider>();
        }

        _slider.onValueChanged.AddListener(ValueChange);
    }


    public virtual void ValueChange(float value) {
        if (_onValueChange != null) {
            _onValueChange.Invoke(value);
        }
    }

    public virtual void SetValue(float value) { 
        _slider.value = value;
    }

}
