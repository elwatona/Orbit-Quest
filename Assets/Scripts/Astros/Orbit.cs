using System;
using UnityEngine;

public class Orbit : MonoBehaviour, IOrbitable
{
    [Header("Orbit Settings")]
    [SerializeField] OrbitRenderer.Data _orbitRenderer;
    private OrbitData _runtimeData;

    [Header("References")]
    [SerializeField] Transform _transform;
    private OrbitRenderer _shaderController;
    private Vector3 _lastPosition;

    public OrbitData Data => _runtimeData;

    void Awake()
    {
        CacheReferences();
    }
    void OnEnable()
    {
        _lastPosition = _transform.position;
    }
    void OnValidate()
    {
        CacheReferences();
    }
    void Update()
    {
        Debug.DrawRay(_transform.position, Vector3.up, Color.green, 1f);
        Debug.DrawRay(_transform.parent.position, Vector3.right, Color.blue, 1f);
        _shaderController.Update();
    }
    void LateUpdate()
    {
        _runtimeData.velocity = (_transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = _transform.position;
    }
    void CacheReferences()
    {
        if (!_transform) _transform = transform;
        _shaderController = new OrbitRenderer(_orbitRenderer, _transform, UnityEngine.Camera.main.transform);
    }
    public void EnterOrbit()
    {

    }
    public void ExitOrbit()
    {
        
    }
    public void SetData(OrbitData data)
    {
        _runtimeData = data;
        _runtimeData.transform = _transform;
    }

}
