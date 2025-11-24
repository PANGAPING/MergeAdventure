using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace FlyEggFrameWork
{
    public class FlyEggInstance : MonoBehaviour
    {
        protected virtual void Awake()
        {
            InitSelf();
        }
        protected virtual void Start()
        {
            Init();
        }

        protected virtual void InitSelf()
        {

        }

        protected virtual void Init()
        {


        }
    }
}
