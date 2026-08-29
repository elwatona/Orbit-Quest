using System.Collections.Generic;

public class EditionSettings
{
    public RangeParameter Gravity { get; }
    public RangeParameter OrbitRadius { get; }
    public RangeParameter BodyRadius { get; }
    public RangeParameter RotationSpeed { get; }
    public RangeParameter OrbiterSpeed { get; }
    public RangeParameter OrbiterRadius { get; }
    public RangeParameter OrbiterEccentricity { get; }
    public RangeParameter ThrustForce { get; }
    public RangeParameter ImpulseForce { get; }
    public RangeParameter RechargeDuration { get; }
    public RangeParameter InertiaDampTime { get; }
    public RangeParameter StabilizerMaxThrustSpeed { get; }

    public EditionSettings()
    {
        Gravity = new RangeParameter("gravity", "Gravity", AstroTuning.GravityMin, AstroTuning.GravityMax);
        OrbitRadius = new RangeParameter("orbitRadius", "Orbit Radius", AstroTuning.OrbitRadiusMin, AstroTuning.OrbitRadiusMax);
        BodyRadius = new RangeParameter("bodyRadius", "Body Radius", AstroTuning.BodyRadiusMin, AstroTuning.BodyRadiusMax);
        RotationSpeed = new RangeParameter("rotationSpeed", "Rotation Speed", AstroTuning.RotationSpeedMin, AstroTuning.RotationSpeedMax);
        OrbiterSpeed = new RangeParameter("orbiterSpeed", "Orbiter Speed", AstroTuning.OrbiterSpeedMin, AstroTuning.OrbiterSpeedMax);
        OrbiterRadius = new RangeParameter("orbiterRadius", "Orbiter Radius", AstroTuning.OrbiterRadiusMin, AstroTuning.OrbiterRadiusMax);
        OrbiterEccentricity = new RangeParameter("orbiterEccentricity", "Eccentricity", AstroTuning.OrbiterEccentricityMin, AstroTuning.OrbiterEccentricityMax);
        ThrustForce = new RangeParameter("thrustForce", "Thrust Force", OrbiterTuning.ThrustForceMin, OrbiterTuning.ThrustForceMax);
        ImpulseForce = new RangeParameter("impulseForce", "Impulse Force", OrbiterTuning.ImpulseForceMin, OrbiterTuning.ImpulseForceMax);
        RechargeDuration = new RangeParameter("rechargeDuration", "Impulse Cooldown", OrbiterTuning.RechargeDurationMin, OrbiterTuning.RechargeDurationMax);
        InertiaDampTime = new RangeParameter("inertiaDampTime", "Inertia Damp Time", OrbiterTuning.InertiaDampTimeMin, OrbiterTuning.InertiaDampTimeMax);
        StabilizerMaxThrustSpeed = new RangeParameter(
            "stabilizerMaxThrustSpeed",
            "Max Thrust Speed",
            OrbiterTuning.StabilizerMaxThrustSpeedMin,
            OrbiterTuning.StabilizerMaxThrustSpeedMax);
    }

    public IReadOnlyList<RangeParameter> AstroParameters => new[]
    {
        Gravity,
        OrbitRadius,
        BodyRadius,
        RotationSpeed,
        OrbiterSpeed,
        OrbiterRadius,
        OrbiterEccentricity
    };

    public IReadOnlyList<RangeParameter> PlayerParameters => new[]
    {
        ThrustForce,
        ImpulseForce,
        RechargeDuration,
        InertiaDampTime,
        StabilizerMaxThrustSpeed
    };

    public IReadOnlyList<RangeParameter> Parameters
    {
        get
        {
            var list = new List<RangeParameter>(AstroParameters.Count + PlayerParameters.Count);
            list.AddRange(AstroParameters);
            list.AddRange(PlayerParameters);
            return list;
        }
    }

