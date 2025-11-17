using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FlyEggFrameWork.UI
{
    public abstract class GameSystemUIPanel : GameUIPanel
    {
        [HideInInspector]
        public GameSystem _GameSystem;

        public override void Init()
        {
            base.Init();
        }
    }
}
