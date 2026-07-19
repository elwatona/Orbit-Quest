using UnityEngine;
using TMPro;

public class TitleHeader : IScrollItem
{
    readonly GameObject _root;
    readonly TextMeshProUGUI _title;

    public TitleHeader(Transform transform)
    {
        _root = transform.gameObject;
        _title = transform.GetComponent<TextMeshProUGUI>();
    }

    public GameObject Root => _root;

    public void SetTitle(string title)
    {
        _title.text = title;
    }

    public void SetActive(bool active)
    {
        _root.SetActive(active);
    }
}
