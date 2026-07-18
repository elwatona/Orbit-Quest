using System;
using UnityEngine;

[Serializable]
public struct PresetData
{
    public LimitsData Limits;
    public AstroPresetEntry[] AstroPresetEntries;
    public Vector3 PlayerSpawnPoint;
    public PresetData(LimitsData limits, AstroPresetEntry[] astroPresetEntries, Vector3 playerSpawnPoint)
    {
        Limits = limits;
        AstroPresetEntries = astroPresetEntries;
        PlayerSpawnPoint = playerSpawnPoint;
    }
}

[Serializable]
public struct AstroPresetEntry
{
    public Vector3 Position;
    public EditableData EditableData;
}