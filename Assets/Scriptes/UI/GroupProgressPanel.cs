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
    private GameUIButton _unlockBtn;

    private int _need;

    private int _group;

    public void Mount(int need,int group) { 
        _need = need;
        _group = group;

        _unlockBtn._onClick += () =>
        {
            GridControllerSystem._instance.UnlockGroup(_group);
        };
    }

    public void UpdateView(int have) {
        int need = _need;
        _progressSlider.value = (float)have / (float)need;
        _progressText.text = have.ToString() + "/" + need.ToString();
        if (have >= need)
        {
            _progressSlider.gameObject.SetActive(false);
            _unlockBtn.gameObject.SetActive(true);
        }
        else {
            _progressSlider.gameObject.SetActive(true);
            _unlockBtn.gameObject.SetActive(false);
        }
    }
}
