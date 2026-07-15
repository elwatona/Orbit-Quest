using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class ScrollComponent
{
    readonly GameObject _scrollRoot;
    readonly ScrollRect _scrollRect;
    readonly Transform _contentTransform;
    readonly GameObject _prefab;
    public ScrollComponent(Transform transform, GameObject prefab)
    {
        _scrollRoot = transform.gameObject;
        _scrollRect = transform.GetComponentInChildren<ScrollRect>();
        _contentTransform = _scrollRect.content;
        _prefab = prefab;
    }
    public void SetActive(bool active)
    {
        _scrollRoot.SetActive(active);
    }
    public void AddItem(string item)
    {
        GameObject itemObject = GameObject.Instantiate(_prefab, _contentTransform);
        itemObject.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = item;
    }
    public void AddItems(string[] items)
    {
        foreach (string item in items) AddItem(item);
    }
    public void ClearItems()
    {
        foreach (Transform child in _contentTransform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
