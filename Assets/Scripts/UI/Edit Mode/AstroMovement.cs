using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class AstroMovement : IPanel
{
    public GameObject Root { get; private set; }
    readonly public ScrollComponent TargetsToOrbit;
    readonly public SliderComponent Radius;
    readonly public SliderComponent Speed;
    readonly public SliderComponent Eccentricity;
    public List<IEditable> TargetsToOrbitList { get; private set; } = new List<IEditable>();
    public AstroMovement(AstroMovementDependencies dependencies)
    {
        Root = dependencies.Root;
        TargetsToOrbit = new ScrollComponent(dependencies.TargetsToOrbit, dependencies.TargetsToOrbitPrefab);
        Radius = new SliderComponent(dependencies.Radius);
        Speed = new SliderComponent(dependencies.Speed);
        Eccentricity = new SliderComponent(dependencies.Eccentricity);

        Radius.UpdateValueRange(AstroTuning.OrbiterRadiusMin, AstroTuning.OrbiterRadiusMax);
        Speed.UpdateValueRange(AstroTuning.OrbiterSpeedMin, AstroTuning.OrbiterSpeedMax);
        Eccentricity.UpdateValueRange(AstroTuning.OrbiterEccentricityMin, AstroTuning.OrbiterEccentricityMax);

    }
    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
    public void InyectEditable(IEditable next, IEditable original = null)
    {
        if(next == null) return;

        if(original != null)
        {
            UnsubscribeFromEditable(original);
        }
        if(original != next) SubscribeToEditable(next);
        
        LoadTargetsToOrbitDataToUI(next);
        UpdateAstroDataToUI(next);
    }
    public void LoadTargetsToOrbitDataToUI(IEditable editable)
    {
        TargetsToOrbit.ClearItems();
        TargetsToOrbitList = editable.Data.orbiter.targets.ToList();

        if(TargetsToOrbitList.Count == 0) return;
        else if (TargetsToOrbitList.Count == 1)
            TargetsToOrbit.AddItem(TargetsToOrbitList[0].transform.name);
        else if(TargetsToOrbitList.Count > 1) 
            TargetsToOrbit.AddItems(TargetsToOrbitList.Select(target => target.transform.name).ToArray());
    }
    public void ClearTargetsToOrbit()
    {
        TargetsToOrbit.ClearItems();
        TargetsToOrbitList.Clear();
    }

    private void SubscribeToEditable(IEditable editable)
    {
        Radius.OnValueChanged += editable.UpdateOrbiterRadius;
        Speed.OnValueChanged += editable.UpdateOrbiterSpeed;
        Eccentricity.OnValueChanged += editable.UpdateOrbiterEccentricity;
    }
    private void UnsubscribeFromEditable(IEditable editable)
    {
        Radius.OnValueChanged -= editable.UpdateOrbiterRadius;
        Speed.OnValueChanged -= editable.UpdateOrbiterSpeed;
        Eccentricity.OnValueChanged -= editable.UpdateOrbiterEccentricity;
    }
    private void UpdateAstroDataToUI(IEditable editable)
    {
        Radius.UpdateValue(editable.Data.orbiter.radius);
        Speed.UpdateValue(editable.Data.orbiter.speed);
        Eccentricity.UpdateValue(editable.Data.orbiter.eccentricity);
    }
}

[Serializable]
public class AstroMovementDependencies : PanelDependencies
{
    public GameObject TargetsToOrbitPrefab;
    public Transform TargetsToOrbit;
    public Transform Radius;
    public Transform Speed;
    public Transform Eccentricity;
}