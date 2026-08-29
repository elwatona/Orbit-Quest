using System.Collections.Generic;

public class GraphicsSettings
{
    public FloatParameter GridThickness { get; }
    public FloatParameter SkyboxColdness { get; }
    public FloatParameter SkyboxDensity { get; }
    public BoolParameter SkyboxEnabled { get; }

    public GraphicsSettings()
    {
        GridThickness = new FloatParameter(
            "gridThickness",
            "Grid Thickness",
            GraphicsTuning.ThicknessMin,
            GraphicsTuning.ThicknessMax,
            GraphicsTuning.ThicknessDefault);
        SkyboxColdness = new FloatParameter(
            "skyboxColdness",
            "Skybox Coldness",
            GraphicsTuning.ColdnessMin,
            GraphicsTuning.ColdnessMax,
            GraphicsTuning.ColdnessDefault);
        SkyboxDensity = new FloatParameter(
            "skyboxDensity",
            "Skybox Density",
            GraphicsTuning.DensityMin,
            GraphicsTuning.DensityMax,
            GraphicsTuning.DensityDefault);
        SkyboxEnabled = new BoolParameter(
            "skyboxEnabled",
            "Star Skybox",
            GraphicsTuning.SkyboxEnabledDefault);
    }

    public IReadOnlyList<FloatParameter> Parameters => new[]
    {
        GridThickness,
        SkyboxColdness,
        SkyboxDensity
    };

    public void ApplyData(GameSettingsData data)
    {
        GridThickness.SetValue(data.GridThickness);
        SkyboxColdness.SetValue(data.SkyboxColdness);
        SkyboxDensity.SetValue(data.SkyboxDensity);
        SkyboxEnabled.SetValue(data.SkyboxEnabled);
    }
}
