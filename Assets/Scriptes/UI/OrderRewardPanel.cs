using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OrderRewardPanel : GameUIPanel
{
    protected List<RewardItemPanel> _rewardItemPanels;
    public void Mount(OrderModel orderModel) {
        int[] rewardTypes = orderModel.RewardItemType;
        int[] rewardNums = orderModel.RewardItemNum;
        
        _rewardItemPanels = new List<RewardItemPanel>();
        GameObject rewardItemPrefab = ResourceHelper.GetUIPrefab("RewardItemPanel");

        for (int i = 0; i < rewardTypes.Length; i++) {
            GameObject rewardItemObj = GameObject.Instantiate(rewardItemPrefab, transform);    
            RewardItemPanel rewardItemPanel = rewardItemObj.GetComponent<RewardItemPanel>();
            int rewardType = rewardTypes[i];
            int rewardNum = rewardNums[i];

            AssetConfig assetConfig = ConfigSystem.GetAssetConfig(rewardType);
            Sprite assetSprite = null;
            if (assetConfig != null)
            {
                assetSprite = ResourceHelper.GetAssetSprite(assetConfig);
            }

            rewardItemPanel.UpdateView(assetSprite, rewardNum.ToString());
            _rewardItemPanels.Add(rewardItemPanel);
        }
    }

}
