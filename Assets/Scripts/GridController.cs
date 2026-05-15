using UnityEngine;

// [ExecuteAlways]
[DisallowMultipleComponent]
public class GridController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Camera _camera;
    [SerializeField] PlayerData _playerData;

    [Header("Fit Settings")]
    [SerializeField, Min(1f)] float _coverageMargin = 1.1f;
    [SerializeField, Min(1f)] float _maxSize = 2000f;
    [SerializeField, Min(1f)] float _fallbackSize = 100f;

    private Transform _transform;
    private Vector3 _planeOrigin;
    private Vector3 _planeRight;
    private Vector3 _planeUp;
    private Plane _gridPlane;
    private Renderer _renderer;
    private GridShaderController _shaderController;
    void Awake()
    {
        _transform = transform;
        _renderer = GetComponent<Renderer>();
        CacheGridFrame();
        _shaderController = new GridShaderController(_renderer);
        if (_camera == null) _camera = Camera.main;
    }

    void Start()
    {
        HandleEditModeToggled();
    }

    void OnEnable()
    {
        _playerData.IsInEditModeUpdated += HandleEditModeToggled;
    }

    void OnDisable()
    {
        _playerData.IsInEditModeUpdated -= HandleEditModeToggled;
    }

    void HandleEditModeToggled()
    {
        _shaderController.UpdateOpacity(_playerData.IsInEditMode ? 1f : 0.026f);
    }

    void LateUpdate()
    {
        if (_camera == null) return;
        FitToCameraView();
    }

    void CacheGridFrame()
    {
        _planeOrigin = _transform.position;
        _planeRight = _transform.right;
        _planeUp = _transform.up;
        _gridPlane = new Plane(-_transform.forward, _planeOrigin);
    }

    void FitToCameraView()
    {
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        bool anyHit = false;

        for (int i = 0; i < 4; i++)
        {
            Vector3 viewport = new Vector3((i & 1) == 0 ? 0f : 1f, (i & 2) == 0 ? 0f : 1f, 0f);
            Ray ray = _camera.ViewportPointToRay(viewport);

            if (!_gridPlane.Raycast(ray, out float distance) || distance < 0f)
                continue;

            Vector3 worldHit = ray.GetPoint(distance);
            Vector3 offset = worldHit - _planeOrigin;
            float u = Vector3.Dot(offset, _planeRight);
            float v = Vector3.Dot(offset, _planeUp);

            if (u < min.x) min.x = u;
            if (v < min.y) min.y = v;
            if (u > max.x) max.x = u;
            if (v > max.y) max.y = v;
            anyHit = true;
        }

        Vector2 center;
        Vector2 size;

        if (anyHit)
        {
            center = (min + max) * 0.5f;
            size = new Vector2(
                Mathf.Min(_maxSize, (max.x - min.x) * _coverageMargin),
                Mathf.Min(_maxSize, (max.y - min.y) * _coverageMargin));
        }
        else
        {
            Vector3 cameraOffset = _camera.transform.position - _planeOrigin;
            center = new Vector2(
                Vector3.Dot(cameraOffset, _planeRight),
                Vector3.Dot(cameraOffset, _planeUp));
            size = new Vector2(_fallbackSize, _fallbackSize);
        }

        Vector3 worldCenter = _planeOrigin + _planeRight * center.x + _planeUp * center.y;
        _transform.position = worldCenter;
        _transform.localScale = new Vector3(size.x, size.y, 1f);
    }
}
