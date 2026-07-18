using UnityEngine;
using TMPro;
public class DeveloperUIController : MonoBehaviour
{
    [SerializeField] LevelData _levelData;
    [SerializeField] TMP_Text _stateText;
    [SerializeField] TMP_Text _versionText;

    void Awake()
    {
        _versionText.text = Application.version;
    }
    void Start()
    {
        OnStateEntered(_levelData.CurrentState);
    }
    void OnEnable()
    {
        _levelData.StateEntered += OnStateEntered;
    }
    void OnDisable()
    {
        _levelData.StateEntered -= OnStateEntered;
    }

    private void OnStateEntered(GameState state)
    {
        _stateText.color = StateToColor(state);
        _stateText.text = $"{StateToString(state)} Mode";
    }
    static string StateToString(GameState state)
    {
        return state switch
        {
            GameState.Edition => "Edit",
            GameState.Precision => "Precision",
            _ => "Contemplative"
        };
    }
    static Color StateToColor(GameState state)
    {
        return state switch
        {
            GameState.Edition => Color.red,
            GameState.Precision => Color.green,
            _ => Color.blue
        };
    }
}
