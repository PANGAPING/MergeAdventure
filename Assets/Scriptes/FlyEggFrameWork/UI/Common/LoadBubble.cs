using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadBubble : GameUIPanel
{
    protected Slider _loadSlider;

    protected Transform _followPoint;

    protected RectTransform _rectTransform;

    public override void InitSelf()
    {
        base.InitSelf();
    }

    public virtual void Mount(Transform followPoint)
    {
        _loadSlider = GetComponent<Slider>();
        _followPoint = followPoint;
        _rectTransform = GetComponent<RectTransform>();
    }

    protected virtual void Update()
    {
        if (_followPoint != null)
        {

            Vector2 targetScreenPos = Camera.main.WorldToScreenPoint(_followPoint.position);
            _rectTransform.position = targetScreenPos;
        }
        else {
            GetDestory();
        }

    }


    public virtual void SetValue(float value)
    {
        _loadSlider.value = value;
    }

    public virtual void GetDestory()
    {
        GameObject.Destroy(gameObject);
    }
}
