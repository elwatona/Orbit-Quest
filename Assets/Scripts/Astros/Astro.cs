using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Astro : MonoBehaviour, IPointerDownHandler, IEditable, IDragHandler
{
    public static event Action<IEditable> OnEditableClicked;
    public event Action OnEditableDragged;

    [Header("Settings")]
    [SerializeField] PlayerData _playerData;
    [SerializeField] OrbitData _orbitData;
    [SerializeField] BodyData _bodyData;
    [SerializeField] float _rotationSpeed = 5f;
    [SerializeField] Transform _transform, _baseTransform, _orbitTransform;

    private AstroSpawnPreset _spawnSource;
    private IOrbitable _orbit;
    private BodyShader _baseShader;
    private TransformOrbiter _orbiter;
    private bool _isSelected;

    public float BodyRadius => _bodyData.radius;
    public bool HasOrbiter() => _orbiter != null;

    public EditableData Data => new EditableData
    {
        type = _orbitData.type,
        body = new EditableBodyData
        {
            radius = _bodyData.radius,
            rotationSpeed = _rotationSpeed
        },
        orbit = new EditableOrbitData
        {
            radius = _orbitData.radius,
            gravity = _orbitData.gravity
        },
        orbiter = new EditableOrbiterData
        {
            speed = _orbiter?.Speed ?? 0f,
            radius = _orbiter?.Radius ?? 0f,
            eccentricity = _orbiter?.Eccentricity ?? 0f,
            targets = _orbiter?.TargetsEditable?? new IEditable[0]
        }
    };


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
        if(!_playerData.IsInEditMode || eventData.button != PointerEventData.InputButton.Left) return;

        Vector2 desiredPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        _transform.position = desiredPosition;

        OnEditableDragged?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;

        OnEditableClicked?.Invoke(this);
    }

    public void UpdateBodyRadius(float radius)
    {
        _bodyData.radius = radius;
        UpdateBaseValues();
    }

    public void UpdateBodyRotationSpeed(float rotationSpeed)
    {
        _rotationSpeed = rotationSpeed;
    }

    public void UpdateOrbitRadius(float radius)
    {
        _orbitData.radius = radius;
        UpdateOrbitValues();
    }

    public void UpdateOrbitGravity(float gravity)
    {
        _orbitData.gravity = gravity;
        UpdateOrbitValues();
    }

    public void UpdateOrbiterSpeed(float speed)
    {
        _orbiter.SetSpeed(speed);
    }

    public void UpdateOrbiterRadius(float radius)
    {
        _orbiter.SetRadius(radius);
    }

    public void UpdateOrbiterEccentricity(float eccentricity)
    {
        _orbiter.SetEccentricity(eccentricity);
    }

    public void UpdateOrbiterTargets(IEditable[] targets)
    {
        _orbiter.SetTargets(targets);
    }
}
[Serializable]
public struct BodyData
{
    [Range(0.5f, 7.5f)] public float radius;
    public Color baseColor;
    public Color selectedColor;
}
