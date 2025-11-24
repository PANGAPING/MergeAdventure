using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemandItemPanel : MonoBehaviour
{
    [SerializeField]
    protected Image _img;

    [SerializeField]
    protected TextMeshProUGUI _num;
    public virtual void Show(int itemId,int itemNum)
    {
        Image itemImg =_img.GetComponent<Image>();
        itemImg.sprite = ResourceHelper.GetItemSprite(ConfigSystem.GetItemConfig(itemId));
        itemImg.rectTransform.pivot = Vector2.one - ResourceHelper.ConvertSpritePivotToRectTransform(itemImg.sprite);
        itemImg.SetNativeSize();
        
        _num.text = itemNum.ToString();
    }
}
