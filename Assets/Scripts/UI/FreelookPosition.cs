using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FreelookPosition : MonoBehaviour
{
    [SerializeField] LevelData _levelData;
    [SerializeField] Transform _target;
    [SerializeField] float _smoothTime = 2f;

    private Transform _transform;
    private bool _followTarget;
    private Vector3 _clickPoint;
    private Vector3 _velocity;

    void Awake()
    {
        _transform = transform;
        _clickPoint = _transform.position;
        _clickPoint.y = 0f;
    }

    void OnEnable()
    {
        _levelData.StateEntered += OnStateEntered;
        _levelData.StateExited += OnStateExited;
        _followTarget = _levelData.CurrentState == GameState.Precision;
    }

    void OnDisable()
    {
        _levelData.StateEntered -= OnStateEntered;
        _levelData.StateExited -= OnStateExited;
    }

    void Update()
    {
        if (_followTarget) return;
        if (!CanReadWorldClick()) return;

        TryProjectClickToPlane();
    }

    private bool CanReadWorldClick()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return false;
        if (!Application.isFocused) return false;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        if (screenPos.x < 0f || screenPos.x > Screen.width ||
            screenPos.y < 0f || screenPos.y > Screen.height)
            return false;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return false;

        return true;
    }

    void LateUpdate()
    {
        Vector3 destination = _followTarget && _target
            ? _target.position
            : _clickPoint;

        _transform.position = Vector3.SmoothDamp(
            _transform.position,
            destination,
            ref _velocity,
            _smoothTime);
    }

    private void OnStateEntered(GameState state)
    {
        if (state == GameState.Precision)
        {
            _followTarget = true;
            _transform.position = _target.position;
        }
    }

    private void OnStateExited(GameState state)
    {
        if (state != GameState.Precision) return;

        _followTarget = false;
        _clickPoint = _transform.position;
        _clickPoint.y = 0f;
    }

    private void TryProjectClickToPlane()
    {
        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Mathf.Abs(ray.direction.y) < 0.0001f) return;

        float t = -ray.origin.y / ray.direction.y;
        if (t < 0f) return;

        Vector3 point = ray.origin + ray.direction * t;
        point.y = 0f;
        _clickPoint = point;
    }
}
