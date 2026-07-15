using UnityEngine;
using System;
using System.Collections.Generic;

public class AstroSelector : IPanel
{
    public GameObject Root { get; private set; }
    public List<IEditable> SelectedAstros { get; private set; } = new List<IEditable>();
    public AstroSelector(AstroSelectorDependencies dependencies)
    {
        Root = dependencies.Root;
    }
    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
    public void StartSelectingTargets(List<IEditable> baseAstros)
    {
        foreach(IEditable editable in baseAstros) 
        {
            SelectedAstros.Add(editable);
            editable.Selected();
        }
    }
    public void StopSelectingTargets()
    {
        foreach(IEditable editable in SelectedAstros) editable.Deselected();
        SelectedAstros.Clear();
    }
    public void AstroTargeted(IEditable editable)
    {
        if(SelectedAstros.Contains(editable))
        {
            SelectedAstros.Remove(editable);
            editable.Deselected();
        }
        else
        {
            SelectedAstros.Add(editable);
            editable.Selected();
        }
    }
}

[Serializable]
public class AstroSelectorDependencies : PanelDependencies
{}