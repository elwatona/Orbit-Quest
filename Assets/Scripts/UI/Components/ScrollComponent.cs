using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class ScrollComponent
{
    [SerializeField] ScrollRect _scrollRect;
    [SerializeField] GameObject _prefab;
    private Transform _contentTransform => _scrollRect.content;
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
