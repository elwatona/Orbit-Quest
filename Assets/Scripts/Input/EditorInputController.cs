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

        Vector3 cursorWorldPosition = _playerData.CursorWorld;
        cursorWorldPosition.y = 0f;
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
        return GetBindingIndex(context) switch
        {
            0 => AstroType.Planet,
            1 => AstroType.Asteroid,
            2 => AstroType.Sun,
            _ => AstroType.Planet
        };
    }

    private static GameState GetGameStateFromBinding(InputAction.CallbackContext context)
    {
        return GetBindingIndex(context) switch
        {
            0 => GameState.Edition,
            1 => GameState.Precision,
            2 => GameState.Contemplative,
            _ => GameState.Edition
        };
    }

    private static int GetBindingIndex(InputAction.CallbackContext context)
    {
        if (context.action == null || context.control == null) return -1;
        return context.action.GetBindingIndexForControl(context.control);
    }
}