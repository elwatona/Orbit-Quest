using System.IO;
using UnityEngine;
using System.Linq;

public static class FileManager
{
    public static void LoadPresetNames()
    {
        Debug.Log("Loading preset names from: " + Application.persistentDataPath);
        string[] presetNames = Directory.GetFiles(Application.persistentDataPath, "*.json").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
        Debug.Log("Preset names: " + string.Join(", ", presetNames));
        PresetEvents.RaisePresetNamesLoadedEvent(presetNames);
    }
    public static void LoadPreset(string presetName)
    {
        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, presetName + ".json"));
        PresetData presetData = JsonUtility.FromJson<PresetData>(json);
        PresetEvents.RaisePresetLoadedEvent(presetData);
    }
    public static void SavePreset(string presetName, PresetData presetData)
    {
        string json = JsonUtility.ToJson(presetData);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, presetName + ".json"), json);
    }
    public static bool PresetNameExists(string presetName)
    {
        return File.Exists(Path.Combine(Application.persistentDataPath, presetName + ".json"));
    }
}
