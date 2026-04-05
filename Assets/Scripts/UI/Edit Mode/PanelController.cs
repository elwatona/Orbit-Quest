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
    public void Initialize()
    {
        SliderComponents = CacheSliderComponents(_sliders);
        ToggleComponents = CacheToggleComponents(_toggles);
        DropdownComponents = CacheDropdownComponents(_dropdowns);
    }
    public void SetActive(bool active)
    {
        _panelRoot.SetActive(active);
    }
    public void ToggleActive()
    {
        SetActive(!_panelRoot.activeSelf);
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
