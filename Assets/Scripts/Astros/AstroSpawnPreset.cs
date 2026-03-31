using System;
using UnityEngine;

/// <summary>
/// Authoring preset for pooled/spawned astros: orbit base parameters and body visuals only (no Transform / velocity).
/// Copied into <see cref="OrbitData"/> / <see cref="BodyData"/> at spawn; runtime edits stay on structs via <see cref="IEditable"/>.
/// </summary>
[CreateAssetMenu(fileName = "AstroSpawnPreset", menuName = "Proto-Pablo/Astro Spawn Preset", order = 0)]
public class AstroSpawnPreset : ScriptableObject
{
    [Serializable]
    public struct AstroPresetRanges
    {
        public float OrbitRadiusMin;
        public float OrbitRadiusMax;
        public float GravityMin;
        public float GravityMax;
        public float TangentialForceMin;
        public float TangentialForceMax;
        public float RadialDampingMin;
        public float RadialDampingMax;
        public float BodyRadiusMin;
        public float BodyRadiusMax;
    }

    [SerializeField] AstroType _astroType;

    [Header("Orbit")]
    public const float OrbitRadiusMin = 1f;
    public const float OrbitRadiusMax = 10f;
    public const float GravityMin = 50f;
    public const float GravityMax = 200f;
    public const float TangentialForceMin = 2f;
    public const float TangentialForceMax = 5f;
    public const float RadialDampingMin = 0.5f;
    public const float RadialDampingMax = 1.5f;

    public const float BodyRadiusMin = 0.5f;
    public const float BodyRadiusMax = 7.5f;

    [SerializeField, Range(OrbitRadiusMin, OrbitRadiusMax)] float _orbitRadius = 3f;
    [SerializeField, Range(GravityMin, GravityMax)] float _gravity = 100f;
    [SerializeField, Range(TangentialForceMin, TangentialForceMax)] float _tangentialForce = 3.5f;
    [SerializeField, Range(RadialDampingMin, RadialDampingMax)] float _radialDamping = 0.85f;

    [Header("Body")]
    [SerializeField, Range(BodyRadiusMin, BodyRadiusMax)] float _bodyRadius = 1.5f;
    [SerializeField] Color _baseColor = new(0.7f, 0.7f, 0.75f);
    [SerializeField] Color _selectedColor = new(0.9f, 0.9f, 1f);

    public AstroPresetRanges Ranges => new AstroPresetRanges
    {
        OrbitRadiusMin = OrbitRadiusMin,
        OrbitRadiusMax = OrbitRadiusMax,
        GravityMin = GravityMin,
        GravityMax = GravityMax,
        TangentialForceMin = TangentialForceMin,
        TangentialForceMax = TangentialForceMax,
        RadialDampingMin = RadialDampingMin,
        RadialDampingMax = RadialDampingMax,
        BodyRadiusMin = BodyRadiusMin,
        BodyRadiusMax = BodyRadiusMax
    };

    public OrbitData ToOrbitData()
    {
        return new OrbitData
        {
            type = _astroType,
            radius = _orbitRadius,
            gravity = _gravity,
            tangentialForce = _tangentialForce,
            radialDamping = _radialDamping,
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
