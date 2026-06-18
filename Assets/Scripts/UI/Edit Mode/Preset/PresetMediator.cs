using System;
using UnityEngine;

public class PresetManager : MonoBehaviour
{
    [SerializeField] PresetList _presetList;
    [SerializeField] PresetSave _presetSave;
    private Preset _selectedPreset;
    void OnEnable()
    {
        PresetEvents.OnPresetSelected += HandlePresetSelected;
        PresetEvents.OnPresetNamesLoaded += _presetList.LoadPresets;
    }
    void Start()
    {
        FileManager.LoadPresetNames();
    }
    void OnDisable()
    {
        PresetEvents.OnPresetSelected -= HandlePresetSelected;
        PresetEvents.OnPresetNamesLoaded -= _presetList.LoadPresets;
    }
    void HandlePresetSelected(Preset preset)
    {
        _selectedPreset = preset;
    }
    public void LoadSelectedPreset()
    {
        //Cargar los valores del preset seleccionado a partir de un JSON cuyo nombre es el nombre del preset
    }
    public void TrySavePreset()
    {
        switch(_presetSave.TrySavePreset())
        {
            case PresetSave.SavePresetResult.Success:
                // Leer los valores de preset a partir de un Manager en escena.
                _presetSave.SetActive(false);
                break;
            case PresetSave.SavePresetResult.Failed:
                _presetSave.SetPlaceholderText("Name is required");
                StartCoroutine(_presetSave.LerpBackgroundColor(Color.yellow));
                break;
            case PresetSave.SavePresetResult.NameAlreadyExists:
                _presetSave.SetPlaceholderText("Name already exists");
                StartCoroutine(_presetSave.LerpBackgroundColor(Color.red));
                break;
        }
    }
}   