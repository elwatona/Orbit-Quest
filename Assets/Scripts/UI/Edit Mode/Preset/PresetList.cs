using System;
using UnityEngine;

[Serializable]
public class PresetList
{
    [SerializeField] Transform _presetListContent;
    [SerializeField] Preset _presetPrefab;
    public void LoadPresets(string[] presetNames)
    {
        if(_presetListContent.childCount > 0)
        {
            foreach (Transform child in _presetListContent)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        
        foreach (string presetName in presetNames)
        {
            Preset preset = GameObject.Instantiate(_presetPrefab);
            preset.OnCreate(presetName);
        }
    }
}