using UnityEngine;

public class SkyboxShaderController
{
    readonly Material _runtimeMaterial;
    readonly int _coldnessId = Shader.PropertyToID("_Coldness");
    readonly int _densityId = Shader.PropertyToID("_Density");

    public SkyboxShaderController(Material source)
    {
        if (source == null)
        {
            Debug.LogError("SkyboxShaderController requires a skybox material.");
            return;
        }

        _runtimeMaterial = new Material(source);
    }

    public void UpdateColdness(float value)
    {
        if (_runtimeMaterial == null)
            return;
        _runtimeMaterial.SetFloat(_coldnessId, Mathf.Clamp(value, GraphicsTuning.ColdnessMin, GraphicsTuning.ColdnessMax));
    }

    public void UpdateDensity(float value)
    {
        if (_runtimeMaterial == null)
            return;
        _runtimeMaterial.SetFloat(_densityId, Mathf.Clamp(value, GraphicsTuning.DensityMin, GraphicsTuning.DensityMax));
    }

    public void SetEnabled(bool enabled)
    {
        if (_runtimeMaterial == null)
            return;
        RenderSettings.skybox = enabled ? _runtimeMaterial : null;
    }

    public void Apply(GraphicsSettings graphics)
    {
        if (graphics == null)
            return;

        bool enabled = graphics.SkyboxEnabled.Value;
        SetEnabled(enabled);
        if (!enabled)
            return;

        UpdateColdness(graphics.SkyboxColdness.Value);
        UpdateDensity(graphics.SkyboxDensity.Value);
    }
}
