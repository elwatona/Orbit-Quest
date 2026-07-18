using UnityEngine;
using UnityEngine.InputSystem;

public class EditorInputController : MonoBehaviour
{
    [SerializeField] LevelData _levelData;
    [SerializeField] PlayerData _playerData;
    [SerializeField] GameObject _orbGameObject;
    [SerializeField] Transform _spawnPointGameObject;
    [SerializeField] AstroManager _astroManager;

    public void SetSpawnPoint(InputAction.CallbackContext context)
    {
        if(!context.started) return;
        if(!_levelData.IsInEditMode) return;
        if(_orbGameObject.activeSelf) return;

        Vector3 cursorWorldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _spawnPointGameObject.position = cursorWorldPosition;
    }
    public void CreateAstro(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (_astroManager == null || !_levelData.IsInEditMode) return;

        AstroType type = GetAstroTypeFromBinding(context);
        if (type == AstroType.None) return;
        
        _astroManager.CreateAstro(type, _playerData.CursorWorld);
    }
    public void DeveloperMode(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        _levelData.SetState(GetGameStateFromBinding(context));
    }

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

    private static GameState GetGameStateFromBinding(InputAction.CallbackContext context)
    {
        string displayName = context.control?.displayName ?? "";
        return displayName switch
        {
            "F1" => GameState.Edition,
            "F2" => GameState.Precision,
            "F3" => GameState.Contemplative,
            _ => GameState.Edition
        };
    }
}