using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PresetNameComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _presetName;
    [SerializeField] Image _background;
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
            _background.color = Color.black;
        }
        else
        {
            _background.color = Color.gray;
        }
    }
}