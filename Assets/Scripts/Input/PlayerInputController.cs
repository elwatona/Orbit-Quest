using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public static event Action<PanelEnum> OnPanelToggled;

    [Header("Settings")]
    [SerializeField] GameObject _orbGameObject;
    [SerializeField] Transform _spawnPoint;
    [SerializeField] Orb _orb;
    [SerializeField] PlayerData _playerData;
    [SerializeField] LevelData _levelData;

    private bool _canReadInputs => _levelData.CurrentState == GameState.Precision && _playerData.IsAlive;

    Vector3 _lastMoveValue;

    void Awake()
    {
        CacheReferences();
    }

    void Update()
    {
        UpdateCursorWorld();
        
        if (!_canReadInputs) return;

        Vector3 orbPos = _orbGameObject.transform.position;
        Vector2 direction = new Vector2(_playerData.CursorWorld.x - orbPos.x, _playerData.CursorWorld.z - orbPos.z);

        if (direction.sqrMagnitude > 0.0001f)
        {
            _orb.SetAimDirection(direction.normalized);
        }
    }
    void UpdateCursorWorld()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        UnityEngine.Camera cam = UnityEngine.Camera.main;
        Ray ray = cam.ScreenPointToRay(screenPos);
        float t = 0f;
        if (Mathf.Abs(ray.direction.y) > 0.0001f)
            t = -ray.origin.y / ray.direction.y; // y=0
        Vector3 cursorWorld = ray.origin + ray.direction * t;
        _playerData.UpdateCursorWorld(cursorWorld);
    }
    void CacheReferences()
    {
        if (!_orbGameObject) _orbGameObject = transform.Find("Orb").gameObject;
        if (!_orb) _orb = _orbGameObject?.GetComponent<Orb>();
    }
    public void Aim(InputAction.CallbackContext context)
    {
        if (!_canReadInputs) return;
        if (context.started)
            _orb.SetAiming(true);
        else if (context.canceled)
            _orb.SetAiming(false);
    }
    public void Impulse(InputAction.CallbackContext context)
    {
        if(!context.started || !_canReadInputs) return;

        Vector3 cursorWorldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _orb.Impulse(cursorWorldPosition);
    }

    /// <summary>Called by the Inertia Stabilizer input action (Left Ctrl). Toggles inertia stabilizer on/off.</summary>
    public void ToggleInertiaStabilizer(InputAction.CallbackContext context)
    {
        if (!context.started || !_canReadInputs) return;
        _orb.ToggleInertiaStabilizer();
    }

    /// <summary>Called by the Move input action. Passes the movement vector to the orb for thrust (used when Apply Thrust is not held).</summary>
    public void Move(InputAction.CallbackContext context)
    {
        Vector2 moveValue = context.ReadValue<Vector2>();

        Vector3 camForward = Vector3.ProjectOnPlane(UnityEngine.Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(UnityEngine.Camera.main.transform.right, Vector3.up).normalized;

        _lastMoveValue = camRight * moveValue.x + camForward * moveValue.y;

        _lastMoveValue = Vector3.ClampMagnitude(_lastMoveValue, 1f);

        if (!_canReadInputs) return;
        _orb.SetThrustInput(_lastMoveValue);
    }

    public void Respawn(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (context.control.device == Keyboard.current && Keyboard.current.altKey.isPressed) 
            _orbGameObject.SetActive(false);
        if (!_orbGameObject.activeSelf)
        {
            _lastMoveValue = Vector3.zero;
            _orbGameObject.transform.position = _spawnPoint.position;
            _orbGameObject.SetActive(true);
        }
    }

    public void TogglePanel(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        PanelEnum panel = GetPanelIndexFromBinding(context);

        if (panel == PanelEnum.None) return;
        
        OnPanelToggled?.Invoke(panel);
    }
    private static PanelEnum GetPanelIndexFromBinding(InputAction.CallbackContext context)
    {
        if (context.action == null || context.control == null) return PanelEnum.None;

        return context.action.GetBindingIndexForControl(context.control) switch
        {
            0 => PanelEnum.Controls,
            1 => PanelEnum.PlayerData,
            2 => PanelEnum.Console,
            _ => PanelEnum.None
        };
    }
}
