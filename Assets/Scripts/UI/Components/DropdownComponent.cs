using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class DropdownComponent
{
    readonly GameObject _dropdownRoot;
    public Action<int> OnValueChanged;
    readonly TMP_Dropdown _dropdown;
    public DropdownComponent(Transform transform)
    {
        _dropdownRoot = transform.gameObject;
        _dropdown = transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        _dropdown.onValueChanged.AddListener(HandleDropdownChanged);
    }
    public void SetActive(bool active)
    {
        _dropdownRoot.SetActive(active);
    }
    public void SetInteractable(bool interactable)
    {
        _dropdown.interactable = interactable;
    }
    public void SetOptions(IReadOnlyList<string> options)
    {
        _dropdown.ClearOptions();
        if (options == null || options.Count == 0) return;
        var list = new List<TMP_Dropdown.OptionData>(options.Count);
        foreach (string option in options)
            list.Add(new TMP_Dropdown.OptionData(option));
        _dropdown.AddOptions(list);
    }
    public void SetValueWithoutNotify(int value)
    {
        _dropdown.SetValueWithoutNotify(value);
        _dropdown.RefreshShownValue();
    }
    public void UpdateValue(int value)
    {
        SetValueWithoutNotify(value);
        OnValueChanged?.Invoke(value);
    }
    void HandleDropdownChanged(int value)
    {
        OnValueChanged?.Invoke(value);
    }
}
