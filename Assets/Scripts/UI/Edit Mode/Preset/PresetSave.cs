using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[Serializable]
public class PresetSave
{
    [SerializeField] GameObject _rootPresetSave;
    [SerializeField] TMP_InputField _desiredName;
    [SerializeField] TMP_Text _placeholderText;
    [SerializeField] Image _background;

    public void SetPlaceholderText(string text)
    {
        _placeholderText.text = text;
    }
    public IEnumerator LerpBackgroundColor(Color color)
    {
        float time = 0;
        Color startColor = _background.color;
        while(time < 1)
        {
            time += Time.deltaTime;
            _background.color = Color.Lerp(color, startColor, time);
            yield return null;
        }
        _background.color = startColor;
    }
    public enum SavePresetResult
    {
        Success,
        Failed,
        NameAlreadyExists
    }
    public SavePresetResult TrySavePreset()
    {
        if(string.IsNullOrEmpty(_desiredName.text)) return SavePresetResult.Failed;
        else if(FileManager.PresetNameExists(_desiredName.text)) return SavePresetResult.NameAlreadyExists;
        return SavePresetResult.Success;
    }
    public void SetActive(bool active)
    {
        _rootPresetSave.SetActive(active);
    }
}
