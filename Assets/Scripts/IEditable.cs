using UnityEngine;
using System;
public interface IEditable
{
    event Action OnEditableDragged;
    static event Action<IEditable> OnEditableClicked;
    Transform transform { get; }
    EditableData Data { get; } 
    void Selected();
    void Deselected();
    void Deactivate();
    void UpdateBodyRadius(float radius);
    void UpdateBodyRotationSpeed(float rotationSpeed);
    void UpdateOrbitRadius(float radius);
    void UpdateOrbitGravity(float gravity);
    void UpdateOrbiterSpeed(float speed);
    void UpdateOrbiterRadius(float radius);
    void UpdateOrbiterEccentricity(float eccentricity);
    void UpdateOrbiterTargets(IEditable[] targets);
    bool HasOrbiter();
}
[Serializable]
public struct EditableData
{
    public AstroType type;
    public EditableBodyData body;
    public EditableOrbitData orbit;
    public EditableOrbiterData orbiter;
}
[Serializable]
public struct EditableBodyData
{
    public float radius;
    public float rotationSpeed;
}
[Serializable]
public struct EditableOrbitData
{
    public float radius;
    public float gravity;
}
[Serializable]
public struct EditableOrbiterData
{
    public float speed;
    public float radius;
    public float eccentricity;
    public int[] targetIndices;
    public IEditable[] targets;
}