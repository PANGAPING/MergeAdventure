using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GroupUnlocker : Mechanism
{
    public int _group;

    public void SetUnlockerGroup(int group) {
        _group = group; 
    }
    public int GetUnlockerGroup() { 
        return _group;
    }

    public int GetNeedKeyCount()
    {
        return Model.IntData;
    }

    protected override void ShowInEditor()
    {
        base.ShowInEditor();

        transform.Find("GroupPanel").gameObject.SetActive(true);
        transform.Find("GroupPanel").GetComponent<TextMeshProUGUI>().text =GetNeedKeyCount().ToString();
    }
}
