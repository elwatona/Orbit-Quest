using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    const string ActionMapPlayer = "Player";
    const string ActionMousePosition = "Mouse Position";

    [Header("Astro Creation")]
    [SerializeField] AstroFactory _astroFactory;

    [Header("Settings")]
    [SerializeField] GameObject _orbGameObject;
    [SerializeField] Orb _orb;
    [SerializeField] UIManager _uiManager;
    [SerializeField] PlayerInput _playerInput;

    InputAction _mousePositionAction;
    Vector2 _lastMoveValue;
    bool _applyThrustHeld;

    void Awake()
    {
        CacheReferences();
        if (!_playerInput) _playerInput = GetComponent<PlayerInput>();
        if (_playerInput != null)
            _mousePositionAction = _playerInput.actions.FindAction(ActionMapPlayer + "/" + ActionMousePosition, true);
    }

    void Update()
    {
        if (_orb == null || !_orbGameObject.activeSelf) return;
        if (_mousePositionAction == null || !_mousePositionAction.enabled) return;

        Vector2 screenPos = _mousePositionAction.ReadValue<Vector2>();
        Vector3 cursorWorld = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        cursorWorld.z = 0f;
        Vector2 orbPos = _orbGameObject.transform.position;
        Vector2 direction = new Vector2(cursorWorld.x - orbPos.x, cursorWorld.y - orbPos.y);
        if (direction.sqrMagnitude > 0.0001f)
        {
            _orb.SetAimDirection(direction.normalized);
            if (_applyThrustHeld)
                _orb.SetThrustInput(direction.normalized);
        }
    }

    void CacheReferences()
    {
        if (!_orbGameObject) _orbGameObject = transform.Find("Orb").gameObject;
        if (!_orb) _orb = _orbGameObject?.GetComponent<Orb>();
    }

    /// <summary>Called by the Apply Thrust input action (right click). Wire Player/Apply Thrust (Started and Canceled) to this. While held, thrust is applied continuously towards the cursor.</summary>
    public void ApplyThrust(InputAction.CallbackContext context)
    {
        if (!_orbGameObject.activeSelf || _orb == null) return;

        if (context.started)
            _applyThrustHeld = true;
        else if (context.canceled)
        {
            _applyThrustHeld = false;
            _orb.SetThrustInput(_lastMoveValue);
        }
    }

    public void SetSpawnPoint(InputAction.CallbackContext context)
    {
        if(_orbGameObject.activeSelf || !context.started) return;
        
        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        gameObject.transform.position = cursorWorldPosition;
    }
    public void Aim(InputAction.CallbackContext context)
    {
        if (context.started)
            _orb.SetAiming(true);
        else if (context.canceled)
            _orb.SetAiming(false);
    }
    public void Loose(InputAction.CallbackContext context)
    {
        if(!context.started) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _orb.Loose(cursorWorldPosition);
    }

    /// <summary>Called by the Move input action. Passes the movement vector to the orb for thrust (used when Apply Thrust is not held).</summary>
    public void Move(InputAction.CallbackContext context)
    {
        _lastMoveValue = context.ReadValue<Vector2>();
        if (_orb != null && !_applyThrustHeld)
            _orb.SetThrustInput(_lastMoveValue);
    }

    public void Respawn(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (HasModifiers(context)) _orbGameObject.SetActive(false);
        if (!_orbGameObject.activeSelf)
        {
            _orbGameObject.transform.localPosition = Vector3.zero;
            _orbGameObject.SetActive(true);
        }
    }

    /// <summary>True if the input that triggered the action had Shift, Ctrl or Alt pressed.</summary>
    private static bool HasModifiers(InputAction.CallbackContext context)
    {
        var keyboard = context.control?.device as Keyboard;
        if (keyboard == null)
            keyboard = Keyboard.current;
        if (keyboard == null)
            return false;
        return keyboard.altKey.isPressed;
    }
    public void CreateAstro(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (_astroFactory == null) return;

        AstroType type = GetAstroTypeFromBinding(context);
        if (type == AstroType.None) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0f;

        _astroFactory.Create(type, cursorWorldPosition);
    }
    public void TogglePanel(InputAction.CallbackContext context)
    {
        if(!context.started) return;
        int panelIndex = GetPanelIndexFromBinding(context);
        if(panelIndex == -1) return;
        _uiManager.TogglePanel(panelIndex);
    }

    /// <summary>
    /// Obtiene el AstroType según el control que disparó la acción (p. ej. tecla 1=Planet, 2=Asteroid, 3=Sun).
    /// </summary>
    private static AstroType GetAstroTypeFromBinding(InputAction.CallbackContext context)
    {
        string displayName = context.control?.displayName ?? "";
        return displayName switch
        {
            "1" => AstroType.Planet,
            "2" => AstroType.Asteroid,
            "3" => AstroType.Sun,
            _ => AstroType.Planet
        };
    }
    private static int GetPanelIndexFromBinding(InputAction.CallbackContext context)
    {
        string displayName = context.control?.displayName ?? "";
        return displayName switch
        {
            "F2" => 0,
            "F3" => 1,
            _ => -1
        };
    }
}
