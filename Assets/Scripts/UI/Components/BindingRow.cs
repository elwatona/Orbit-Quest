using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BindingRow : IScrollItem
{
    readonly GameObject _root;
    readonly TextMeshProUGUI _title;
    readonly TextMeshProUGUI _key;
    readonly Button _rebind;
    readonly Button _reset;
    string _keyDisplay;

    public BindingRow(Transform transform)
    {
        _root = transform.gameObject;
        _title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        _key = transform.Find("Key").GetComponent<TextMeshProUGUI>();
        _rebind = transform.Find("Rebind").GetComponent<Button>();
        _reset = transform.Find("Reset").GetComponent<Button>();
    }

    public GameObject Root => _root;
    public InputBindingInfo Info { get; private set; }

    public void Bind(
        InputBindingInfo info,
        string titleLabel,
        string keyDisplay,
        Action onRebind,
        Action onReset)
    {
        Info = info;
        _keyDisplay = keyDisplay;
        _title.text = titleLabel;
        _key.text = keyDisplay;

        _rebind.onClick.RemoveAllListeners();
        _reset.onClick.RemoveAllListeners();
        _rebind.onClick.AddListener(() => onRebind?.Invoke());
        _reset.onClick.AddListener(() => onReset?.Invoke());

        SetActive(true);
    }

    public void SetKey(string display)
    {
        _keyDisplay = display;
        _key.text = display;
    }

    public void SetListening(bool listening)
    {
        _key.text = listening ? "..." : _keyDisplay;
    }

    public void SetActive(bool active)
    {
        _root.SetActive(active);
    }
}
