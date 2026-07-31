using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BindingRow : IScrollItem
{
    static readonly string[] PressTypeOptions = { "Press", "Hold", "Release" };
    static readonly Color ListeningColor = new Color(1f, 0.82f, 0.35f, 1f);

    readonly GameObject _root;
    readonly TextMeshProUGUI _title;
    readonly Button _keyButton;
    readonly TextMeshProUGUI _keyLabel;
    readonly Image _keyImage;
    readonly Button _altKeyButton;
    readonly TextMeshProUGUI _altKeyLabel;
    readonly Image _altKeyImage;
    readonly Button _reset;
    readonly DropdownComponent _pressType;
    readonly GameObject _altKeyRoot;

    string _keyDisplay;
    string _altKeyDisplay;
    Color _keyNormalColor;
    Color _altKeyNormalColor;
    bool _listeningKey;
    bool _listeningAlt;

    public BindingRow(Transform transform)
    {
        _root = transform.gameObject;
        _title = transform.Find("Title").GetComponent<TextMeshProUGUI>();

        Transform keyRoot = transform.Find("Key");
        _keyButton = keyRoot.GetComponent<Button>();
        _keyLabel = keyRoot.Find("Text").GetComponent<TextMeshProUGUI>();
        _keyImage = keyRoot.GetComponent<Image>();

        Transform altKeyRoot = transform.Find("AltKey");
        _altKeyRoot = altKeyRoot.gameObject;
        _altKeyButton = altKeyRoot.GetComponent<Button>();
        _altKeyLabel = altKeyRoot.Find("Text").GetComponent<TextMeshProUGUI>();
        _altKeyImage = altKeyRoot.GetComponent<Image>();

        _reset = transform.Find("Reset").GetComponent<Button>();

        Transform pressTypeRoot = transform.Find("PressType");
        _pressType = new DropdownComponent(pressTypeRoot);
        _pressType.SetOptions(PressTypeOptions);
        Transform pressTypeTitle = pressTypeRoot.Find("Title");
        if (pressTypeTitle != null)
            pressTypeTitle.gameObject.SetActive(false);

        if (_keyImage != null)
            _keyNormalColor = _keyImage.color;
        if (_altKeyImage != null)
            _altKeyNormalColor = _altKeyImage.color;
    }

    public GameObject Root => _root;
    public InputBindingInfo Info { get; private set; }
    public InputBindingInfo? AltInfo { get; private set; }

    public void Bind(
        InputBindingInfo primary,
        InputBindingInfo? alt,
        string titleLabel,
        string keyDisplay,
        string altKeyDisplay,
        Action onRebindPrimary,
        Action onRebindAlt,
        Action onReset,
        BindingPressType pressType,
        bool showPressType,
        bool showAlt,
        Action<int> onPressTypeChanged)
    {
        Info = primary;
        AltInfo = alt;
        _keyDisplay = keyDisplay;
        _altKeyDisplay = altKeyDisplay;
        _title.text = titleLabel;
        _keyLabel.text = keyDisplay;
        _altKeyLabel.text = altKeyDisplay;

        _keyButton.onClick.RemoveAllListeners();
        _altKeyButton.onClick.RemoveAllListeners();
        _reset.onClick.RemoveAllListeners();
        _keyButton.onClick.AddListener(() => onRebindPrimary?.Invoke());
        _altKeyButton.onClick.AddListener(() => onRebindAlt?.Invoke());
        _reset.onClick.AddListener(() => onReset?.Invoke());

        SetAltActive(showAlt);
        SetListening(false, alt: false);
        SetListening(false, alt: true);

        _pressType.OnValueChanged = null;
        _pressType.SetActive(showPressType);
        if (showPressType)
        {
            _pressType.SetValueWithoutNotify((int)pressType);
            _pressType.OnValueChanged = value => onPressTypeChanged?.Invoke(value);
        }

        SetActive(true);
    }

    public void SetKey(string display, bool alt = false)
    {
        if (alt)
        {
            _altKeyDisplay = display;
            _altKeyLabel.text = display;
            return;
        }

        _keyDisplay = display;
        _keyLabel.text = display;
    }

    public void SetListening(bool listening, bool alt = false)
    {
        if (alt)
        {
            _listeningAlt = listening;
            if (_altKeyImage != null)
                _altKeyImage.color = listening ? ListeningColor : _altKeyNormalColor;
            _altKeyLabel.text = listening ? "..." : _altKeyDisplay;
            return;
        }

        _listeningKey = listening;
        if (_keyImage != null)
            _keyImage.color = listening ? ListeningColor : _keyNormalColor;
        _keyLabel.text = listening ? "..." : _keyDisplay;
    }

    public void SetListeningPreview(string display, bool alt = false)
    {
        if (alt)
        {
            if (!_listeningAlt) return;
            _altKeyLabel.text = string.IsNullOrEmpty(display) ? "..." : display;
            return;
        }

        if (!_listeningKey) return;
        _keyLabel.text = string.IsNullOrEmpty(display) ? "..." : display;
    }

    public void SetAltActive(bool active)
    {
        _altKeyRoot.SetActive(active);
    }

    public void SetActive(bool active)
    {
        _root.SetActive(active);
    }
}
