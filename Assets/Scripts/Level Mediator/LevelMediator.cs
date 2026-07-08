using UnityEngine;

public class LevelMediator : MonoBehaviour
{
    private LevelSignals _levelSignals;

    private void Awake()
    {
        _levelSignals = new LevelSignals();
    }
}