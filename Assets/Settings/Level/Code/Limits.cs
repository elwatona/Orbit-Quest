using UnityEngine;
using System;
[CreateAssetMenu(fileName = "Limits", menuName = "Proto-Pablo/Limits", order = 0)]
[Serializable]
public class Limits : ScriptableObject
{
    [SerializeField] Vector2 _min;
    [SerializeField] Vector2 _max;
    public Vector2 Min => _min;
    public Vector2 Max => _max;
    public bool IsInside(float x, float y)
    {
        return x >= Min.x && x <= Max.x && y >= Min.y && y <= Max.y;
    }
    public void SetLimits(Vector2 min, Vector2 max)
    {
        _min = min;
        _max = max;
        UpdateShaders();
    }
    private void UpdateShaders()
    {
        Shader.SetGlobalVector("_Limits_Min", Min);
        Shader.SetGlobalVector("_Limits_Max", Max);
    }
}

[Serializable]
public struct LimitsData
{
    public Vector2 min;
    public Vector2 max;

    public LimitsData(Vector2 min, Vector2 max)
    {
        this.min = min;
        this.max = max;
    }

    public static LimitsData From(Limits limits) => new LimitsData(limits.Min, limits.Max);
}