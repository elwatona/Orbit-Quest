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
        Rotate,
        SwitchCameraType
    }
    public static event Action<InputType, float> CameraInput;

    [Header("Zoom Input")]
    [SerializeField] float _mouseWheelZoomStep = 1f;

    bool _pointerOverUI;
    readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
    PointerEventData _pointerEventData;

    void Update()
    {
        _pointerOverUI = IsPointerOverUI();
    }

    public void ChangeCameraZoom(InputAction.CallbackContext context)
    {
        if (!context.started) return;

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

    public void RotateCamera(InputAction.CallbackContext context)
    {
        CameraInput?.Invoke(InputType.Rotate, context.ReadValue<float>());
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
