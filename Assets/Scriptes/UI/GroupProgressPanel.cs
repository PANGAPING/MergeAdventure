using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroupProgressPanel : GameUIPanel
{
    [SerializeField]
    private Image _lockerImg;

    [SerializeField]
    private Slider _progressSlider;

    [SerializeField]
    private TextMeshProUGUI _progressText;

    [SerializeField]
    private GroupUnlocker _groupUnlocker;

    public void Mount(GroupUnlocker groupUnlocker) { 
        _groupUnlocker = groupUnlocker;
    }

    public void UpdateView(int have) {
        int need = _groupUnlocker.GetNeedKeyCount();
        _progressSlider.value = (float)have / (float)need;
        _progressText.text = have.ToString() + "/" + need.ToString();
    }
}
