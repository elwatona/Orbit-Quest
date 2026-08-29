using System;

[Serializable]
public struct EditorRangesData
{
    public float GravityMin;
    public float GravityMax;
    public float OrbitRadiusMin;
    public float OrbitRadiusMax;
    public float BodyRadiusMin;
    public float BodyRadiusMax;
    public float RotationSpeedMin;
    public float RotationSpeedMax;
    public float OrbiterSpeedMin;
    public float OrbiterSpeedMax;
    public float OrbiterRadiusMin;
    public float OrbiterRadiusMax;
    public float OrbiterEccentricityMin;
    public float OrbiterEccentricityMax;
    public float ThrustForceMin;
    public float ThrustForceMax;
    public float ImpulseForceMin;
    public float ImpulseForceMax;
    public float RechargeDurationMin;
    public float RechargeDurationMax;
    public float InertiaDampTimeMin;
    public float InertiaDampTimeMax;
    public float StabilizerMaxThrustSpeedMin;
    public float StabilizerMaxThrustSpeedMax;

    public static EditorRangesData Defaults => new EditorRangesData
    {
        GravityMin = AstroTuning.GravityMin,
        GravityMax = AstroTuning.GravityMax,
        OrbitRadiusMin = AstroTuning.OrbitRadiusMin,
        OrbitRadiusMax = AstroTuning.OrbitRadiusMax,
        BodyRadiusMin = AstroTuning.BodyRadiusMin,
        BodyRadiusMax = AstroTuning.BodyRadiusMax,
        RotationSpeedMin = AstroTuning.RotationSpeedMin,
        RotationSpeedMax = AstroTuning.RotationSpeedMax,
        OrbiterSpeedMin = AstroTuning.OrbiterSpeedMin,
        OrbiterSpeedMax = AstroTuning.OrbiterSpeedMax,
        OrbiterRadiusMin = AstroTuning.OrbiterRadiusMin,
        OrbiterRadiusMax = AstroTuning.OrbiterRadiusMax,
        OrbiterEccentricityMin = AstroTuning.OrbiterEccentricityMin,
        OrbiterEccentricityMax = AstroTuning.OrbiterEccentricityMax,
        ThrustForceMin = OrbiterTuning.ThrustForceMin,
        ThrustForceMax = OrbiterTuning.ThrustForceMax,
        ImpulseForceMin = OrbiterTuning.ImpulseForceMin,
        ImpulseForceMax = OrbiterTuning.ImpulseForceMax,
        RechargeDurationMin = OrbiterTuning.RechargeDurationMin,
        RechargeDurationMax = OrbiterTuning.RechargeDurationMax,
        InertiaDampTimeMin = OrbiterTuning.InertiaDampTimeMin,
        InertiaDampTimeMax = OrbiterTuning.InertiaDampTimeMax,
        StabilizerMaxThrustSpeedMin = OrbiterTuning.StabilizerMaxThrustSpeedMin,
        StabilizerMaxThrustSpeedMax = OrbiterTuning.StabilizerMaxThrustSpeedMax
    };
}

[Serializable]
public struct GameSettingsData
{
    public float GridThickness;
    public float SkyboxColdness;
    public float SkyboxDensity;
    public bool SkyboxEnabled;
    public float MasterVolume;
    public EditorRangesData Editor;

    public static GameSettingsData Defaults => new GameSettingsData
    {
        GridThickness = GraphicsTuning.ThicknessDefault,
        SkyboxColdness = GraphicsTuning.ColdnessDefault,
        SkyboxDensity = GraphicsTuning.DensityDefault,
        SkyboxEnabled = GraphicsTuning.SkyboxEnabledDefault,
        MasterVolume = AudioTuning.MasterVolumeDefault,
        Editor = EditorRangesData.Defaults
    };
}
