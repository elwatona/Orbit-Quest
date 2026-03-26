using UnityEngine;

/// <summary>
/// Creates Astro instances from a pooled prefab per type (Sun, Planet, Asteroid) and initializes them with default or provided OrbitData/BodyData.
/// </summary>
public class AstroFactory : MonoBehaviour, IAstroFactory
{
    [Header("Prefabs (one per astro type)")]
    [SerializeField] GameObject _sunPrefab;
    [SerializeField] GameObject _planetPrefab;
    [SerializeField] GameObject _asteroidPrefab;

    [Header("Spawn presets (orbit + body); used when Create omits data")]
    [SerializeField] AstroSpawnPreset _sunSpawnPreset;
    [SerializeField] AstroSpawnPreset _planetSpawnPreset;
    [SerializeField] AstroSpawnPreset _asteroidSpawnPreset;

    [Header("Pool Settings")]
    [SerializeField] int _initialPoolSize = 8;
    [SerializeField] Transform _poolParent;

    private GameObjectPool _sunPool;
    private GameObjectPool _planetPool;
    private GameObjectPool _asteroidPool;

    void Awake()
    {
        if (_sunPrefab != null && _sunPrefab.GetComponent<Astro>() == null)
            Debug.LogWarning("AstroFactory: Sun prefab does not have an Astro component.", this);
        if (_planetPrefab != null && _planetPrefab.GetComponent<Astro>() == null)
            Debug.LogWarning("AstroFactory: Planet prefab does not have an Astro component.", this);
        if (_asteroidPrefab != null && _asteroidPrefab.GetComponent<Astro>() == null)
            Debug.LogWarning("AstroFactory: Asteroid prefab does not have an Astro component.", this);

        _sunPool = _sunPrefab != null ? new GameObjectPool(_sunPrefab, _initialPoolSize, _poolParent) : null;
        _planetPool = _planetPrefab != null ? new GameObjectPool(_planetPrefab, _initialPoolSize, _poolParent) : null;
        _asteroidPool = _asteroidPrefab != null ? new GameObjectPool(_asteroidPrefab, _initialPoolSize, _poolParent) : null;
    }

    private GameObjectPool GetPool(AstroType type)
    {
        switch (type)
        {
            case AstroType.Sun: return _sunPool;
            case AstroType.Planet: return _planetPool;
            case AstroType.Asteroid: return _asteroidPool;
            default: return null;
        }
    }

    private AstroSpawnPreset GetSpawnPreset(AstroType type)
    {
        switch (type)
        {
            case AstroType.Sun: return _sunSpawnPreset;
            case AstroType.Planet: return _planetSpawnPreset;
            case AstroType.Asteroid: return _asteroidSpawnPreset;
            default: return null;
        }
    }

    public Astro Create(AstroType type, Vector3 position, Transform parent = null)
    {
        return Create(type, position, null, null, parent);
    }

    public Astro Create(AstroType type, Vector3 position, OrbitData? orbitData, BodyData? bodyData, Transform parent = null)
    {
        GameObjectPool pool = GetPool(type);
        if (pool == null)
        {
            Debug.LogError($"AstroFactory: no prefab or pool for type {type}.", this);
            return null;
        }

        GameObject go = pool.Get();
        if (parent != null)
            go.transform.SetParent(parent);

        go.transform.position = position;

        Astro astro = go.GetComponent<Astro>();
        if (astro == null)
        {
            Debug.LogError($"AstroFactory: prefab for {type} has no Astro component.", this);
            return null;
        }

        AstroSpawnPreset preset = GetSpawnPreset(type);
        if ((orbitData == null || bodyData == null) && preset == null)
        {
            Debug.LogError($"AstroFactory: no spawn preset assigned for type {type}.", this);
            return null;
        }

        OrbitData o = orbitData ?? preset.ToOrbitData();
        BodyData b = bodyData ?? preset.ToBodyData();
        astro.Initialize(o, b);

        return astro;
    }
}
