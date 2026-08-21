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

    private bool _canReadInputs => _levelData.CurrentState != GameState.Edition && _playerData.IsAlive;

    Vector3 _lastMoveValue;
    InputAction _forceRespawnAction;

    void Awake()
    {
        CacheReferences();
    }

    void OnEnable()
    {
        // PlayerInput UnityEvent for Force Respawn does not invoke at runtime; bind directly.
        var playerInput = GetComponent<PlayerInput>();
        _forceRespawnAction = playerInput != null
            ? playerInput.actions?.FindAction("Force Respawn")
            : null;
        if (_forceRespawnAction != null)
        {
            _forceRespawnAction.started += ForceRespawn;
            _forceRespawnAction.performed += ForceRespawn;
            _forceRespawnAction.canceled += ForceRespawn;
        }
    }

    void OnDisable()
    {
        if (_forceRespawnAction == null) return;
        _forceRespawnAction.started -= ForceRespawn;
        _forceRespawnAction.performed -= ForceRespawn;
        _forceRespawnAction.canceled -= ForceRespawn;
        _forceRespawnAction = null;
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
        if (context.performed)
            _orb.SetAiming(true);
        else if (context.canceled)
            _orb.SetAiming(false);
    }
    public void Impulse(InputAction.CallbackContext context)
    {
        if(!context.performed || !_canReadInputs) return;

        Vector3 cursorWorldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _orb.Impulse(cursorWorldPosition);
    }

    /// <summary>Called by the Inertia Stabilizer input action (Left Ctrl). Toggles inertia stabilizer on/off.</summary>
    public void ToggleInertiaStabilizer(InputAction.CallbackContext context)
    {
        if (!context.performed || !_canReadInputs) return;
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
        if (!context.performed) return;
        if (!_orbGameObject.activeSelf)
            PerformRespawn();
    }

    public void ForceRespawn(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        _orbGameObject.SetActive(false);
        PerformRespawn();
    }

    void PerformRespawn()
    {
        _lastMoveValue = Vector3.zero;
        _orbGameObject.transform.position = _spawnPoint.position;
        _orbGameObject.SetActive(true);
    }

    public void ToggleControls(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnPanelToggled?.Invoke(PanelEnum.Controls);
    }

    public void TogglePlayerData(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnPanelToggled?.Invoke(PanelEnum.PlayerData);
    }

    public void ToggleConsole(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        OnPanelToggled?.Invoke(PanelEnum.Console);
    }
}
