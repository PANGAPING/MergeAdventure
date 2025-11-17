using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountSelector : GameUIPanel
{
    protected int countMinLimit=1;
    protected int countMaxLimit=10;

    protected TextMeshProUGUI _countPanel;

    protected int countValue = 1;

    public EventHandler _onChangeCount;

    public override void InitSelf()
    {
        base.InitSelf();

        _countPanel = transform.Find("CountValue").GetComponent<TextMeshProUGUI>();

        GameUIButton addButton = transform.Find("CountChange/Add").GetComponent<GameUIButton>();
        GameUIButton subButton = transform.Find("CountChange/Sub").GetComponent<GameUIButton>();

        addButton._onClick += () => { CountChange(1); };
        subButton._onClick += () => { CountChange(-1); };
    }

    public int GetCountValue()
    {
        return countValue;
    }

    public virtual void SetLimit(int minValue, int maxValue)
    {
        countMaxLimit = maxValue;
        countMinLimit = minValue;
        CountChange(0);
    }

    public virtual void CountChange(int offset)
    {
        countValue += offset;
        if (countValue < countMinLimit)
        {
            countValue = countMinLimit;
        }

        if (countValue > countMaxLimit)
        {
            countValue = countMaxLimit;
        }
        SetCount(countValue);
    }

    public virtual void SetCount(int value)
    {
        countValue = value;
        _countPanel.text = countValue.ToString();
        if (_onChangeCount != null) {
            _onChangeCount.Invoke();
        }
    }

}
