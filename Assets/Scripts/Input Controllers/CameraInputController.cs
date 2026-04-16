using UnityEngine;
using UnityEngine.InputSystem;

public class CameraInputController : MonoBehaviour
{
    [SerializeField] CameraController _cameraController;

    [Header("Zoom Input")]
    [SerializeField] float _mouseWheelZoomStep = 1f;

    void Awake()
    {
        if (!_cameraController) _cameraController = Camera.main.GetComponent<CameraController>();
    }

    public void ChangeCameraZoom(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        
        if(context.control.device is Mouse) _cameraController.AddZoomDelta(context.ReadValue<float>() * _mouseWheelZoomStep);
        else _cameraController.AddZoomDelta(context.ReadValue<float>());
    }
}
