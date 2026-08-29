using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public class CameraInputController : MonoBehaviour
{
    public enum InputType
    {
        Zoom,
        SwitchCameraType
    }

    public static event Action<InputType, float> CameraInput;
    public static event Action<bool> LookHeld;
    public static event Action<Vector2> MoveInput;

    [Header("Zoom Input")]
    [SerializeField] float _mouseWheelZoomStep = 1f;
    [SerializeField] LevelData _levelData;

    bool _pointerOverUI;
    readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
    PointerEventData _pointerEventData;
    InputAction _lookAction;
    InputAction _moveAction;

    void Update()
    {
        _pointerOverUI = IsPointerOverUI();
    }

    void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        InputActionAsset actions = playerInput != null ? playerInput.actions : null;
        if (actions == null) return;

        _lookAction = actions.FindAction("Look");
        if (_lookAction != null)
        {
            _lookAction.started += OnLook;
            _lookAction.canceled += OnLook;
        }

        _moveAction = actions.FindAction("Move");
        if (_moveAction != null)
        {
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
        }
    }

    void OnDisable()
    {
        if (_lookAction != null)
        {
            _lookAction.started -= OnLook;
            _lookAction.canceled -= OnLook;
            _lookAction = null;
        }

        if (_moveAction != null)
        {
            _moveAction.performed -= OnMove;
            _moveAction.canceled -= OnMove;
            _moveAction = null;
        }
    }

    bool IsPaused => _levelData != null && _levelData.IsPaused;

    public void ChangeCameraZoom(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (IsPaused) return;

        if (context.control.device is Mouse)
        {
            if (_pointerOverUI) return;
            CameraInput?.Invoke(InputType.Zoom, context.ReadValue<float>() * _mouseWheelZoomStep);
        }
        else
        {
            CameraInput?.Invoke(InputType.Zoom, context.ReadValue<float>());
        }
    }

    void OnLook(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            LookHeld?.Invoke(!IsPaused && !_pointerOverUI);
            return;
        }

        if (context.canceled)
            LookHeld?.Invoke(false);
    }

    void OnMove(InputAction.CallbackContext context)
    {
        if (IsPaused)
        {
            MoveInput?.Invoke(Vector2.zero);
            return;
        }
        MoveInput?.Invoke(context.ReadValue<Vector2>());
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        if (Mouse.current == null) return false;

        _pointerEventData ??= new PointerEventData(EventSystem.current);
        _pointerEventData.Reset();
        _pointerEventData.position = Mouse.current.position.ReadValue();
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);
        return _raycastResults.Count > 0;
    }
}
