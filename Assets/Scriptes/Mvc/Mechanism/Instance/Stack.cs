using FlyEggFrameWork.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : Mechanism
{
   public StackType _stackType = StackType.GRASSSTACK;
    public override void MountModel(ItemModel itemModel)
    {
        base.MountModel(itemModel);
        EnumUtils.TryParseFromStringValue<StackType>(itemModel.GetItemConfig().Name, out _stackType);
    }

}
public enum StackType
{
    [StringValue("GRASSSTACK")]
    GRASSSTACK,
    [StringValue("CHAINSTACK")]
    CHAINSTACK
}

