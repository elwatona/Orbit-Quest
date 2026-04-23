using UnityEngine;
public interface ILevelBounds
{
    Limits Limits { get; }
    void SetLimits(Limits limits);
}