using UnityEngine;
using System.Collections.Generic;
public class LevelMediator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] LevelData _levelData;
    [SerializeField] Limits _limits;
    [Header("Dependencies")]
    [SerializeField] AudioDependencies _audioDependencies;
    [SerializeField] CameraManagerDependencies _cameraDependencies;
    [Header("Level Bounds")]
    [SerializeField] GameObject[] _levelBoundsGO;
    private CameraManager _cameraManager;
    private AudioManager _audioManager;
    private List<ILimitable> _levelBounds = new List<ILimitable>();
    void Awake()
    {
        _levelData.Initialize();
        _cameraManager = new CameraManager(_cameraDependencies);
        _audioManager = new AudioManager(_audioDependencies);
        UpdateLimitables();
    }
    void OnEnable()
    {
        _cameraManager.Subscribe();
    }
    void OnDisable()
    {
        _cameraManager.Unsubscribe();
    }
    void Start() => _levelData.Start();
    void Update()
    {
        _cameraManager.Update();
    }

    private void UpdateLimitables()
    {
        foreach (GameObject levelBoundGO in _levelBoundsGO)
        {
            levelBoundGO.TryGetComponent(out ILimitable levelBound);
            if (levelBound != null)
            {
                _levelBounds.Add(levelBound);
            }
        }
        foreach (ILimitable levelBound in _levelBounds)
        {
            levelBound.SetLimits(_limits);
        }
        Shader.SetGlobalVector("_Limits_Min", _limits.Min);
        Shader.SetGlobalVector("_Limits_Max", _limits.Max);
    }
}
