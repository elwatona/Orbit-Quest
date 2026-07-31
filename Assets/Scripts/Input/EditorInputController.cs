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
        if (!context.performed) return;
        if (!_levelData.IsInEditMode) return;

        Vector3 cursorWorldPosition = _playerData.CursorWorld;
        cursorWorldPosition.y = 0f;
        _spawnPointGameObject.position = cursorWorldPosition;
    }

    public void SpawnPlanet(InputAction.CallbackContext context)
        => SpawnAstro(context, AstroType.Planet);

    public void SpawnAsteroid(InputAction.CallbackContext context)
        => SpawnAstro(context, AstroType.Asteroid);

    public void SpawnSun(InputAction.CallbackContext context)
        => SpawnAstro(context, AstroType.Sun);

    public void EnterEdition(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        _levelData.SetState(GameState.Edition);
    }

    public void TogglePlayMode(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        GameState current = _levelData.CurrentState;
        if (current == GameState.Edition)
        {
            _levelData.SetState(_levelData.LastPlayMode);
            return;
        }

        if (current == GameState.Precision)
            _levelData.SetState(GameState.Contemplative);
        else if (current == GameState.Contemplative)
            _levelData.SetState(GameState.Precision);
    }

    void SpawnAstro(InputAction.CallbackContext context, AstroType type)
    {
        if (!context.performed) return;
        if (_astroManager == null || !_levelData.IsInEditMode) return;
        if (type == AstroType.None) return;

        _astroManager.CreateAstro(type, _playerData.CursorWorld);
    }
}
