using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _inspectorPanel;
    [SerializeField] Transform _propertyContainer;
    [SerializeField] GameObject _orbiterPanel;
    [SerializeField] Transform _orbiterPropertyContainer;
    [SerializeField] OrbiterTargetsView _orbiterTargetsView;
    [SerializeField] GameObject _cancelPickingButton;
    [SerializeField] GameObject _controlsPanel;
    [SerializeField] GameObject _debugPanel;
    [SerializeField] TextMeshProUGUI _debugText;
    [SerializeField] PropertyRow _rowPrefab;
    [SerializeField] GroupHeaderView _groupHeaderPrefab;
    [SerializeField] TextMeshProUGUI _version;
    [SerializeField] TextMeshProUGUI _developerModeLabel;

    private PanelController _panelController;
    private PanelController _orbiterPanelController;
    private IEditable _currentTarget;
    private bool _isPickingTargetForOrbiter;
    private bool _developerMode;

    public bool IsDeveloperMode => _developerMode;

    void Awake()
    {
        _panelController = new PanelController(_propertyContainer, _rowPrefab, _groupHeaderPrefab);
        _orbiterPanelController = new PanelController(_orbiterPropertyContainer, _rowPrefab, _groupHeaderPrefab);
        Astro.OnAstroClicked += SelectTarget;
        Orb.OnDebugUpdate += UpdateDebug;

        if (_version)
            _version.text = $"Version {Application.version} \n Unity {Application.unityVersion}";
    }
    void OnDestroy()
    {
        Astro.OnAstroClicked -= SelectTarget;
        Orb.OnDebugUpdate -= UpdateDebug;
    }
    void Start()
    {
        _inspectorPanel.SetActive(false);
        if (_orbiterPanel != null)
            _orbiterPanel.SetActive(false);
        if (_cancelPickingButton != null)
            _cancelPickingButton.SetActive(false);
        UpdateDeveloperModeLabel();
    }

    /// <summary>Sets developer mode from PlayerController. Closes panels and updates label when disabled.</summary>
    public void SetDeveloperMode(bool value)
    {
        _developerMode = value;
        if (!_developerMode)
        {
            ClosePanel();
            if (_controlsPanel != null) _controlsPanel.SetActive(false);
            if (_debugPanel != null) _debugPanel.SetActive(false);
        }
        UpdateDeveloperModeLabel();
    }

    void UpdateDeveloperModeLabel()
    {
        if (_developerModeLabel == null) return;
        _developerModeLabel.text = _developerMode ? "Developer Mode On" : "Developer Mode Off";
        _developerModeLabel.color = _developerMode ? Color.green : Color.white;
    }

    public void SelectTarget(IEditable target)
    {
        if (!_developerMode) return;
        if (_isPickingTargetForOrbiter)
        {
            var orbiter = (_currentTarget as Component)?.GetComponent<TransformOrbiter>();
            if (orbiter != null && target is Component comp)
            {
                orbiter.AddTarget(comp.transform);
                _orbiterTargetsView?.Refresh();
            }
            _isPickingTargetForOrbiter = false;
            SetInspectorPanelsActive(true);
            HideCancelPickingButton();
            return;
        }

        _currentTarget?.Deselected();

        if (!_inspectorPanel.activeSelf)
            _inspectorPanel.SetActive(true);

        _currentTarget = target;
        _currentTarget.Selected();
        _panelController.Bind(target);

        var targetOrbiter = (target as Component)?.GetComponent<TransformOrbiter>();
        if (targetOrbiter != null)
        {
            _orbiterPanelController.Bind(targetOrbiter.GetProperties());
            _orbiterTargetsView?.Bind(targetOrbiter);
            if (_orbiterPanel != null)
                _orbiterPanel.SetActive(true);
        }
        else
        {
            _orbiterPanelController.Clear();
            _orbiterTargetsView?.Clear();
            if (_orbiterPanel != null)
                _orbiterPanel.SetActive(false);
        }
    }

    public void StartPickingTargetForOrbiter()
    {
        var orbiter = (_currentTarget as Component)?.GetComponent<TransformOrbiter>();
        _isPickingTargetForOrbiter = orbiter != null;
        if (_isPickingTargetForOrbiter)
        {
            SetInspectorPanelsActive(false);
            if (_cancelPickingButton != null)
                _cancelPickingButton.SetActive(true);
        }
    }

    public void CancelPickingTargetForOrbiter()
    {
        _isPickingTargetForOrbiter = false;
        SetInspectorPanelsActive(true);
        HideCancelPickingButton();
    }

    void HideCancelPickingButton()
    {
        if (_cancelPickingButton != null)
            _cancelPickingButton.SetActive(false);
    }

    void SetInspectorPanelsActive(bool active)
    {
        if (_inspectorPanel != null)
            _inspectorPanel.SetActive(active);
        if (_orbiterPanel != null)
            _orbiterPanel.SetActive(active);
    }

    public void ClosePanel()
    {
        _isPickingTargetForOrbiter = false;
        HideCancelPickingButton();
        _currentTarget?.Deselected();
        _panelController.Clear();
        _orbiterPanelController.Clear();
        _orbiterTargetsView?.Clear();
        _inspectorPanel.SetActive(false);
        if (_orbiterPanel != null)
            _orbiterPanel.SetActive(false);
        _currentTarget = null;
    }
    public void TogglePanel(int index)
    {
        switch (index)
        {
            case 0: _controlsPanel.SetActive(!_controlsPanel.activeSelf); break;
            case 1: _debugPanel.SetActive(!_debugPanel.activeSelf); break;
        };
    }

    void UpdateDebug(float speed)
    {
        if (!_debugPanel.activeSelf)
            return;

        _debugText.text = $"Speed: {speed:0.##}";
    }

    public void DeleteTarget()
    {
        _isPickingTargetForOrbiter = false;
        HideCancelPickingButton();
        _panelController.Clear();
        _orbiterPanelController.Clear();
        _orbiterTargetsView?.Clear();
        _inspectorPanel.SetActive(false);
        if (_orbiterPanel != null)
            _orbiterPanel.SetActive(false);
        _currentTarget?.Deselected();
        _currentTarget?.Deactivate();
        _currentTarget = null;
    }
}
