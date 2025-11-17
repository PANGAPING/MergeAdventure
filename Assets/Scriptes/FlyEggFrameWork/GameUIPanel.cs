using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlyEggFrameWork.UI
{
    public class GameUIPanel : MonoBehaviour
    {
        protected Animator _animator;
        public enum PanelState
        {
            Open,
            Hide,
            Close
        }


        public PanelState panelState;

        public delegate void EventHandler();

        public delegate void FloatEventHandler(float value);

        public delegate void IntEventHandler(int value);

        public event EventHandler _onOpen;

        public event EventHandler _onClose;

        public event EventHandler _onHide;

        public bool _isPathPanel = false;

        protected Tree _tree;

        protected CanvasGroup _canvasGroup;

        public virtual void Awake()
        {
            InitSelf();
        }

        public virtual void InitSelf()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

        }


        public virtual void Start()
        {
            Init();
        }

        public virtual void Init()
        {
            _animator = GetComponent<Animator>();
        }

        public virtual void Mount()
        {
        }

        public virtual void Open()
        {
            if (panelState == PanelState.Open)
            {
                return;
            }
            panelState = PanelState.Open;
            gameObject.SetActive(true);
            if (_onOpen != null)
            {
                _onOpen.Invoke();
            }
            if (_isPathPanel)
            {
                SceneUISystem._instance.PushIntoPanelPath(this);
            }

        }

        public virtual void CloseAnimation()
        {
            if (_animator == null)
            {
                Close();
            }
            else
            {
                _animator.SetTrigger("Close");
            }
        }

        public virtual void Close()
        {
            if (panelState == PanelState.Close)
            {
                return;
            }
            panelState = PanelState.Close;
            gameObject.SetActive(false);
            if (_onClose != null)
            {
                _onClose.Invoke();
            }

            if (_isPathPanel)
            {
                SceneUISystem._instance.RemovePanelPathEnd();
            }

        }

        public virtual void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
            }
            else
            {
                gameObject.SetActive(false);
            }

            if (_onHide != null)
            {
                _onHide.Invoke();
            }
        }

        public virtual void Show()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
            }
            else
            {
                gameObject.SetActive(true);
            }

            gameObject.SetActive(true);
        }


        protected virtual void OnDestroy()
        {
        }

    }
}
