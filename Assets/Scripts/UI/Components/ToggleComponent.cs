using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleComponent
{
    readonly Toggle _toggle;
    readonly GameObject _toggleRoot;
    bool _suppress;

    public Action<bool> OnValueChanged;

    public ToggleComponent(Transform transform)
    {
        _toggleRoot = transform.gameObject;
        _toggle = transform.Find("Toggle").GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(HandleToggle);
    }

    public void SetActive(bool active)
    {
        _toggleRoot.SetActive(active);
    }

    public void UpdateValue(bool value)
    {
        _suppress = true;
        _toggle.isOn = value;
        _suppress = false;
    }

    void HandleToggle(bool value)
    {
        if (_suppress)
            return;
        OnValueChanged?.Invoke(value);
    }
}
