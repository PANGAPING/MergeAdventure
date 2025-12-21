using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GroupProgressPanel : GameUIPanel
{
    private Image _lockerImg;

    private Slider _progressSlider;

    private TextMeshProUGUI _progressText;


    public void UpdateView(int have,int all) {
        _progressSlider.value = (float)have / (float)all;
        _progressText.text = have.ToString() + "/" +all.ToString();
    }
}
