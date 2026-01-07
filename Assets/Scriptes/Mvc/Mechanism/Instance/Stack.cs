using FlyEggFrameWork.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : Mechanism
{
    StackType _stackType = StackType.STACK;
    public override void MountModel(ItemModel itemModel)
    {
        EnumUtils.TryParseFromStringValue<StackType>(itemModel.GetItemConfig().Name, out _stackType);
    }

}
public enum StackType
{
    [StringValue("STACK")]
    STACK,
    [StringValue("CHAINSTACK")]
    CHAINSTACK
}

