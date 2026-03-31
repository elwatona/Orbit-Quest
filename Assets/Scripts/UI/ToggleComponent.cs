using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleComponent
{
    readonly Toggle _toggle;
    public ToggleComponent(Transform transform)
    {
        _toggle = transform.Find("Toggle").GetComponent<Toggle>();
    }
    public void UpdateValue(bool value)
    {
        _toggle.isOn = value;
    }
}