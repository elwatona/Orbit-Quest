using System;
using UnityEngine;
public interface IOrbitable
{
    public OrbitData Data { get;}
    public void EnterOrbit();
    public void ExitOrbit();
    public void SetData(OrbitData data);
}
[Serializable]
public struct OrbitData
{
    public AstroType type;
    [HideInInspector] public Transform transform;
    [Range(1f, 10f)] public float radius;
    [Range(50, 200)] public float gravity;
    public Vector3 velocity;
}
public enum AstroType
{
    None,
    Sun,
    Planet,
    Asteroid
}