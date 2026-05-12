using System;
using UnityEngine;

[Serializable]
public class FullScreenShaderController 
{
    [SerializeField] Material _material;
    [SerializeField] Transform _playerTransform;
    private Limits _limits;
    public void SetLimits(Limits limits)
    {
        _limits = limits;
    }

    public void Update()
    {
        if (_material == null || _playerTransform == null || _limits == null)
            return;

        float marginToNearestEdge = Mathf.Min(
            _playerTransform.position.x - _limits.Min.x,
            _limits.Max.x - _playerTransform.position.x,
            _playerTransform.position.y - _limits.Min.y,
            _limits.Max.y - _playerTransform.position.y);

        float halfShortAxis = Mathf.Max(1e-4f, Mathf.Min((_limits.Max.x - _limits.Min.x) * 0.5f, (_limits.Max.y - _limits.Min.y) * 0.5f));
        float normalized = Mathf.Clamp01(marginToNearestEdge / halfShortAxis);

        Shader.SetGlobalFloat("_GlobalDistanceToBounds", normalized);
    }
}
