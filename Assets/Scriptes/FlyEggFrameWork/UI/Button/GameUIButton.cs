using FlyEggFrameWork.Tools;
using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIButton : GameUIPanel, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event EventHandler _onClick;

    protected bool enable = true;

    [SerializeField]
    protected Color _enableColor = Color.red;

    [SerializeField]
    protected Color _disableColor = Color.gray;

    [SerializeField]
    protected Image _background;

    [SerializeField]
    protected UnityEvent _onActive;

    [SerializeField]
    protected UnityEvent _onDeActive;

    [SerializeField]
    protected UnityEvent _onEnable;

    [SerializeField]
    protected UnityEvent _onDisable;


    public override void InitSelf()
    {
        base.InitSelf();
        if (_background == null)
        {
            _background = GetComponent<Image>();
        }
    }



    public void Mount(string text = "", Sprite sprite = null, bool enable = true)
    {
        if (text.Length > 0)
        {
            Text textComponent = CommonTool.FindComponentInDescendant<Text>(gameObject);
            if (textComponent != null)
            {
                textComponent.text = text;
            }

            TextMeshProUGUI textProComponent = CommonTool.FindComponentInDescendant<TextMeshProUGUI>(gameObject);
            if (textProComponent != null)
            {
                textProComponent.text = text;
            }
        }

        if (sprite != null)
        {
            Image imageComponent = CommonTool.FindComponentInDescendant<Image>(gameObject);
            if (imageComponent != null)
            {
                imageComponent.sprite = sprite;
            }
        }

        if (enable)
        {
            Enable();
        }
        else
        {
            Disable();
        }

        base.Mount();
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!enable)
        {
            return;
        }
        if (_onClick != null)
        {
            _onClick.Invoke();
        }
    }

    public virtual void ClearClickCallback()
    {
        _onClick = null;
    }

    public virtual void Hightlight()
    {

    }


    public virtual void DeHightlight()
    {

    }

    public virtual void Enable()
    {
        if (_background != null)
        {
            _background.color = _enableColor;
        }
        enable = true;
    }

    public virtual void Disable()
    {
        if (_background != null)
        {
            _background.color = _disableColor;
        }
        enable = false;
    }

    public void Active()
    {
        if (_onActive != null)
        {
            _onActive.Invoke();
        }
    }

    public void DeActive()
    {
        if (_onDeActive != null)
        {
            _onDeActive.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Active();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeActive();
    }
}
