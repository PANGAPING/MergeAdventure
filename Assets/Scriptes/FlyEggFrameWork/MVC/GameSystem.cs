using FlyEggFrameWork.GameTypes;
using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FlyEggFrameWork
{
    public abstract class GameSystem : MonoBehaviour
    {

        public enum SystemState
        {
            Open,
            Close
        }

        public SystemState systemState;

        protected GameSystemUIPanel _panel;

        public UILayer _panelLayer = UILayer.Middle;

        protected GamePlay _gamePlay;

        protected string _systemUIPanelName = "";

        protected bool _openDefault = true;

        public delegate void EventHandler();

        public event EventHandler _onOpenSystem;

        public event EventHandler _onCloseSystem;

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
            LoadDataFromSaveSystem();
            InitializeResources();
            InitGamePlay();
            InitializeGameSystemUI();
        }

        protected virtual void LoadDataFromSaveSystem()
        {


        }

        protected virtual void InitGamePlay()
        {
            _gamePlay = new GamePlay();
        }

        protected virtual void InitializeGameSystemUI()
        {
            if (_systemUIPanelName.Length == 0)
            {
                return;
            }

            string panelPath = Path.Combine(GameGlobalConfig.FoldPath.GameSystemUIFolderPath, _systemUIPanelName);
            GameObject panelPrefabObject = Resources.Load<GameObject>(panelPath);

            if (panelPrefabObject == null)
            {
                Debug.LogError("There is no object at " + panelPath);
            }
            string canvasLayer = "Middle";
            if (_panelLayer == UILayer.Bottom)
            {
                canvasLayer = "Bottom";
            }
            else if (_panelLayer == UILayer.Middle)
            {
                canvasLayer = "Middle";
            }
            else if (_panelLayer == UILayer.Top)
            {
                canvasLayer = "Top";
            }

            GameObject panelInstance = GameObject.Instantiate(panelPrefabObject, GameObject.Find("Canvas").transform.Find(canvasLayer));
            _panel = panelInstance.GetComponent<GameSystemUIPanel>();
            if (_panel != null)
            {
                _panel.Mount();
            }
        }

        protected virtual GameUIPanel InitializeWorldUIPanel(string path, string canvasLayer = "Top")
        {
            GameObject prefab = LoadUIPanelResource(path);
            GameObject panelInstance = GameObject.Instantiate(prefab, GameObject.Find("WorldCanvas").transform.Find(canvasLayer));

            panelInstance.GetComponent<Canvas>().worldCamera = Camera.main;

            panelInstance.transform.localPosition = Vector3.zero;

            GameUIPanel panel = panelInstance.GetComponent<GameUIPanel>();
            if (panel != null)
            {
                panel.Mount();
            }

            return panel;

        }

        protected virtual GameUIPanel InitializeUIPanel(string path, string canvasLayer = "Top")
        {
            GameObject prefab = LoadUIPanelResource(path);
            GameObject panelInstance = GameObject.Instantiate(prefab, GameObject.Find("Canvas").transform.Find(canvasLayer));
            GameUIPanel panel = panelInstance.GetComponent<GameUIPanel>();
            if (panel != null)
            {
                panel.Mount();
            }

            return panel;
        }

        protected virtual GameObject InitializeUIObj(string path, string canvasLayer = "Top")
        {
            GameObject prefab = LoadUIPanelResource(path);
            GameObject panelInstance = GameObject.Instantiate(prefab, GameObject.Find("Canvas").transform.Find(canvasLayer));
            return panelInstance;
        }

        protected virtual GameObject LoadUIPanelResource(string path)
        {
            string panelPath = Path.Combine(GameGlobalConfig.FoldPath.UIPrefabFolderPath, path);
            GameObject panelPrefabObject = Resources.Load<GameObject>(panelPath);

            return panelPrefabObject;
        }

        protected virtual void InitializeResources()
        {


        }

        protected virtual void Init()
        {
            if (_openDefault)
            {

                Open();
            }
            else
            {
                Close();
            }
        }


        protected virtual void Update()
        {


        }

        public virtual void Open()
        {
            this.enabled = true;
            _gamePlay.Enable();
            systemState = SystemState.Open;
            if (_onOpenSystem != null)
            {
                _onOpenSystem.Invoke();
            }
        }

        public virtual void Close()
        {
            this.enabled = false;
            _gamePlay.Disable();
            if (_panel != null)
            {
                _panel.Close();
            }
            systemState = SystemState.Close;

            if (_onCloseSystem != null)
            {
                _onCloseSystem.Invoke();
            }
        }

    }
}
