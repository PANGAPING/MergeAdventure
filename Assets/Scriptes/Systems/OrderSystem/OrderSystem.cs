using FlyEggFrameWork;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OrderSystem : GameSystem
{
    public static OrderSystem _instance;

    public EventHandler _onOrderChange;
    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();
    }

    protected override void Init()
    {
        base.Init();
    }

    private void OrderChangeEvent()
    {
        if (_onOrderChange != null)
        {
            _onOrderChange.Invoke();
        }
    }
}
