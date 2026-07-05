using UnityEngine;
using TMPro;
using System;

[Serializable]
public class PresetLimits
{
    [SerializeField] Limits _limits;
    [SerializeField] TMP_InputField _minX;
    [SerializeField] TMP_InputField _minY;
    [SerializeField] TMP_InputField _maxX;
    [SerializeField] TMP_InputField _maxY;
    public void Start()
    {
        SetLimits(_limits.Min, _limits.Max);
        _minX.onValueChanged.AddListener(OnMinXChanged);
        _minY.onValueChanged.AddListener(OnMinYChanged);
        _maxX.onValueChanged.AddListener(OnMaxXChanged);
        _maxY.onValueChanged.AddListener(OnMaxYChanged);

        PresetEvents.OnPresetLoaded += HandlePresetLoaded;
    }
    void OnDisable()
    {
        PresetEvents.OnPresetLoaded -= HandlePresetLoaded;
    }
    void HandlePresetLoaded(PresetData presetData)
    {
        
        SetLimits(presetData.Limits.min, presetData.Limits.max);
    }
    private void OnMinXChanged(string value)
    {
        float.TryParse(value, out float minX);
        if(minX == _limits.Min.x) return;
        Vector2 newMin = new Vector2(minX, _limits.Min.y);

        _limits.SetLimits(newMin, _limits.Max);
    }
    private void OnMinYChanged(string value)
    {
        float.TryParse(value, out float minY);
        if(minY == _limits.Min.y) return;
        Vector2 newMin = new Vector2(_limits.Min.x, minY);
        _limits.SetLimits(newMin, _limits.Max);
    }
    private void OnMaxXChanged(string value)
    {
        float.TryParse(value, out float maxX);
        if(maxX == _limits.Max.x) return;
        Vector2 newMax = new Vector2(maxX, _limits.Max.y);
        _limits.SetLimits(_limits.Min, newMax);
    }
    private void OnMaxYChanged(string value)
    {
        float.TryParse(value, out float maxY);
        if(maxY == _limits.Max.y) return;
        Vector2 newMax = new Vector2(_limits.Max.x, maxY);
        _limits.SetLimits(_limits.Min, newMax);
    }
    public void SetLimits(Vector2 min, Vector2 max)
    {
        _minX.text = min.x.ToString();
        _minY.text = min.y.ToString();
        _maxX.text = max.x.ToString();
        _maxY.text = max.y.ToString();
    }
}