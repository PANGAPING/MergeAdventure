using FlyEggFrameWork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

public class GamepadSystem : GameSystem
{
    protected Canvas _canvas;

    public static GamepadSystem _instance;

    public Mouse activeMouse;
    protected PlayerInput _playerInput;
    protected RectTransform _cursorTransform;

    protected bool lastMousePressing = false;

    public bool mouseIsOverUI = false;


    protected Vector2 lastMousePosition;

    protected Vector2 mousePositionDelta;

    public Action _onClickDown;

    public Action _onClickUp;

    protected override void InitSelf()
    {
        _instance = this;
        base.InitSelf();
        _canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        activeMouse = Mouse.current;
    }
    
    protected override void InitGamePlay()
    {
        base.InitGamePlay();
        _gamePlay.UI.Click.performed += OnClickDown;
        _gamePlay.UI.Click.canceled += OnClickUp;
    }



    protected override void Update()
    {
        base.Update();

         mouseIsOverUI = EventSystem.current.IsPointerOverGameObject();

        Vector2 mousePosition = activeMouse.position.ReadValue();
        mousePositionDelta = (mousePosition - lastMousePosition) / Time.deltaTime;
        lastMousePosition = mousePosition;
    }

    public Vector2 GetMousePositionDelta()
    {
        return mousePositionDelta;
    }

    public Vector2 GetMouseOffsetFromCenter(bool normalized = true)
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        Vector2 mousePosition = activeMouse.position.ReadValue();

        Vector2 offset = mousePosition - screenCenter;

        if (normalized)
        {
            offset.x = offset.x / (Screen.width / 2);
            offset.y = offset.y / (Screen.height / 2);
        }

        return offset;
    }


    public void OnClickDown(InputAction.CallbackContext callbackContext)
    {
        if (_onClickDown != null)
        {
            _onClickDown.Invoke();
        }
    }
    public void OnClickUp(InputAction.CallbackContext callbackContext)
    {
        if (_onClickUp != null)
        {
            _onClickUp.Invoke();
        }
    }


    private void OnDisable()
    {
    }
}
