using UnityEngine;

public interface IScrollItem
{
    GameObject Root { get; }
    void SetActive(bool active);
}
