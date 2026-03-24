using System;
using UnityEngine;

public class Orbit : MonoBehaviour, IOrbitable
{
    [Header("Orbit Settings")]
    [SerializeField] DangerZone _dangerZone;
    private OrbitData _runtimeData;

    [Header("References")]
    [SerializeField] Renderer _orbitRenderer;
    [SerializeField] Transform _transform;
    private OrbitShader _shaderController;
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
    void Start()
    {
        _shaderController.Apply();
    }
    void OnValidate()
    {
        CacheReferences();
        _shaderController.SetData(_dangerZone);
    }
    void Update()
    {
        Debug.DrawRay(_transform.position, Vector3.up, Color.green, 1f);
        Debug.DrawRay(_transform.parent.position, Vector3.right, Color.blue, 1f);
    }
    void LateUpdate()
    {
        _runtimeData.velocity = (_transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = _transform.position;
    }
    void CacheReferences()
    {
        if (!_transform) _transform = transform;       
        if (!_orbitRenderer) _orbitRenderer = _transform.GetComponent<Renderer>();
        if (_shaderController == null) _shaderController = new OrbitShader(_orbitRenderer, _dangerZone);
    }
    public void EnterOrbit()
    {
        _shaderController.SetTetha(0,0);
        _shaderController.SetPhi(0,0);
    }
    public void ExitOrbit()
    {
        _shaderController.SetData(_dangerZone);
    }
    public void SetData(OrbitData data)
    {
        _runtimeData = data;
        _runtimeData.transform = _transform;
        _runtimeData.radialDamping = Mathf.Lerp(15, 1, _runtimeData.gravity/100);
    }

}

[Serializable]
public struct DangerZone
{
    [Range(0, Mathf.PI * 2)]
    public float thetaMin;

    [Range(0, Mathf.PI * 2)]
    public float thetaMax;

    [Range(0, Mathf.PI)]
    public float phiMin;

    [Range(0, Mathf.PI)]
    public float phiMax;
}
