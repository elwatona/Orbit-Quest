using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LogComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _message;
    [SerializeField] TextMeshProUGUI _stackTrace;
    [SerializeField] Image _background;
    public void Log(string message, string stackTrace, LogType type)
    {
        switch (type)
        {
            case LogType.Assert:
            case LogType.Error:
                _background.color = Color.red;
                break;
            case LogType.Exception:
            case LogType.Warning:
                _background.color = Color.yellow;
                break;
            default:
                _background.color = Color.white;
                break;
        }
        _message.text = message;
        _stackTrace.text = stackTrace;
    }
}