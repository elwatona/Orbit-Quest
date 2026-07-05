using System;
using UnityEngine;

public class PresetManager : MonoBehaviour
{
    [SerializeField] PresetList _presetList;
    [SerializeField] PresetSave _presetSave;
    [SerializeField] PresetLimits _presetLimits;
    private PresetNameComponent _selectedPreset;
    void OnEnable()
    {
        PresetEvents.OnPresetSelected += HandlePresetSelected;
        PresetEvents.OnPresetNamesLoaded += _presetList.LoadPresets;
    }
    void Start()
    {
        PresetFileManager.LoadNames();
        _presetSave.Start();
        _presetLimits.Start();
    }
    void OnDisable()
    {
        PresetEvents.OnPresetSelected -= HandlePresetSelected;
        PresetEvents.OnPresetNamesLoaded -= _presetList.LoadPresets;
    }
    void HandlePresetSelected(PresetNameComponent preset)
    {
        _selectedPreset = preset;
    }
    public void LoadSelectedPreset()
    {
        //Cargar los valores del preset seleccionado a partir de un JSON cuyo nombre es el nombre del preset
        PresetFileManager.Read(_selectedPreset.GetPresetName());
    }
    public void TrySavePreset(bool shouldUpdate)
    {
        switch(_presetSave.TryToSave(out string presetName, shouldUpdate))
        {
            case PresetSave.Result.Success:
                _presetSave.SetDebugText(string.Empty);
                PresetEvents.RaisePresetSavedEvent(presetName);
                _presetSave.SetActive(false);
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
    public void OnDelete()
    {
        PresetFileManager.Delete(_selectedPreset.GetPresetName());
        PresetEvents.RaisePresetSelectedEvent(null);
    }
    public void OnClose()
    {
        PresetEvents.RaisePresetSelectedEvent(null);
    }
}   