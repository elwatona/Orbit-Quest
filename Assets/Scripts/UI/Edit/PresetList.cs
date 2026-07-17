using System;
using UnityEngine;
using System.Collections.Generic;

public class PresetList : IPanel
{
    public GameObject Root { get; private set; }
    public PresetNameComponent SelectedPreset { get; private set; }
    readonly public Transform PresetListContent;
    readonly public PresetNameComponent PresetPrefab;
    readonly public List<GameObject> PresetNameComponents = new List<GameObject>();
    public PresetList(PresetListDependencies dependencies)
    {
        Root = dependencies.Root;
        PresetListContent = dependencies.PresetListContent;
        PresetPrefab = dependencies.PresetPrefab;
    }
    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
    public void LoadPresets(string[] presetNames)
    {
        if(PresetNameComponents.Count > 0)
        {
            foreach (GameObject presetNameComponent in PresetNameComponents)
            {
                GameObject.Destroy(presetNameComponent);
            }
            PresetNameComponents.Clear();
        }
        
        foreach (string presetName in presetNames)
        {
            PresetNameComponent preset = GameObject.Instantiate(PresetPrefab, PresetListContent);
            preset.OnCreate(presetName);
            PresetNameComponents.Add(preset.gameObject);
        }
        Debug.Log("Preset names loaded: " + PresetNameComponents.Count);
    }
    public void HandlePresetSelected(PresetNameComponent preset)
    {
        SelectedPreset = preset;
    }
}

[Serializable]
public class PresetListDependencies : PanelDependencies
{
    public Transform PresetListContent;
    public PresetNameComponent PresetPrefab;
}