using UnityEngine;

public class AstroFactory
{
    readonly GameObject _sunPrefab;
    readonly GameObject _planetPrefab;
    readonly GameObject _asteroidPrefab;
    readonly int _initialPoolSize;
    readonly Transform _poolParent;
    readonly AstroSpawnPreset _sunSpawnPreset;
    readonly AstroSpawnPreset _planetSpawnPreset;
    readonly AstroSpawnPreset _asteroidSpawnPreset;
    readonly GameObjectPool _sunPool;
    readonly GameObjectPool _planetPool;
    readonly GameObjectPool _asteroidPool;
    public AstroFactory(AstroFactoryDependencies dependencies)
    {
        _sunPrefab = dependencies.SunPrefab;
        _planetPrefab = dependencies.PlanetPrefab;
        _asteroidPrefab = dependencies.AsteroidPrefab;
        _initialPoolSize = dependencies.InitialPoolSize;
        _poolParent = dependencies.PoolParent;
        _sunSpawnPreset = dependencies.SunSpawnPreset;
        _planetSpawnPreset = dependencies.PlanetSpawnPreset;
        _asteroidSpawnPreset = dependencies.AsteroidSpawnPreset;

        if (_sunPrefab != null && _sunPrefab.GetComponent<Astro>() == null)
            Debug.LogWarning("AstroFactory: Sun prefab does not have an Astro component.");
        if (_planetPrefab != null && _planetPrefab.GetComponent<Astro>() == null)
            Debug.LogWarning("AstroFactory: Planet prefab does not have an Astro component.");
        if (_asteroidPrefab != null && _asteroidPrefab.GetComponent<Astro>() == null)
            Debug.LogWarning("AstroFactory: Asteroid prefab does not have an Astro component.");
    
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
            Debug.LogError($"AstroFactory: no prefab or pool for type {type}.");
            return null;
        }

        GameObject go = pool.Get();
        if (parent != null)
            go.transform.SetParent(parent);

        go.transform.position = position;

        Astro astro = go.GetComponent<Astro>();
        if (astro == null)
        {
            Debug.LogError($"AstroFactory: prefab for {type} has no Astro component.");
            return null;
        }

        AstroSpawnPreset preset = GetSpawnPreset(type);
        if ((orbitData == null || bodyData == null) && preset == null)
        {
            Debug.LogError($"AstroFactory: no spawn preset assigned for type {type}.");
            return null;
        }

        OrbitData o = orbitData ?? preset.ToOrbitData();
        BodyData b = bodyData ?? preset.ToBodyData();
        AstroSpawnPreset spawnSource = (orbitData == null || bodyData == null) ? preset : null;
        astro.Initialize(o, b, spawnSource);

        return astro;
    }
    public void Release(Astro astro)
    {
        GameObjectPool pool = GetPool(astro.Data.type);
        if (pool == null)
        {
            Debug.LogError($"AstroFactory: no pool for type {astro.Data.type}.");
            return;
        }
        pool.Release(astro.gameObject);
    }
}
[System.Serializable]
public class AstroFactoryDependencies
{
    [Header("Prefabs (one per astro type)")]
    public GameObject SunPrefab;
    public GameObject PlanetPrefab;
    public GameObject AsteroidPrefab;

    [Header("Spawn presets (orbit + body); used when Create omits data")]
    public AstroSpawnPreset SunSpawnPreset;
    public AstroSpawnPreset PlanetSpawnPreset;
    public AstroSpawnPreset AsteroidSpawnPreset;

    [Header("Pool Settings")]
    public int InitialPoolSize = 8;
    public Transform PoolParent;
}
