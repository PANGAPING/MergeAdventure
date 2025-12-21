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
    private GameObject _unlockBtn;

    private int _need;

    public void Mount(int need) { 
        _need = need;
    }

    public void UpdateView(int have) {
        int need = _need;
        _progressSlider.value = (float)have / (float)need;
        _progressText.text = have.ToString() + "/" + need.ToString();
        if (have >= need)
        {
            _progressSlider.gameObject.SetActive(false);
            _unlockBtn.SetActive(true);
        }
        else {
            _progressSlider.gameObject.SetActive(true);
            _unlockBtn.SetActive(false);
        }
    }
}
