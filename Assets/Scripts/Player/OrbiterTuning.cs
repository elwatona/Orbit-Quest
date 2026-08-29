/// <summary>
/// Single source for orbiter slider clamps and ranges shared by OrbiterConfig, Orb, and UI.
/// </summary>
public static class OrbiterTuning
{
    public const float ThrustForceMin = 0f;
    public const float ThrustForceMax = 20f;
    public static float ThrustForceEditMin = ThrustForceMin;
    public static float ThrustForceEditMax = ThrustForceMax;

    public const float MinThrustAssistMin = 0f;
    public const float MinThrustAssistMax = 10f;

    public const float ImpulseForceMin = 0f;
    public const float ImpulseForceMax = 30f;
    public static float ImpulseForceEditMin = ImpulseForceMin;
    public static float ImpulseForceEditMax = ImpulseForceMax;

    public const float RechargeDurationMin = 1f;
    public const float RechargeDurationMax = 5f;
    public static float RechargeDurationEditMin = RechargeDurationMin;
    public static float RechargeDurationEditMax = RechargeDurationMax;

    public const float InertiaDampTimeMin = 0.5f;
    public const float InertiaDampTimeMax = 5f;
    public static float InertiaDampTimeEditMin = InertiaDampTimeMin;
    public static float InertiaDampTimeEditMax = InertiaDampTimeMax;

    public const float StabilizerMaxThrustSpeedMin = 1f;
    public const float StabilizerMaxThrustSpeedMax = 25f;
    public static float StabilizerMaxThrustSpeedEditMin = StabilizerMaxThrustSpeedMin;
    public static float StabilizerMaxThrustSpeedEditMax = StabilizerMaxThrustSpeedMax;
}
