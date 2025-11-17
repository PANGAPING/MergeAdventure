using FlyEggFrameWork.GameGlobalConfig;
using FlyEggFrameWork.GameTypes;
using FlyEggFrameWork.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SceneUISystem : MonoBehaviour
{
    public static SceneUISystem _instance;

    protected List<GameUIPanel> _uIPanelPath = new List<GameUIPanel>();

    protected GamePlay _gamePlay;

    protected InquiryUIPanel _inquiryUIPanel;

    //Option Setting
    protected GameUIPanel _optionsUIPanel;

    protected GameUIPanel _graphicOptionsUIPanel;

    protected GameUIPanel _languageOptionsUIPanel;

    protected GameUIPanel _soundOptionsUIPanel;

    protected GameUIPanel _storageChooseUIPanel;

    public delegate void EventHandler();


    protected virtual void Awake()
    {
        _instance = this;
        InitSelf();
    }

    protected virtual void Start()
    {
        Init();
    }

    protected virtual void InitSelf()
    {
        LoadDataFromSaveSystem();
        InitGamePlay();
        InitializeSystemUI();
    }

    protected virtual void Init()
    {

    }

    protected virtual void LoadDataFromSaveSystem()
    {


    }

    protected virtual void InitGamePlay()
    {
        _gamePlay = new GamePlay();
        _gamePlay.Enable();
    }

    protected virtual void InitializeSystemUI()
    {
        _inquiryUIPanel = (InquiryUIPanel)InitializeUIPanel("InquiryUIPanel/InquiryUIPanel", "Middle");
        _inquiryUIPanel.Close();

        _optionsUIPanel = InitializeUIPanel("OptionsUIPanel/OptionsUIPanel", "Middle");
        _optionsUIPanel.Close();

        _soundOptionsUIPanel = InitializeUIPanel("OptionsUIPanel/SoundOptionsUIPanel", "Middle");
        _soundOptionsUIPanel.Close();

        _graphicOptionsUIPanel = InitializeUIPanel("OptionsUIPanel/GraphicOptionsUIPanel", "Middle");
        _graphicOptionsUIPanel.Close();

        _languageOptionsUIPanel = InitializeUIPanel("OptionsUIPanel/LanguageOptionsUIPanel", "Middle");
        _languageOptionsUIPanel.Close();

        _storageChooseUIPanel = InitializeUIPanel("StorageChooseUIPanel/StorageChooseUIPanel", "Middle");
        _storageChooseUIPanel.Close();
    }
    public static GameObject InitializeGameUI(GameObject prefab, UILayer uiLayer)
    {
        string canvasLayer = "";
        if (uiLayer == UILayer.Bottom)
        {
            canvasLayer = "Bottom";
        }
        else if (uiLayer == UILayer.Middle)
        {
            canvasLayer = "Middle";
        }
        else if (uiLayer == UILayer.Top)
        {
            canvasLayer = "Top";
        }

        GameObject uiInstance = Instantiate(prefab, GameObject.Find("Canvas").transform.Find(canvasLayer));
        return uiInstance;
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

    public static GameObject LoadUIPanelResource(string path)
    {
        string panelPath = Path.Combine(FoldPath.UIPrefabFolderPath, path);
        GameObject panelPrefabObject = Resources.Load<GameObject>(panelPath);

        return panelPrefabObject;
    }
    public virtual void OpenStorageChoosePanel()
    {
        _storageChooseUIPanel.Open();
    }

    public virtual void OpenOptionsPanel()
    {
        _optionsUIPanel.Open();
    }

    public virtual void OpenSoundOptionsPanel()
    {
        _soundOptionsUIPanel.Open();

    }

    public virtual void OpenGraphicOptionsPanel()
    {
        _graphicOptionsUIPanel.Open();
    }

    public virtual void OpenLanguageOptionsPanel()
    {
        _languageOptionsUIPanel.Open();
    }

    public void Inquiry(string inquiryText = "", GameUIPanel.EventHandler sureCallback = null, GameUIPanel.EventHandler cancelCallback = null, string sureBtnText = "", string cancelBtnText = "")
    {

        _inquiryUIPanel.Inquiry(inquiryText, sureCallback, cancelCallback);
        _inquiryUIPanel.Open();
    }

    public virtual void PushIntoPanelPath(GameUIPanel uiPanel)
    {
        if (_uIPanelPath.Count > 0)
        {
            _uIPanelPath[_uIPanelPath.Count - 1].Hide();
        }

        _uIPanelPath.Add(uiPanel);
    }

    public virtual void PopPanel()
    {

        if (_uIPanelPath.Count > 0)
        {
            _uIPanelPath[_uIPanelPath.Count - 1].Show();
        }
    }

    public virtual void RemovePanelPathEnd()
    {
        if (_uIPanelPath.Count > 0)
        {
            _uIPanelPath.RemoveAt(_uIPanelPath.Count - 1);
        }
        PopPanel();
    }

}
