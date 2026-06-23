using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class PresetList
{
    [SerializeField] Transform _presetListContent;
    [SerializeField] PresetNameComponent _presetPrefab;
    private List<GameObject> _presetNameComponents = new List<GameObject>();
    public void LoadPresets(string[] presetNames)
    {
        if(_presetNameComponents.Count > 0)
        {
            foreach (GameObject presetNameComponent in _presetNameComponents)
            {
                GameObject.Destroy(presetNameComponent);
            }
            _presetNameComponents.Clear();
        }
        
        foreach (string presetName in presetNames)
        {
            PresetNameComponent preset = GameObject.Instantiate(_presetPrefab, _presetListContent);
            preset.OnCreate(presetName);
            _presetNameComponents.Add(preset.gameObject);
        }
        Debug.Log("Preset names loaded: " + _presetNameComponents.Count);
    }
}