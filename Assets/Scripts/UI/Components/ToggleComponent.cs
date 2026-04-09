using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleComponent
{
    readonly Toggle _toggle;
    private GameObject _toggleRoot;
    public ToggleComponent(Transform transform)
    {
        _toggleRoot = transform.gameObject;
        _toggle = transform.Find("Toggle").GetComponent<Toggle>();
    }
    public void SetActive(bool active)
    {
        _toggleRoot.SetActive(active);
    }
    public void UpdateValue(bool value)
    {
        _toggle.isOn = value;
    }
}