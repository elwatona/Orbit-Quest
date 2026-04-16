using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public enum CameraViewType
    {
        SideView = 0,
        Isometric = 1
    }

    public enum ZoomMode
    {
        Auto = 0,
        OrthographicSize = 1,
        FieldOfView = 2,
        DistanceToTarget = 3
    }

    [Serializable]
    public class ViewSettings
    {
        public Vector3 Offset = new Vector3(0f, 2f, -10f);
        public Vector3 EulerAngles = Vector3.zero;
        public float DefaultZoom = 6f;
    }

    [Header("Target")]
    [SerializeField] Transform _target;

    [Header("Bounds (world space)")]
    [SerializeField] float _minX = -20f;
    [SerializeField] float _maxX = 20f;
    [SerializeField] float _minY = -10f;
    [SerializeField] float _maxY = 10f;

    [Header("Follow")]
    [SerializeField] float _positionSmoothTime = 0.12f;
    [SerializeField] float _rotationLerpSpeed = 10f;

    [Header("Views")]
    [SerializeField] CameraViewType _viewType = CameraViewType.SideView;
    [SerializeField] ViewSettings _sideView = new ViewSettings
    {
        Offset = new Vector3(0f, 2f, -10f),
        EulerAngles = Vector3.zero,
        DefaultZoom = 6f
    };
    [SerializeField] ViewSettings _isometricView = new ViewSettings
    {
        Offset = new Vector3(-5f, 8f, -5f),
        EulerAngles = new Vector3(45f, 45f, 0f),
        DefaultZoom = 50f
    };

    [Header("Zoom")]
    [SerializeField] ZoomMode _zoomMode = ZoomMode.Auto;
    [SerializeField] float _minZoom = 3f;
    [SerializeField] float _maxZoom = 8f;
    [SerializeField] float _zoomLerpSpeed = 8f;

    Camera _camera;
    Vector3 _positionVelocity;
    Vector3 _currentOffset;
    Quaternion _targetRotation;
    float _targetZoom;
    float _distanceZoomBaseOffsetMagnitude;

    public Transform Target
    {
        get => _target;
        set => _target = value;
    }

    public CameraViewType ViewType => _viewType;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        ApplyViewSettings(_viewType, snap: true);
        _targetZoom = GetCurrentZoom();
        _distanceZoomBaseOffsetMagnitude = Mathf.Max(0.001f, _currentOffset.magnitude);
    }

    void Update()
    {
        if (_target == null)
            return;

        UpdateFollowPosition();
        UpdateRotation();
        UpdateZoom();
    }

    public void SetViewType(CameraViewType viewType, bool snap = false)
    {
        if (_viewType == viewType)
            return;

        _viewType = viewType;
        ApplyViewSettings(viewType, snap);
    }

    public void ToggleViewType(bool snap = false)
    {
        SetViewType(_viewType == CameraViewType.SideView ? CameraViewType.Isometric : CameraViewType.SideView, snap);
    }

    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        _minX = Mathf.Min(minX, maxX);
        _maxX = Mathf.Max(minX, maxX);
        _minY = Mathf.Min(minY, maxY);
        _maxY = Mathf.Max(minY, maxY);
    }

    public void SetZoom(float zoomValue, bool snap = false)
    {
        _targetZoom = Mathf.Clamp(zoomValue, Mathf.Min(_minZoom, _maxZoom), Mathf.Max(_minZoom, _maxZoom));
        if (snap)
            ApplyZoomImmediate(_targetZoom);
    }

    public void AddZoomDelta(float delta)
    {
        SetZoom(_targetZoom + (delta), snap: false);
    }

    void ApplyViewSettings(CameraViewType viewType, bool snap)
    {
        ViewSettings settings = GetViewSettings(viewType);
        _currentOffset = settings.Offset;
        _targetRotation = Quaternion.Euler(settings.EulerAngles);
        SetZoom(settings.DefaultZoom, snap);

        if (snap && _target != null)
        {
            Vector3 desired = GetDesiredPosition();
            transform.position = desired;
            transform.rotation = _targetRotation;
        }

        if (_zoomMode == ZoomMode.DistanceToTarget)
            _distanceZoomBaseOffsetMagnitude = Mathf.Max(0.001f, _currentOffset.magnitude);
    }

    ViewSettings GetViewSettings(CameraViewType viewType)
    {
        return viewType == CameraViewType.SideView ? _sideView : _isometricView;
    }

    void UpdateFollowPosition()
    {
        Vector3 desired = GetDesiredPosition();
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _positionVelocity, Mathf.Max(0.0001f, _positionSmoothTime));
    }

    Vector3 GetDesiredPosition()
    {
        Vector3 baseDesired = _target.position + _currentOffset;
        float clampedX = Mathf.Clamp(baseDesired.x, Mathf.Min(_minX, _maxX), Mathf.Max(_minX, _maxX));
        float clampedY = Mathf.Clamp(baseDesired.y, Mathf.Min(_minY, _maxY), Mathf.Max(_minY, _maxY));
        return new Vector3(clampedX, clampedY, baseDesired.z);
    }

    void UpdateRotation()
    {
        float t = 1f - Mathf.Exp(-Mathf.Max(0f, _rotationLerpSpeed) * Time.unscaledDeltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, t);
    }

    void UpdateZoom()
    {
        ZoomMode effectiveZoomMode = ResolveZoomMode();
        float t = 1f - Mathf.Exp(-Mathf.Max(0f, _zoomLerpSpeed) * Time.unscaledDeltaTime);

        if (effectiveZoomMode == ZoomMode.OrthographicSize)
        {
            if (!_camera.orthographic)
                return;

            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetZoom, t);
            return;
        }

        if (effectiveZoomMode == ZoomMode.FieldOfView)
        {
            if (_camera.orthographic)
                return;

            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetZoom, t);
            return;
        }

        if (effectiveZoomMode == ZoomMode.DistanceToTarget)
        {
            float zoomMin = Mathf.Min(_minZoom, _maxZoom);
            float zoomMax = Mathf.Max(_minZoom, _maxZoom);
            float normalized = Mathf.InverseLerp(zoomMin, zoomMax, _targetZoom);
            float desiredMagnitude = Mathf.Lerp(_distanceZoomBaseOffsetMagnitude, _distanceZoomBaseOffsetMagnitude * 2f, normalized);

            if (_currentOffset.sqrMagnitude > 0.0001f)
                _currentOffset = _currentOffset.normalized * desiredMagnitude;

            return;
        }
    }

    ZoomMode ResolveZoomMode()
    {
        if (_zoomMode != ZoomMode.Auto)
            return _zoomMode;

        return _camera.orthographic ? ZoomMode.OrthographicSize : ZoomMode.FieldOfView;
    }

    float GetCurrentZoom()
    {
        ZoomMode effectiveZoomMode = ResolveZoomMode();
        if (effectiveZoomMode == ZoomMode.OrthographicSize)
            return _camera.orthographicSize;
        if (effectiveZoomMode == ZoomMode.FieldOfView)
            return _camera.fieldOfView;
        return _targetZoom;
    }

    void ApplyZoomImmediate(float zoomValue)
    {
        ZoomMode effectiveZoomMode = ResolveZoomMode();
        if (effectiveZoomMode == ZoomMode.OrthographicSize && _camera.orthographic)
        {
            _camera.orthographicSize = zoomValue;
            return;
        }

        if (effectiveZoomMode == ZoomMode.FieldOfView && !_camera.orthographic)
        {
            _camera.fieldOfView = zoomValue;
            return;
        }
    }
}
