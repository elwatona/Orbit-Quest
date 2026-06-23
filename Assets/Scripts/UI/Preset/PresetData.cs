using System;
using UnityEngine;

[Serializable]
public struct PresetData
{
    public AstroPresetEntry[] AstroPresetEntries;
    public PresetData(AstroPresetEntry[] astroPresetEntries)
    {
        AstroPresetEntries = astroPresetEntries;
    }
}

[Serializable]
public struct AstroPresetEntry
{
    public Vector3 Position;
    public EditableData EditableData;
}