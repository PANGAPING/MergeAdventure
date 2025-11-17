using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace FlyEggFrameWork
{

    public class GameProgressController : MonoBehaviour
    {
        protected GameSystem[] _gameSystem;

        public delegate void EventHandler();

        public delegate void IntEventHandler(int i);

        protected virtual void Awake()
        {
            InitProgress();
        }
        protected virtual void InitProgress()
        {
            if (!GameSettingSystem.Inited)
            {
                GameSettingSystem.InitSetting();
            }
            if (!LocalizationManager.LanguageSeted)
            {
                //LocalizationManager.SetLanguage(GameSettingSystem.PlayerGameSetting.language);
            }
            LoadAllSystems();
        }

        protected virtual void LoadAllSystems()
        {
            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in allTypes)
            {
                if (type.IsSubclassOf(typeof(GameSystem)))
                {
                    GameObject systemMounter = new GameObject(type.Name);
                    systemMounter.transform.SetParent(transform);
                    systemMounter.AddComponent(type);
                }
            }

        }


        public virtual void TryQuitGame() {
            SceneUISystem._instance.Inquiry("Quit Game?", QuitGame);
        }

        public virtual void QuitGame() { 
            Application.Quit();
        }

    }
}
