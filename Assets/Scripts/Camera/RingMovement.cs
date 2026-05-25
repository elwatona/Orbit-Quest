using UnityEngine;
using Unity.Cinemachine;

public class RingMovement
{
    public struct Settings
    {
        public float speed;
    }
    public struct RingData
    {
        public float radius;
        public float height;
    }
    readonly CinemachineCamera _camera;
    readonly Transform _transform;
    readonly Settings _settings;
    readonly RingData[] _ringsData;
    public RingMovement(CinemachineCamera camera, Transform transform, Settings settings, RingData[] ringsData)
    {
        _camera = camera;
        _transform = transform;
        _settings = settings;
        _ringsData = ringsData;
    }
    public void VisualizeRings()
    {
        foreach (var ringData in _ringsData)
        {
            Debug.DrawRay(_transform.position, _transform.forward * ringData.radius, Color.red);
        }
    }
    public void Update(float deltaTime)
    {

    }
}