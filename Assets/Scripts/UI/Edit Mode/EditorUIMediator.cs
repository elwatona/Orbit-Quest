using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EditorUIMediator : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    [SerializeField] PanelController[] _panelControllers;
    [SerializeField] ScrollComponent _targetsToOrbitScrollComponent;
    [SerializeField] GameObject _rootTargetsToOrbitButtons, _controlEditData, _controlGameplayData;
    private EditorPanelsMediator _editorPanelsMediator;
    private List<IEditable> _selectedAstros = new List<IEditable>();
    private bool _isSelectingTargets;
    void Awake()
    {
        _editorPanelsMediator = new EditorPanelsMediator(_panelControllers, _playerData, _targetsToOrbitScrollComponent);
    }
    void OnEnable()
    {
        Astro.OnEditableClicked += HandleAstroClicked;
        UIEventHandler.UIPanelEvent += HandlePanelToggled;
        _editorPanelsMediator.ConnectPlayerDataToUI();
        _playerData.IsInEditModeUpdated += HandleEditModeToggled;
    }
    void Start()
    {
        _rootTargetsToOrbitButtons.SetActive(false);
        _editorPanelsMediator.TogglePanel(0,false);
        _editorPanelsMediator.TogglePanel(1,false);
        _editorPanelsMediator.TogglePanel(2,false);
        HandleEditModeToggled();
    }
    void OnDisable()
    {
        _playerData.IsInEditModeUpdated -= HandleEditModeToggled;
        _editorPanelsMediator.DisconnectPlayerDataFromUI();
        Astro.OnEditableClicked -= HandleAstroClicked;
        UIEventHandler.UIPanelEvent -= HandlePanelToggled;
    }
    void HandleEditModeToggled()
    {
        _controlEditData.SetActive(_playerData.IsInEditMode);
        _controlGameplayData.SetActive(!_playerData.IsInEditMode);
        _editorPanelsMediator.TogglePanel(0, _playerData.IsInEditMode);
        if(!_playerData.IsInEditMode) ClosePanel();
    }
    void HandlePanelToggled(PanelEnum panel)
    {
        if(_isSelectingTargets) return;
        if(panel == PanelEnum.PlayerData) _editorPanelsMediator.TogglePanel(0);
    }
    void HandleAstroClicked(IEditable editable)
    {
        if(!_playerData.IsInEditMode) return;
        if(!_isSelectingTargets) AstroSelected(editable);
        else TargetClicked(editable);
    }
    void AstroSelected(IEditable editable)
    {
        _selectedAstros?.Clear();
        _editorPanelsMediator.InyectEditable(editable);
    }
    void TargetClicked(IEditable editable)
    {
        if(editable == _editorPanelsMediator.SelectedEditable) return;
        if(_selectedAstros.Contains(editable))
        {
            _selectedAstros.Remove(editable);
            editable.Deselected();
        }
        else 
        {
            _selectedAstros.Add(editable);
            editable.Selected();
        }
    }
    public void StartSelectingTargets()
    {
        _isSelectingTargets = true;
        _editorPanelsMediator.SelectedEditable.Deselected();
        _editorPanelsMediator.TogglePanels(!_isSelectingTargets);
        foreach(IEditable target in _editorPanelsMediator.TargetsToOrbit)
        {
            _selectedAstros.Add(target);
            target.Selected();
        }
        _rootTargetsToOrbitButtons.SetActive(_isSelectingTargets);
    }
    public void StopSelectingTargets()
    {
        _isSelectingTargets = false;
        _editorPanelsMediator.UpdateTargetsToOrbit(_selectedAstros.ToArray());
        foreach(IEditable editable in _selectedAstros) editable.Deselected();
        _selectedAstros.Clear();
        _editorPanelsMediator.SelectedEditable.Selected();
        _editorPanelsMediator.TogglePanels(!_isSelectingTargets);
        _rootTargetsToOrbitButtons.SetActive(_isSelectingTargets);
    }
    public void CancelSelectingTargets()
    {
        foreach(IEditable editable in _selectedAstros) editable.Deselected();
        _isSelectingTargets = false;
        _editorPanelsMediator.TogglePanels(!_isSelectingTargets);
        _selectedAstros.Clear();
    }
    public void ClearTargetsToOrbit()
    {
        _editorPanelsMediator.ClearTargetsToOrbit();
    }
    public void DeleteAstro()
    {
        _editorPanelsMediator.SelectedEditable.Deactivate();
        ClosePanel();
    }
    public void ClosePanel()
    {
        _editorPanelsMediator.SelectedEditable?.Deselected();
        _editorPanelsMediator.TogglePanel(1, false);
        _editorPanelsMediator.TogglePanel(2, false);
    }
}