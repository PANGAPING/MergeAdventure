using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ElfCloud :Mechanism
{
    public int GetGroup() {
        return Model.IntData;
    }

    protected override void ShowInEditor()
    {
        base.ShowInEditor();

        transform.Find("GroupPanel").gameObject.SetActive(true);
        transform.Find("GroupPanel").GetComponent<TextMeshProUGUI>().text = GetGroup().ToString();
    }
}
