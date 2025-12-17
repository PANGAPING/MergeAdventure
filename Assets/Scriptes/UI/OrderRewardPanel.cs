using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderRewardPanel : GameUIPanel
{
    protected List<RewardItemPanel> _rewardItemPanels;
    public void UpdateView(int[] itemId, int[] itemNum)
    {
        if (_rewardItemPanels == null) {
            _rewardItemPanels = new List<RewardItemPanel>(); 
        }

    }
}
