using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputController : MonoBehaviour
{
    [SerializeField] CameraManager _cameraManager;

    [Header("Zoom Input")]
    [SerializeField] float _mouseWheelZoomStep = 1f;

    public void ChangeCameraZoom(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        if(context.control.device is Mouse) _cameraManager.Zoom(context.ReadValue<float>() * _mouseWheelZoomStep);
        else _cameraManager.Zoom(context.ReadValue<float>());
    }
    public void SwitchCameraType(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        _cameraManager.ToggleCameraType();
    }
    public void RotateCamera(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        _cameraManager.Rotate(context.ReadValue<float>());
    }
}
