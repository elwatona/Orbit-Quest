using UnityEngine;
public interface IPanel
{
    GameObject Root { get; }
    void Toggle(bool active);
}