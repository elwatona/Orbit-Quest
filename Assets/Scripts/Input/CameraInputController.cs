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
    public static event Action<InputType, float> CameraInput;

    [Header("Zoom Input")]
    [SerializeField] float _mouseWheelZoomStep = 1f;

    public void ChangeCameraZoom(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        if(context.control.device is Mouse) 
            CameraInput?.Invoke(InputType.Zoom, context.ReadValue<float>() * _mouseWheelZoomStep);
        else CameraInput?.Invoke(InputType.Zoom, context.ReadValue<float>());
    }
    public void RotateCamera(InputAction.CallbackContext context)
    {
        CameraInput?.Invoke(InputType.Rotate, context.ReadValue<float>());
    }
}
