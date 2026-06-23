using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class LevelManager : MonoBehaviour
{
    [SerializeField] Limits _limits;
    [SerializeField] GameObject[] _levelBoundsGO;
    [SerializeField] AstroFactory _astroFactory;
    private List<ILevelBounds> _levelBounds = new List<ILevelBounds>();
    private List<Astro> _astros = new List<Astro>();
    void Awake()
    {
        foreach (GameObject levelBoundGO in _levelBoundsGO)
        {
            levelBoundGO.TryGetComponent(out ILevelBounds levelBound);
            if (levelBound != null)
            {
                _levelBounds.Add(levelBound);
            }
        }
        foreach (ILevelBounds levelBound in _levelBounds)
        {
            levelBound.SetLimits(_limits);
        }
        Shader.SetGlobalVector("_Limits_Min", _limits.Min);
        Shader.SetGlobalVector("_Limits_Max", _limits.Max);
    }
    void OnEnable()
    {
        PresetEvents.OnPresetLoaded += HandlePresetLoaded;
        PresetEvents.OnPresetSaved += HandlePresetSaved;
    }
    void OnDisable()
    {
        PresetEvents.OnPresetLoaded -= HandlePresetLoaded;
        PresetEvents.OnPresetSaved -= HandlePresetSaved;
    }
    void HandlePresetLoaded(PresetData presetData)
    {
        ClearAstros();
        LoadAstros(presetData.AstroPresetEntries);
        UpdateAstrosOrbit(presetData.AstroPresetEntries);
    }
    void HandlePresetSaved(string presetName)
    {
        AstroPresetEntry[] astroPresetEntries = new AstroPresetEntry[_astros.Count];
        for (int i = 0; i < _astros.Count; i++)
        {
            AstroPresetEntry entry = _astros[i].ToAstroPresetEntry();
            EditableData editableData = entry.EditableData;
            EditableOrbiterData orbiter = editableData.orbiter;
            orbiter.targetIndices = GetTargetIndices(orbiter.targets, _astros);
            editableData.orbiter = orbiter;
            entry.EditableData = editableData;
            astroPresetEntries[i] = entry;
        }
        PresetData presetData = new PresetData(astroPresetEntries);
        FileManager.SavePreset(presetName, presetData);
    }
    private void ClearAstros()
    {
        foreach (Astro astro in _astros)
        {
            astro.Deactivate();
        }
        _astros.Clear();
    }
    private void LoadAstros(AstroPresetEntry[] astroPresetEntries)
    {
        foreach (AstroPresetEntry astroPresetEntry in astroPresetEntries)
        {
            Astro astro = _astroFactory.Create(astroPresetEntry.EditableData.type, astroPresetEntry.Position);
            astro.UpdateBodyRadius(astroPresetEntry.EditableData.body.radius);
            astro.UpdateBodyRotationSpeed(astroPresetEntry.EditableData.body.rotationSpeed);
            astro.UpdateOrbitRadius(astroPresetEntry.EditableData.orbit.radius);
            astro.UpdateOrbitGravity(astroPresetEntry.EditableData.orbit.gravity);
            if (astro.HasOrbiter())
            {
                astro.UpdateOrbiterSpeed(astroPresetEntry.EditableData.orbiter.speed);
                astro.UpdateOrbiterRadius(astroPresetEntry.EditableData.orbiter.radius);
                astro.UpdateOrbiterEccentricity(astroPresetEntry.EditableData.orbiter.eccentricity);
            }
            _astros.Add(astro);
        }
    }
    private void UpdateAstrosOrbit(AstroPresetEntry[] astroPresetEntries)
    {
        for (int i = 0; i < astroPresetEntries.Length; i++)
        {
            if (!_astros[i].HasOrbiter()) continue;
            IEditable[] targets = GetTargetsFromIndexes(astroPresetEntries[i].EditableData.orbiter.targetIndices, _astros);
            _astros[i].UpdateOrbiterTargets(targets);
        }
    }
    static int[] GetTargetIndices(IEditable[] targets, List<Astro> astros)
    {
        if (targets == null || targets.Length == 0)
            return System.Array.Empty<int>();

        int[] indexes = new int[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            indexes[i] = astros.FindIndex(astro => (IEditable)astro == targets[i]);
        }
        return indexes;
    }
    static IEditable[] GetTargetsFromIndexes(int[] targetIndexes, List<Astro> astros)
    {
        if (targetIndexes == null || targetIndexes.Length == 0)
            return System.Array.Empty<IEditable>();

        IEditable[] targets = new IEditable[targetIndexes.Length];
        for (int i = 0; i < targetIndexes.Length; i++)
        {
            int index = targetIndexes[i];
            targets[i] = index >= 0 && index < astros.Count ? astros[index] : null;
        }
        return targets;
    }
    public void CreateAstro(AstroType type, Vector3 position)
    {
        Astro astro = _astroFactory.Create(type, position);
        _astros.Add(astro);
    }
}