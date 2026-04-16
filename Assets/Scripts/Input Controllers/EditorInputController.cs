using UnityEngine;
using UnityEngine.InputSystem;

public class EditorInputController : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    [SerializeField] GameObject _orbGameObject;
    [SerializeField] Transform _spawnPointGameObject;
    [SerializeField] AstroFactory _astroFactory;

    public void SetSpawnPoint(InputAction.CallbackContext context)
    {
        if(!context.started) return;
        if(!_playerData.IsInEditMode) return;
        if(_orbGameObject.activeSelf) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _spawnPointGameObject.position = cursorWorldPosition;
    }
    public void CreateAstro(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (_astroFactory == null || !_playerData.IsInEditMode) return;

        AstroType type = GetAstroTypeFromBinding(context);
        if (type == AstroType.None) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0f;

        _astroFactory.Create(type, cursorWorldPosition);
    }
    /// <summary>Called by the Developer Mode input action (F1). Toggles developer mode and notifies DeveloperToolsUI (and any other consumers) with the new value.</summary>
    public void DeveloperMode(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        _playerData.SetIsInEditMode(!_playerData.IsInEditMode);
    }

    /// <summary>
    /// Obtiene el AstroType según el control que disparó la acción (p. ej. tecla 1=Planet, 2=Asteroid, 3=Sun).
    /// </summary>
    private static AstroType GetAstroTypeFromBinding(InputAction.CallbackContext context)
    {
        string displayName = context.control?.displayName ?? "";
        return displayName switch
        {
            "P" => AstroType.Planet,
            "A" => AstroType.Asteroid,
            "S" => AstroType.Sun,
            _ => AstroType.Planet
        };
    }
}