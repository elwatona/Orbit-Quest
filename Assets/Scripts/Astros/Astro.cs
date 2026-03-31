using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Astro : MonoBehaviour, IPointerDownHandler, IEditable, IDragHandler
{
    public const float BodyRadiusMin = 0.5f;
    public const float BodyRadiusMax = 7.5f;
    public const float OrbitRadiusMin = 1f;
    public const float OrbitRadiusMax = 10f;
    public const float GravityMin = 50f;
    public const float GravityMax = 200f;
    public const float TangentialForceMin = 2f;
    public const float TangentialForceMax = 5f;
    public const float RadialDampingMin = 0.5f;
    public const float RadialDampingMax = 1.5f;
    public const float RotationSpeedMin = 0f;
    public const float RotationSpeedMax = 20f;

    public static event Action<IEditable> OnAstroClicked;

    [Header("Settings")]
    [SerializeField] OrbitData _orbitData;
    [SerializeField] BodyData _bodyData;
    [SerializeField] float _rotationSpeed = 5f;

    [SerializeField] Transform _transform, _baseTransform, _orbitTransform;
    [FormerlySerializedAs("_uiManager")]
    // [SerializeField] DeveloperToolsUI _developerTools;
    private AstroSpawnPreset _spawnSource;
    private IOrbitable _orbit;
    private BodyShader _baseShader;
    private TransformOrbiter _orbiter;
    private bool _isSelected;

    public string DisplayName => _orbitData.type.ToString();
    public float BodyRadius => _bodyData.radius;
    public float OrbitRadius => _orbitData.radius;
    public float Gravity => _orbitData.gravity;
    public float TangentialForce => _orbitData.tangentialForce;
    public float RadialDamping => _orbitData.radialDamping;
    public float RotationSpeed => _rotationSpeed;

    public AstroSpawnPreset SpawnSource => _spawnSource;

    void Awake()
    {
        CacheReferences();
    }
    void OnEnable()
    {
        Apply();
    }
    void OnValidate()
    {
        CacheReferences();
        UpdateBaseValues();
        UpdateOrbitValues();
    }
    void Update()
    {
        _transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
    }
    void CacheReferences()
    {
        if(!_transform) _transform = transform;
        if(!_baseTransform) _baseTransform = _transform.Find("Base");
        if(!_orbitTransform) _orbitTransform = _transform.Find("Orbit");
        if(_orbit == null) _orbit = _orbitTransform?.GetComponent<IOrbitable>();
        if(_baseShader == null) _baseShader = new BodyShader(_baseTransform?.GetComponent<Renderer>());
        if(_orbiter == null) _orbiter = GetComponent<TransformOrbiter>();
        // if (_developerTools == null) _developerTools = FindFirstObjectByType<DeveloperToolsUI>();
    }
    void UpdateBaseValues()
    {
        float diameter = _bodyData.radius * 2f;
        Color desiredColor = _isSelected ? _bodyData.selectedColor : _bodyData.baseColor;

        _baseTransform.localScale = Vector3.one * diameter;
        _baseShader?.SetColor(desiredColor);
    }
    void UpdateOrbitValues()
    {
        float diameter = _orbitData.radius * 2f;

        _orbitTransform.localScale = Vector3.one * diameter;
        _orbit?.SetData(_orbitData);
    }
    void Apply()
    {
        UpdateBaseValues();
        UpdateOrbitValues();
    }

    public void SetBodyRadius(float value)
    {
        _bodyData.radius = Mathf.Clamp(value, BodyRadiusMin, BodyRadiusMax);
        UpdateBaseValues();
    }

    public void SetOrbitRadius(float value)
    {
        _orbitData.radius = Mathf.Clamp(value, OrbitRadiusMin, OrbitRadiusMax);
        UpdateOrbitValues();
    }

    public void SetGravity(float value)
    {
        _orbitData.gravity = Mathf.Clamp(value, GravityMin, GravityMax);
        UpdateOrbitValues();
    }

    public void SetTangentialForce(float value)
    {
        _orbitData.tangentialForce = Mathf.Clamp(value, TangentialForceMin, TangentialForceMax);
        UpdateOrbitValues();
    }

    public void SetRadialDamping(float value)
    {
        _orbitData.radialDamping = Mathf.Clamp(value, RadialDampingMin, RadialDampingMax);
        UpdateOrbitValues();
    }

    public void SetRotationSpeed(float value)
    {
        _rotationSpeed = Mathf.Clamp(value, RotationSpeedMin, RotationSpeedMax);
    }

    public void Selected()
    {
        _isSelected = true;
        Apply();
    }

    public void Deselected()
    {
        _isSelected = false;
        Apply();
    }

    /// <summary>
    /// Sets orbit and body data (e.g. when spawning from pool) and refreshes visuals. Type is defined by the prefab, not here.
    /// </summary>
    public void Initialize(OrbitData orbitData, BodyData bodyData, AstroSpawnPreset spawnSource = null)
    {
        _orbitData = orbitData;
        _bodyData = bodyData;
        _spawnSource = spawnSource;
        Apply();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // if (_developerTools == null) _developerTools = FindFirstObjectByType<DeveloperToolsUI>();
        // if (_developerTools == null || !_developerTools.IsAvailable || !_developerTools.IsDeveloperModeActive) return;

        Vector2 desiredPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        _transform.position = desiredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;

        OnAstroClicked?.Invoke(this);
    }
}
[Serializable]
public struct BodyData
{
    [Range(0.5f, 7.5f)] public float radius;
    public Color baseColor;
    public Color selectedColor;
}
