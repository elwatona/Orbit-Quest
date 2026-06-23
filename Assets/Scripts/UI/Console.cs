using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Console : MonoBehaviour
{
    private GameObjectPool _logPool;
    [SerializeField] GameObject _logPrefab;
    [SerializeField] Transform _logContainer;
    void Awake()
    {
        _logPool = new GameObjectPool(_logPrefab, 10, _logContainer);
    }
    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }
    private void Log(string message, string stackTrace, LogType type)
    {
        GameObject logGO = _logPool.Get();
        LogComponent log = logGO.GetComponent<LogComponent>();
        log.Log(message, stackTrace, type);
    }
}
