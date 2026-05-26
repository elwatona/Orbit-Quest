using UnityEngine;
using System;
public class OrbitRenderer
{

    [Serializable]
    public struct Data
    {
        public LineRenderer yAxisRenderer;
        public LineRenderer cameraPerpendicularRenderer;
    }
    private Data _data;
    readonly Transform _transform;
    readonly Transform _camera;
    public OrbitRenderer(Data data, Transform transform, Transform camera)
    {
        _transform = transform;
        _camera = camera;
        SetData(data);
    }
    public void SetData(Data data)
    {
        _data = data;
        MountRenderer(_data.yAxisRenderer, _transform.position, Vector3.up);
        MountRenderer(_data.cameraPerpendicularRenderer, _transform.position, _camera.forward); 
    }
    public void Update()
    {
        MountRenderer(_data.yAxisRenderer, _transform.position, Vector3.up);
        MountRenderer(_data.cameraPerpendicularRenderer, _transform.position, _camera.forward);
    }
    private void MountRenderer(LineRenderer lineRenderer, Vector3 position, Vector3 direction)
    {
        int segments = 64;
        float radius = _transform.lossyScale.x * 0.5f;
        lineRenderer.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            Vector3 localPos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Quaternion q = Quaternion.FromToRotation(Vector3.forward, direction.normalized);
            Vector3 rotated = q * localPos;
            lineRenderer.SetPosition(i, position + rotated);
        }
    }
}