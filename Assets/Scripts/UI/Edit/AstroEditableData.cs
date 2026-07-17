using UnityEngine;
using System;

public class AstroEditableData : IPanel
{
    public GameObject Root { get; private set; }
    readonly public DropdownComponent Type;
    readonly public SliderComponent OrbitGravity;
    readonly public SliderComponent OrbitRadius;
    readonly public SliderComponent BodyRadius;
    readonly public SliderComponent BodyRotationSpeed;
    public AstroEditableData(AstroEditableDataDependencies dependencies)
    {
        Root = dependencies.Root;
        Type = new DropdownComponent(dependencies.Type);
        OrbitGravity = new SliderComponent(dependencies.OrbitGravity);
        OrbitRadius = new SliderComponent(dependencies.OrbitRadius);
        BodyRadius = new SliderComponent(dependencies.BodyRadius);
        BodyRotationSpeed = new SliderComponent(dependencies.BodyRotationSpeed);

        OrbitGravity.UpdateValueRange(AstroTuning.GravityMin, AstroTuning.GravityMax);
        OrbitRadius.UpdateValueRange(AstroTuning.OrbitRadiusMin, AstroTuning.OrbitRadiusMax);
        BodyRadius.UpdateValueRange(AstroTuning.BodyRadiusMin, AstroTuning.BodyRadiusMax);
        BodyRotationSpeed.UpdateValueRange(AstroTuning.RotationSpeedMin, AstroTuning.RotationSpeedMax);
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
        
        UpdateAstroDataToUI(next);
    }
    private void SubscribeToEditable(IEditable editable)
    {
        OrbitGravity.OnValueChanged += editable.UpdateOrbitGravity;
        OrbitRadius.OnValueChanged += editable.UpdateOrbitRadius;
        BodyRadius.OnValueChanged += editable.UpdateBodyRadius;
        BodyRotationSpeed.OnValueChanged += editable.UpdateBodyRotationSpeed;
    }
    private void UnsubscribeFromEditable(IEditable editable)
    {
        OrbitGravity.OnValueChanged -= editable.UpdateOrbitGravity;
        OrbitRadius.OnValueChanged -= editable.UpdateOrbitRadius;
        BodyRadius.OnValueChanged -= editable.UpdateBodyRadius;
        BodyRotationSpeed.OnValueChanged -= editable.UpdateBodyRotationSpeed;
    }
    private void UpdateAstroDataToUI(IEditable editable)
    {
        Type.UpdateValue((int)editable.Data.type);
        OrbitGravity.UpdateValue(editable.Data.orbit.gravity);
        OrbitRadius.UpdateValue(editable.Data.orbit.radius);
        BodyRadius.UpdateValue(editable.Data.body.radius);
        BodyRotationSpeed.UpdateValue(editable.Data.body.rotationSpeed);
    }
}

[Serializable]
public class AstroEditableDataDependencies : PanelDependencies
{
    public Transform Type;
    public Transform OrbitGravity;
    public Transform OrbitRadius;
    public Transform BodyRadius;
    public Transform BodyRotationSpeed;
}