    public void ApplyData(EditorRangesData data)
    {
        Gravity.SetRange(data.GravityMin, data.GravityMax, notify: false);
        OrbitRadius.SetRange(data.OrbitRadiusMin, data.OrbitRadiusMax, notify: false);
        BodyRadius.SetRange(data.BodyRadiusMin, data.BodyRadiusMax, notify: false);
        RotationSpeed.SetRange(data.RotationSpeedMin, data.RotationSpeedMax, notify: false);
        OrbiterSpeed.SetRange(data.OrbiterSpeedMin, data.OrbiterSpeedMax, notify: false);
        OrbiterRadius.SetRange(data.OrbiterRadiusMin, data.OrbiterRadiusMax, notify: false);
        OrbiterEccentricity.SetRange(data.OrbiterEccentricityMin, data.OrbiterEccentricityMax, notify: false);
        ThrustForce.SetRange(data.ThrustForceMin, data.ThrustForceMax, notify: false);
        ImpulseForce.SetRange(data.ImpulseForceMin, data.ImpulseForceMax, notify: false);
        RechargeDuration.SetRange(data.RechargeDurationMin, data.RechargeDurationMax, notify: false);
        InertiaDampTime.SetRange(data.InertiaDampTimeMin, data.InertiaDampTimeMax, notify: false);
        StabilizerMaxThrustSpeed.SetRange(data.StabilizerMaxThrustSpeedMin, data.StabilizerMaxThrustSpeedMax, notify: false);
        ApplyToTuning();
    }

    public EditorRangesData ToData()
    {
        return new EditorRangesData
        {
            GravityMin = Gravity.Min,
            GravityMax = Gravity.Max,
            OrbitRadiusMin = OrbitRadius.Min,
            OrbitRadiusMax = OrbitRadius.Max,
            BodyRadiusMin = BodyRadius.Min,
            BodyRadiusMax = BodyRadius.Max,
            RotationSpeedMin = RotationSpeed.Min,
            RotationSpeedMax = RotationSpeed.Max,
            OrbiterSpeedMin = OrbiterSpeed.Min,
            OrbiterSpeedMax = OrbiterSpeed.Max,
            OrbiterRadiusMin = OrbiterRadius.Min,
            OrbiterRadiusMax = OrbiterRadius.Max,
            OrbiterEccentricityMin = OrbiterEccentricity.Min,
            OrbiterEccentricityMax = OrbiterEccentricity.Max,
            ThrustForceMin = ThrustForce.Min,
            ThrustForceMax = ThrustForce.Max,
            ImpulseForceMin = ImpulseForce.Min,
            ImpulseForceMax = ImpulseForce.Max,
            RechargeDurationMin = RechargeDuration.Min,
            RechargeDurationMax = RechargeDuration.Max,
            InertiaDampTimeMin = InertiaDampTime.Min,
            InertiaDampTimeMax = InertiaDampTime.Max,
            StabilizerMaxThrustSpeedMin = StabilizerMaxThrustSpeed.Min,
            StabilizerMaxThrustSpeedMax = StabilizerMaxThrustSpeed.Max
        };
    }

    public void ApplyToTuning()
    {
        AstroTuning.GravityEditMin = Gravity.Min;
        AstroTuning.GravityEditMax = Gravity.Max;
        AstroTuning.OrbitRadiusEditMin = OrbitRadius.Min;
        AstroTuning.OrbitRadiusEditMax = OrbitRadius.Max;
        AstroTuning.BodyRadiusEditMin = BodyRadius.Min;
        AstroTuning.BodyRadiusEditMax = BodyRadius.Max;
        AstroTuning.RotationSpeedEditMin = RotationSpeed.Min;
        AstroTuning.RotationSpeedEditMax = RotationSpeed.Max;
        AstroTuning.OrbiterSpeedEditMin = OrbiterSpeed.Min;
        AstroTuning.OrbiterSpeedEditMax = OrbiterSpeed.Max;
        AstroTuning.OrbiterRadiusEditMin = OrbiterRadius.Min;
        AstroTuning.OrbiterRadiusEditMax = OrbiterRadius.Max;
        AstroTuning.OrbiterEccentricityEditMin = OrbiterEccentricity.Min;
        AstroTuning.OrbiterEccentricityEditMax = OrbiterEccentricity.Max;

        OrbiterTuning.ThrustForceEditMin = ThrustForce.Min;
        OrbiterTuning.ThrustForceEditMax = ThrustForce.Max;
        OrbiterTuning.ImpulseForceEditMin = ImpulseForce.Min;
        OrbiterTuning.ImpulseForceEditMax = ImpulseForce.Max;
        OrbiterTuning.RechargeDurationEditMin = RechargeDuration.Min;
        OrbiterTuning.RechargeDurationEditMax = RechargeDuration.Max;
        OrbiterTuning.InertiaDampTimeEditMin = InertiaDampTime.Min;
        OrbiterTuning.InertiaDampTimeEditMax = InertiaDampTime.Max;
        OrbiterTuning.StabilizerMaxThrustSpeedEditMin = StabilizerMaxThrustSpeed.Min;
        OrbiterTuning.StabilizerMaxThrustSpeedEditMax = StabilizerMaxThrustSpeed.Max;
    }
}
