using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextComponent
{
    readonly GameObject _root;
    readonly TextMeshProUGUI _title;
    readonly TextMeshProUGUI _value;
    readonly Image _background;
    public TextComponent(Transform transform)
    {
        _root = transform.gameObject;
        _title = transform.Find("Title").GetComponent<TextMeshProUGUI>();
        _value = transform.Find("Value").GetComponent<TextMeshProUGUI>();
        _background = transform.GetComponent<Image>();
    }
    public void SetActive(bool active)
    {
        _root.SetActive(active);
    }
    public void SetTitle(string title)
    {
        _title.text = title;
    }
    public void SetValue(string value)
    {
        _value.text = value;
    }
    public void SetBackground(Color color)
    {
        _background.color = color;
    }
}