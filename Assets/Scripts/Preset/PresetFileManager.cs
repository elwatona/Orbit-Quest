using System.IO;
using System.Linq;
using UnityEngine;

public static class PresetFileManager
{
    public static void LoadNames()
    {
        Debug.Log("Loading preset names from: " + Application.persistentDataPath);
        string[] presetNames = Directory.GetFiles(Application.persistentDataPath, "*.json").Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
        Debug.Log("Preset names: " + string.Join(", ", presetNames));
        PresetEvents.RaisePresetNamesLoadedEvent(presetNames);
    }
    public static void Write(string presetName, PresetData presetData)
    {
        string json = JsonUtility.ToJson(presetData);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, presetName + ".json"), json);
    }
    public static void Read(string presetName)
    {
        string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, presetName + ".json"));
        PresetData presetData = JsonUtility.FromJson<PresetData>(json);
        PresetEvents.RaisePresetLoadedEvent(presetData);
    }
    public static void Delete(string presetName)
    {
        File.Delete(Path.Combine(Application.persistentDataPath, presetName + ".json"));
        LoadNames();
    }
    public static bool NameExists(string presetName)
    {
        return File.Exists(Path.Combine(Application.persistentDataPath, presetName + ".json"));
    }
}
