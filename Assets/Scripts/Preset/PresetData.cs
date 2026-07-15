using System;
using UnityEngine;

[Serializable]
public struct PresetData
{
    public LimitsData Limits;
    public AstroPresetEntry[] AstroPresetEntries;
    public PresetData(LimitsData limits, AstroPresetEntry[] astroPresetEntries)
    {
        Limits = limits;
        AstroPresetEntries = astroPresetEntries;
    }
}

[Serializable]
public struct AstroPresetEntry
{
    public Vector3 Position;
    public EditableData EditableData;
}