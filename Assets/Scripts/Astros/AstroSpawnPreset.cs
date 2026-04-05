using System;
using UnityEngine;

/// <summary>
/// Authoring preset for pooled/spawned astros: orbit base parameters and body visuals only (no Transform / velocity).
/// Copied into <see cref="OrbitData"/> / <see cref="BodyData"/> at spawn; runtime edits stay on structs via <see cref="IEditable"/>.
/// </summary>
[CreateAssetMenu(fileName = "AstroSpawnPreset", menuName = "Proto-Pablo/Astro Spawn Preset", order = 0)]
public class AstroSpawnPreset : ScriptableObject
{
    [SerializeField] AstroType _astroType;

    [Header("Orbit")]
    [SerializeField, Range(AstroTuning.OrbitRadiusMin, AstroTuning.OrbitRadiusMax)] float _orbitRadius = 3f;
    [SerializeField, Range(AstroTuning.GravityMin, AstroTuning.GravityMax)] float _gravity = 100f;

    [Header("Body")]
    [SerializeField, Range(AstroTuning.BodyRadiusMin, AstroTuning.BodyRadiusMax)] float _bodyRadius = 1.5f;
    [SerializeField] Color _baseColor = new(0.7f, 0.7f, 0.75f);
    [SerializeField] Color _selectedColor = new(0.9f, 0.9f, 1f);

    public OrbitData ToOrbitData()
    {
        return new OrbitData
        {
            type = _astroType,
            radius = _orbitRadius,
            gravity = _gravity,
            velocity = default,
            transform = null
        };
    }

    public BodyData ToBodyData()
    {
        return new BodyData
        {
            radius = _bodyRadius,
            baseColor = _baseColor,
            selectedColor = _selectedColor
        };
    }
}
