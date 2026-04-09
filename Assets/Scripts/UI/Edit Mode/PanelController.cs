using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class PanelController
{
    [SerializeField] GameObject _panelRoot;
    [SerializeField] Transform[] _sliders;
    [SerializeField] Transform[] _toggles;
    [SerializeField] Transform[] _dropdowns;
    public SliderComponent[] SliderComponents { get; private set; }
    public ToggleComponent[] ToggleComponents { get; private set; }
    public DropdownComponent[] DropdownComponents { get; private set; }
    public enum ComponentType
    {
        Slider,
        Toggle,
        Dropdown
    }
    public void Initialize()
    {
        SliderComponents = CacheSliderComponents(_sliders);
        ToggleComponents = CacheToggleComponents(_toggles);
        DropdownComponents = CacheDropdownComponents(_dropdowns);
    }
    public void SetActive(bool active)
    {
        _panelRoot.SetActive(active);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_panelRoot.transform as RectTransform);
    }
    public void ToggleActive()
    {
        SetActive(!_panelRoot.activeSelf);
    }
    public void SetComponentsActiveValue(ComponentType type, int index, bool active)
    {
        switch (type)
        {
            case ComponentType.Slider:
                SliderComponents[index].SetActive(active);
                break;
            case ComponentType.Toggle:
                ToggleComponents[index].SetActive(active);
                break;
            case ComponentType.Dropdown:
                DropdownComponents[index].SetActive(active);
                break;
            default:
                Debug.LogError("Invalid component type: " + type);
                break;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_panelRoot.transform as RectTransform);
    }
    private SliderComponent[] CacheSliderComponents(Transform[] sliderTransforms)
    {
        SliderComponent[] components = new SliderComponent[sliderTransforms.Length];
        for (int i = 0; i < sliderTransforms.Length; i++)
        {
            components[i] = new SliderComponent(sliderTransforms[i]);
        }
        return components;
    }   
    private ToggleComponent[] CacheToggleComponents(Transform[] toggleTransforms)
    {
        ToggleComponent[] components = new ToggleComponent[toggleTransforms.Length];
        for (int i = 0; i < toggleTransforms.Length; i++)
        {
            components[i] = new ToggleComponent(toggleTransforms[i]);
        }
        return components;
    }
    private DropdownComponent[] CacheDropdownComponents(Transform[] dropdownTransforms)
    {
        DropdownComponent[] components = new DropdownComponent[dropdownTransforms.Length];
        for (int i = 0; i < dropdownTransforms.Length; i++)
        {
            components[i] = new DropdownComponent(dropdownTransforms[i]);
        }
        return components;
    }
}
