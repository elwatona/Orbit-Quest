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

    Vector2 _lastMoveValue;

    void Awake()
    {
        CacheReferences();
    }

    void Update()
    {
        if (!_playerData.CanReadInputs) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 cursorWorld = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        cursorWorld.z = 0f;
        Vector2 orbPos = _orbGameObject.transform.position;
        Vector2 direction = new Vector2(cursorWorld.x - orbPos.x, cursorWorld.y - orbPos.y);
        if (direction.sqrMagnitude > 0.0001f)
        {
            _orb.SetAimDirection(direction.normalized);
        }
    }

    void CacheReferences()
    {
        if (!_orbGameObject) _orbGameObject = transform.Find("Orb").gameObject;
        if (!_orb) _orb = _orbGameObject?.GetComponent<Orb>();
    }
    public void Aim(InputAction.CallbackContext context)
    {
        if (!_playerData.CanReadInputs) return;
        if (context.started)
            _orb.SetAiming(true);
        else if (context.canceled)
            _orb.SetAiming(false);
    }
    public void Impulse(InputAction.CallbackContext context)
    {
        if(!context.started || !_playerData.CanReadInputs) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _orb.Impulse(cursorWorldPosition);
    }

    /// <summary>Called by the Inertia Stabilizer input action (Left Ctrl). Toggles inertia stabilizer on/off.</summary>
    public void ToggleInertiaStabilizer(InputAction.CallbackContext context)
    {
        if (!context.started || !_playerData.CanReadInputs) return;
        _orb.ToggleInertiaStabilizer();
    }

    /// <summary>Called by the Move input action. Passes the movement vector to the orb for thrust (used when Apply Thrust is not held).</summary>
    public void Move(InputAction.CallbackContext context)
    {
        _lastMoveValue = context.ReadValue<Vector2>();
        if (!_playerData.CanReadInputs) return;
        _orb.SetThrustInput(_lastMoveValue);
    }

    public void Respawn(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (context.control.device == Keyboard.current && Keyboard.current.altKey.isPressed) 
            _orbGameObject.SetActive(false);
        if (!_orbGameObject.activeSelf)
        {
            _lastMoveValue = Vector2.zero;
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
        string displayName = context.control?.displayName ?? "";
        return displayName switch
        {
            "F2" => PanelEnum.Controls,
            "F3" => PanelEnum.PlayerData,
            _ => PanelEnum.None
        };
    }
}
