using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SliderComponent
{
    readonly TMP_InputField _valueText;
    readonly Slider _slider;
    private float _minValue;
    private float _maxValue;
    private GameObject _sliderRoot;
    public float Value { get; private set; }
    public Action<float> OnValueChanged;
    public SliderComponent(Transform transform)
    {
        _sliderRoot = transform.gameObject;
        _valueText = transform.Find("Variable/Value").GetComponent<TMP_InputField>();
        _slider = transform.Find("Variable/Slider").GetComponent<Slider>();
        _slider.onValueChanged.AddListener(UpdateValueText);
        _valueText.onEndEdit.AddListener(UpdateSliderValue);
    }
    public void SetActive(bool active)
    {
        _sliderRoot.SetActive(active);
    }
    public void UpdateValueRange(float minValue, float maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _slider.minValue = _minValue;
        _slider.maxValue = _maxValue;
    }
    public void UpdateValue(float value)
    {
        Value = value;
        UpdateValueText(value);
        UpdateSliderValue(value.ToString("F2"));
    }
    public void UpdateValueText(float value)
    {
        _valueText.text = value.ToString("F2");
        OnValueChanged?.Invoke(value);
    }
    public void UpdateSliderValue(string value)
    {
        float floatValue = float.Parse(value);
        _slider.value = floatValue;
        OnValueChanged?.Invoke(floatValue);
    }
}