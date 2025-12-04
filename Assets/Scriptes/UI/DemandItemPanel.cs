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

    protected int needId;
    protected int needNum;
    public virtual void Init(int itemId,int itemNum)
    {
        Image itemImg =_img.GetComponent<Image>();

        ItemConfig config = ConfigSystem.GetItemConfig(itemId);
        if (config != null) {
            itemImg.sprite = ResourceHelper.GetItemSprite(config);
        }
        _num.text = itemNum.ToString();
        needId = itemId;
        needNum = itemNum;
    }

    public int GetNeedItemId() { 
        return needId;
    }

    public virtual void UpdateView(int have) {
        _num.text = have.ToString() + "/" + needNum.ToString();
    }
}
