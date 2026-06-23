using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PresetNameComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _presetName;
    public string GetPresetName() => _presetName.text;
    void OnEnable()
    {
        PresetEvents.OnPresetSelected += HandlePresetSelected;
    }
    void OnDisable()
    {
        PresetEvents.OnPresetSelected -= HandlePresetSelected;
    }
    public void OnSelect()
    {
        PresetEvents.RaisePresetSelectedEvent(this);
    }
    public void OnCreate(string presetName)
    {
        _presetName.text = presetName;
    }
    void HandlePresetSelected(PresetNameComponent preset)
    {
        if (preset == this)
        {
            _presetName.fontStyle = FontStyles.Strikethrough;
        }
        else
        {
            _presetName.fontStyle = FontStyles.Normal;
        }
    }
}