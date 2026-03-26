/// <summary>
/// Single source for orbiter slider clamps and ranges shared by OrbiterConfig, Orb, and UI.
/// </summary>
public static class OrbiterTuning
{
    public const float ThrustForceMin = 0f;
    public const float ThrustForceMax = 20f;

    public const float MinThrustAssistMin = 0f;
    public const float MinThrustAssistMax = 10f;

    public const float ImpulseForceMin = 0f;
    public const float ImpulseForceMax = 30f;

    public const float InertiaDampTimeMin = 0.5f;
    public const float InertiaDampTimeMax = 5f;

    public const float StabilizerMaxThrustSpeedMin = 1f;
    public const float StabilizerMaxThrustSpeedMax = 25f;
}
