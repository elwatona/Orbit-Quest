using UnityEngine;

// [ExecuteAlways]
[DisallowMultipleComponent]
public class GridController : MonoBehaviour
{
    const int FrustumCornerCount = 8;
    static readonly int[] FrustumNearLoop = { 0, 1, 3, 2 };
    static readonly int[] FrustumFarLoop = { 4, 5, 7, 6 };

    [Header("References")]
    [SerializeField] UnityEngine.Camera _camera;
    [SerializeField] LevelData _levelData;

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
    private readonly Vector3[] _frustumCorners = new Vector3[FrustumCornerCount];
    private float GetOpacity(GameState state)
    {
        return state switch
        {
            GameState.Edition => 1f,
            GameState.Contemplative => 0f,
            _ => 0.125f
        };
    }
    
    void Awake()
    {
        _transform = transform;
        _renderer = GetComponent<Renderer>();
        CacheGridFrame();
        _shaderController = new GridShaderController(_renderer);
        if (_camera == null) _camera = UnityEngine.Camera.main;
    }
    void OnEnable()
    {
        _levelData.StateEntered += HandleStateEntered;
    }
    void OnDisable()
    {
        _levelData.StateEntered -= HandleStateEntered;
    }
    void LateUpdate()
    {
        if (_camera == null) return;
        FitToCameraView();
    }

    private void HandleStateEntered(GameState state)
    {
        _shaderController.UpdateOpacity(GetOpacity(state));
    }
    private void CacheGridFrame()
    {
        _planeOrigin = _transform.position;
        _planeRight = _transform.right;
        _planeUp = _transform.up;
        _gridPlane = new Plane(-_transform.forward, _planeOrigin);
    }
    private void FitToCameraView()
    {
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        bool anyHit = false;

        FillFrustumWorldCorners(_camera, _frustumCorners);

        // Frustum ∩ plane: every polygon vertex lies on a frustum edge. Corner rays alone
        // miss edge hits when the near/far clip rectangles cut the plane (common when
        // the camera is close and tilted relative to the grid).
        for (int e = 0; e < 12; e++)
        {
            GetFrustumEdge(_frustumCorners, e, out Vector3 a, out Vector3 b);
            if (!TrySegmentPlaneIntersection(a, b, _gridPlane, out Vector3 worldHit))
                continue;

            ExpandPlaneBounds(worldHit, ref min, ref max);
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

    static void FillFrustumWorldCorners(UnityEngine.Camera camera, Vector3[] corners)
    {
        float nearZ = camera.nearClipPlane;
        float farZ = camera.farClipPlane;
        for (int i = 0; i < 4; i++)
        {
            float sx = (i & 1) == 0 ? 0f : 1f;
            float sy = (i & 2) == 0 ? 0f : 1f;
            corners[i] = camera.ViewportToWorldPoint(new Vector3(sx, sy, nearZ));
            corners[i + 4] = camera.ViewportToWorldPoint(new Vector3(sx, sy, farZ));
        }
    }

    static void GetFrustumEdge(Vector3[] corners, int edgeIndex, out Vector3 a, out Vector3 b)
    {
        if (edgeIndex < 4)
        {
            int i = FrustumNearLoop[edgeIndex];
            int j = FrustumNearLoop[(edgeIndex + 1) & 3];
            a = corners[i];
            b = corners[j];
            return;
        }

        edgeIndex -= 4;
        if (edgeIndex < 4)
        {
            int i = FrustumFarLoop[edgeIndex];
            int j = FrustumFarLoop[(edgeIndex + 1) & 3];
            a = corners[i];
            b = corners[j];
            return;
        }

        edgeIndex -= 4;
        a = corners[edgeIndex];
        b = corners[edgeIndex + 4];
    }

    static bool TrySegmentPlaneIntersection(Vector3 segmentStart, Vector3 segmentEnd, Plane plane, out Vector3 hit)
    {
        hit = default;
        Vector3 dir = segmentEnd - segmentStart;
        float length = dir.magnitude;
        if (length < 1e-8f)
            return false;

        Vector3 unitDir = dir / length;
        Ray ray = new Ray(segmentStart, unitDir);
        if (!plane.Raycast(ray, out float distance))
            return false;

        const float epsilon = 1e-4f;
        if (distance < -epsilon || distance > length + epsilon)
            return false;

        hit = ray.GetPoint(distance);
        return true;
    }

    private void ExpandPlaneBounds(Vector3 worldHit, ref Vector2 min, ref Vector2 max)
    {
        Vector3 offset = worldHit - _planeOrigin;
        float u = Vector3.Dot(offset, _planeRight);
        float v = Vector3.Dot(offset, _planeUp);

        if (u < min.x) min.x = u;
        if (v < min.y) min.y = v;
        if (u > max.x) max.x = u;
        if (v > max.y) max.y = v;
    }
}
