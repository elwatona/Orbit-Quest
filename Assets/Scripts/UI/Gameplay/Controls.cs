using UnityEngine;
using TMPro;
using System;

public class Controls : IPanel
{
    public GameObject Root {get; private set;}
    public Controls(ControlsDependencies dependencies)
    {
        Root = dependencies.Root;
    }

    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
}
[Serializable]
public class ControlsDependencies : PanelDependencies {}