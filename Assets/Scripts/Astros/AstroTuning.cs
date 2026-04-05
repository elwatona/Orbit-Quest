public static class AstroTuning
{
    public const float OrbitRadiusMin = 1f;
    public const float OrbitRadiusMax = 10f;

    public const float GravityMin = 50f;
    public const float GravityMax = 200f;

    public const float BodyRadiusMin = 0.5f;
    public const float BodyRadiusMax = 7.5f;

    public const float OrbiterSpeedMin = 0.1f;
    public const float OrbiterSpeedMax = 15f;

    public const float OrbiterRadiusMin = 0.5f;
    public const float OrbiterRadiusMax = 20f;

    public const float OrbiterEccentricityMin = 0.01f;
    public const float OrbiterEccentricityMax = 0.97f;

    public const float RotationSpeedMin = 0.1f;
    public const float RotationSpeedMax = 15f;

#region Orbiter Path Settings
    public const float OrbiterPathMarginFactor = 0.5f;
    public const float OrbiterPathMarginMax = 20f;
    public const int OrbiterPathSampleMin = 48;
    public const int OrbiterPathSampleMax = 128;
    public const float OrbiterPathTargetSegmentLength = 0.6f;
    public const int OrbiterPathSmoothPassesBaseline = 80;
    public const int OrbiterPathSmoothPassesMax = 120;
    public const float OrbiterJaggednessThreshold = 0.08f;
#endregion
}