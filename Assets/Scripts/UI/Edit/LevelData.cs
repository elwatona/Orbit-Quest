using UnityEngine;
using System;
public class LevelData : IPanel
{
    public readonly Vector2Component Max;
    public readonly Vector2Component Min;
    public readonly Limits Limits;
    public GameObject Root { get; private set; }
    public LevelData(LevelDataDependencies dependencies)
    {
        Root = dependencies.Root;
        Max = new Vector2Component(dependencies.Max.transform);
        Min = new Vector2Component(dependencies.Min.transform);
        Limits = dependencies.Limits;
    }
    public void Toggle(bool active) 
    {
        Root.SetActive(active);
        if(active) 
        {
            Max.UpdateValue(Limits.Max);
            Min.UpdateValue(Limits.Min);
            Max.OnValueChanged += HandleMaxValueChanged;
            Min.OnValueChanged += HandleMinValueChanged;
        }
        else
        {
            Max.OnValueChanged -= HandleMaxValueChanged;
            Min.OnValueChanged -= HandleMinValueChanged;
        }
    }
    void HandleMaxValueChanged(Vector2 value)
    {
        Limits.SetLimits(Limits.Min, value);
    }
    void HandleMinValueChanged(Vector2 value)
    {
        Limits.SetLimits(value, Limits.Max);
    }

}

[Serializable]
public class LevelDataDependencies : PanelDependencies
{
    public Limits Limits;
    public Transform Min;
    public Transform Max;
}