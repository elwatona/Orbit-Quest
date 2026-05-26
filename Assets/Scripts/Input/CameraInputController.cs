using UnityEngine;
using UnityEngine.InputSystem;
using System;
public class CameraInputController : MonoBehaviour
{
    public enum InputType
    {
        Zoom,
        Rotate,
        SwitchCameraType
    }
    public static event Action<InputType, float> OnCameraInput;

    [Header("Zoom Input")]
    [SerializeField] float _mouseWheelZoomStep = 1f;

    public void ChangeCameraZoom(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        if(context.control.device is Mouse) OnCameraInput?.Invoke(InputType.Zoom, context.ReadValue<float>() * _mouseWheelZoomStep);
        else OnCameraInput?.Invoke(InputType.Zoom, context.ReadValue<float>());
    }
    public void SwitchCameraType(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        OnCameraInput?.Invoke(InputType.SwitchCameraType, 0);
    }
    public void RotateCamera(InputAction.CallbackContext context)
    {
        OnCameraInput?.Invoke(InputType.Rotate, context.ReadValue<float>());
    }
}
