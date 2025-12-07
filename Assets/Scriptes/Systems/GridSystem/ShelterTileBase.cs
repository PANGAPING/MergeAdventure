using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShelterTileBase : TileBase, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    protected Transform _lockedContent;

    [SerializeField]
    protected Transform _unlockedContent;

    [SerializeField]
    protected Slider _slider;
    
    public delegate void EventHandler();
    

    public event EventHandler _onClick;

    public event EventHandler _onHighlight;

    public event EventHandler _onDeHighlight;



    public override void Refresh()
    {
        //base.Refresh();

        if (OccupiedItems == null || OccupiedItems.Count == 0)
        {
            _lockedContent.gameObject.SetActive(true);
            _unlockedContent.gameObject.SetActive(false);
        }
        else {
            _lockedContent.gameObject.SetActive(false);
            _unlockedContent.gameObject.SetActive(true);
        }
    }

    public override void OccupyItem(TileItem item)
    {
        base.OccupyItem(item);
        item.transform.SetParent(transform, false);
        item.transform.localPosition = Vector3.zero;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeHighlight();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (_onClick != null) {
            _onClick.Invoke();
        }

    }

    public virtual void Highlight()
    {
        if (_onHighlight != null) {
            _onHighlight.Invoke(); 
        }
    }


    public virtual void DeHighlight()
    {
        if (_onDeHighlight != null) {
            _onDeHighlight.Invoke(); 
        }
    }


}
