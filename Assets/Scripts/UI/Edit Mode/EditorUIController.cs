using UnityEngine;
using System.Collections.Generic;

public class EditorUIController : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    [SerializeField] LevelDataDependencies _levelDataDependencies;
    [SerializeField] PresetListDependencies _presetListDependencies;
    [SerializeField] PresetSaveDependencies _presetSaveDependencies;
    [SerializeField] PlayerEditableDataDependencies _playerEditableDataDependencies;
    [SerializeField] AstroMovementDependencies _astroMovementDependencies;
    [SerializeField] AstroEditableDataDependencies _astroEditableDataDependencies;
    [SerializeField] AstroSelectorDependencies _astroSelectorDependencies;

    private LevelData _levelData;
    private PresetList _presetList;
    private PresetSave _presetSave;
    private PlayerEditableData _playerEditableData;
    private AstroMovement _astroMovement;
    private AstroEditableData _astroEditableData;
    private AstroSelector _astroSelector;

    private IPanel[] _panels;
    private IEditable _selectedEditable;
    private bool _isSelectingTargets;

    void Awake()
    {
        _levelData = new LevelData(_levelDataDependencies);
        _presetList = new PresetList(_presetListDependencies);
        _presetSave = new PresetSave(_presetSaveDependencies);
        _playerEditableData = new PlayerEditableData(_playerEditableDataDependencies);
        _astroMovement = new AstroMovement(_astroMovementDependencies);
        _astroEditableData = new AstroEditableData(_astroEditableDataDependencies);
        _astroSelector = new AstroSelector(_astroSelectorDependencies);

        _panels = new IPanel[] { _levelData, _playerEditableData, _astroMovement, _astroEditableData, _astroSelector };
    }
    void OnEnable()
    {
        Astro.OnEditableClicked += HandleAstroClicked;
        PresetEvents.OnPresetNamesLoaded += _presetList.LoadPresets;
        PresetEvents.OnPresetSelected += _presetList.HandlePresetSelected;

        ToggleSelectingTargets(false);
        _playerEditableData.ConnectPlayerDataToUI();
    }
    void OnDisable()
    {
        Astro.OnEditableClicked -= HandleAstroClicked;
        PresetEvents.OnPresetNamesLoaded -= _presetList.LoadPresets;
        PresetEvents.OnPresetSelected -= _presetList.HandlePresetSelected;
    }

    private void HandleAstroClicked(IEditable editable)
    {
        if(!_playerData.IsInEditMode) return;
        if(!_isSelectingTargets) AstroSelected(editable);
        else TargetClicked(editable);
    }
    private void AstroSelected(IEditable editable)
    {
        _astroMovement.InyectEditable(editable, _selectedEditable);
        _astroEditableData.InyectEditable(editable, _selectedEditable);
        _selectedEditable = editable;

        _astroMovement.Toggle(editable.HasOrbiter());
        _astroEditableData.Toggle(true);
    }
    private void TargetClicked(IEditable editable)
    {
        if(editable == _selectedEditable) return;
        _astroSelector.AstroTargeted(editable);
    }

    private void ToggleSelectingTargets(bool active)
    {
        _isSelectingTargets = active;

        foreach(IPanel panel in _panels) 
        {
            switch(panel)
            {
                case AstroSelector:
                    panel.Toggle(_isSelectingTargets);
                    break;
                case AstroEditableData:
                    panel.Toggle(!_isSelectingTargets && _selectedEditable != null);
                    break;
                case AstroMovement:
                    panel.Toggle(!_isSelectingTargets && _selectedEditable != null && _selectedEditable.HasOrbiter());
                    break;
                default:
                    panel.Toggle(!_isSelectingTargets);
                    break;
            }
        }
    }
    public void StartSelectingTargets()
    {
        ToggleSelectingTargets(true);

        _astroSelector.StartSelectingTargets(_astroMovement.TargetsToOrbitList);
        _selectedEditable.Deselected();
    }
    public void StopSelectingTargets()
    {
        ToggleSelectingTargets(false);
        _selectedEditable.Selected();

        _selectedEditable.UpdateOrbiterTargets(_astroSelector.SelectedAstros.ToArray());
        _astroMovement.LoadTargetsToOrbitDataToUI(_selectedEditable);
        _astroSelector.StopSelectingTargets();
    }
    public void CancelSelectingTargets()
    {
        ToggleSelectingTargets(false);
        _selectedEditable.Selected();

        _astroSelector.StopSelectingTargets();
    }
    public void ClearTargetsToOrbit()
    {
        _astroMovement.ClearTargetsToOrbit();
        _selectedEditable.UpdateOrbiterTargets(new IEditable[0]);
    }
    public void LoadSelectedPreset()
    {
        PresetFileManager.Read(_presetList.SelectedPreset.GetPresetName());
        _presetList.Toggle(false);
    }
    public void TrySavePreset(bool shouldUpdate)
    {
        switch(_presetSave.TryToSave(out string presetName, shouldUpdate))
        {
            case PresetSave.Result.Success:
                _presetSave.SetDebugText(string.Empty);
                PresetEvents.RaisePresetSavedEvent(presetName);
                _presetSave.Toggle(false);
                PresetFileManager.LoadNames();
                break;
            case PresetSave.Result.Failed:
                _presetSave.SetDebugText("Name is required");
                StartCoroutine(_presetSave.LerpAlertColor(Color.yellow));
                break;
            case PresetSave.Result.NameAlreadyExists:
                _presetSave.SetDebugText("Name already exists");
                StartCoroutine(_presetSave.LerpAlertColor(Color.red));
                break;
        }
    }
    public void DeleteSelectedPreset()
    {
        PresetFileManager.Delete(_presetList.SelectedPreset.GetPresetName());
        PresetEvents.RaisePresetSelectedEvent(null);
    }
    public void ClosePresetList()
    {
        PresetEvents.RaisePresetSelectedEvent(null);
        _presetList.Toggle(false);
    }
    public void OpenPresetList()
    {
        PresetFileManager.LoadNames();
        _presetList.Toggle(true);
    }
    public void CreateNewPreset()
    {
        _presetList.Toggle(false);
        _presetSave.SetDebugText(string.Empty);
        _presetSave.Toggle(true);
    }
    public void CancelPresetCreation()
    {
        _presetList.Toggle(true);
        _presetSave.Toggle(false);
    }
}