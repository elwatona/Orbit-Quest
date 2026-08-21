using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class FreelookPosition : MonoBehaviour
{
    [SerializeField] LevelData _levelData;
    [SerializeField] Transform _target;
    [SerializeField] float _smoothTime = 0.12f;
    [SerializeField] float _moveSpeed = 1f;

    Transform _transform;
    Vector3 _clickPoint;
    Vector3 _velocity;
    Vector2 _moveInput;
    bool _isEdition;

    void Awake()
    {
        _transform = transform;
        _clickPoint = _transform.position;
        _clickPoint.y = 0f;
    }

    void OnEnable()
    {
        _levelData.StateEntered += OnStateEntered;
        CameraInputController.MoveInput += OnMoveInput;
        _isEdition = _levelData.CurrentState == GameState.Edition;
        if (_isEdition)
            AlignToTarget();
    }

    void OnDisable()
    {
        _levelData.StateEntered -= OnStateEntered;
        CameraInputController.MoveInput -= OnMoveInput;
    }

    void OnMoveInput(Vector2 value)
    {
        _moveInput = value;
    }

    void Update()
    {
        if (!_isEdition) return;

        ApplyKeyboardMove();
        if (!CanReadWorldClick()) return;
        TryProjectClickToPlane();
    }

    void ApplyKeyboardMove()
    {
        if (_moveInput.sqrMagnitude < 0.0001f) return;

        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (cam == null) return;

        Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        Vector3 delta = camRight * _moveInput.x + camForward * _moveInput.y;
        delta = Vector3.ClampMagnitude(delta, 1f);

        _clickPoint += delta * _moveSpeed * cam.orthographicSize * Time.deltaTime;
        _clickPoint.y = 0f;
        _transform.position = _clickPoint;
        _velocity = Vector3.zero;
    }

    bool CanReadWorldClick()
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
        if (!_isEdition) return;
        if (_moveInput.sqrMagnitude > 0.0001f) return;

        _transform.position = Vector3.SmoothDamp(
            _transform.position,
            _clickPoint,
            ref _velocity,
            _smoothTime);
    }

    void OnStateEntered(GameState state)
    {
        _isEdition = state == GameState.Edition;
        if (!_isEdition) return;

        AlignToTarget();
    }

    void AlignToTarget()
    {
        if (_target == null) return;

        Vector3 point = _target.position;
        point.y = 0f;
        _clickPoint = point;
        _transform.position = point;
        _velocity = Vector3.zero;
    }

    void TryProjectClickToPlane()
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
        _velocity = Vector3.zero;
    }
}
