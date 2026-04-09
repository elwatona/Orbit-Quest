using UnityEngine;
using TMPro;
using System;
public class DropdownComponent
{
    private GameObject _dropdownRoot;
    public Action<int> OnValueChanged;
    readonly TMP_Dropdown _dropdown;
    public DropdownComponent(Transform transform)
    {
        _dropdownRoot = transform.gameObject;
        _dropdown = transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        _dropdown.onValueChanged.AddListener(UpdateValue);
    }
    public void SetActive(bool active)
    {
        _dropdownRoot.SetActive(active);
    }
    public void UpdateValue(int value)
    {
        _dropdown.value = value;
        OnValueChanged?.Invoke(value);
    }
}