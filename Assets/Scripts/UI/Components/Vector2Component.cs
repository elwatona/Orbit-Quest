using UnityEngine;
using TMPro;
using System;
public class Vector2Component
{
    readonly GameObject _vector2Root;
    readonly TMP_InputField _xField;
    readonly TMP_InputField _yField;
    public Vector2 Value { get; private set; }
    public Action<Vector2> OnValueChanged;
    public Vector2Component(Transform transform)
    {
        _vector2Root = transform.gameObject;
        _xField = transform.Find("Variable/X").GetComponent<TMP_InputField>();
        _yField = transform.Find("Variable/Y").GetComponent<TMP_InputField>();
        _xField.onValueChanged.AddListener(OnXValueChanged);
        _yField.onValueChanged.AddListener(OnYValueChanged);
    }
    public void SetActive(bool active)
    {
        _vector2Root.SetActive(active);
    }
    public void UpdateValue(Vector2 value, bool notify = false)
    {
        Value = value;
        _xField.text = value.x.ToString();
        _yField.text = value.y.ToString();
        if(notify) OnValueChanged?.Invoke(Value);
    }
    private void OnXValueChanged(string value)
    {
        float.TryParse(value, out float x);
        if(x == Value.x) return;
        Value = new Vector2(x, Value.y);
        OnValueChanged?.Invoke(Value);
    }
    private void OnYValueChanged(string value)
    {
        float.TryParse(value, out float y);
        if(y == Value.y) return;
        Value = new Vector2(Value.x, y);
        OnValueChanged?.Invoke(Value);
    }
}