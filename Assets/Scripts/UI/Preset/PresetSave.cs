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
    [SerializeField] TMP_Text _debugText;
    [SerializeField] Image _background;
    private Color bgColor;
    private Color txtColor;

    public void Start()
    {
        bgColor = _background.color;
        txtColor = _debugText.color;
    }
    public void SetDebugText(string text)
    {
        _debugText.text = text;
    }
    public IEnumerator LerpAlertColor(Color targetColor)
    {
        float time = 0;
        while(time < 1)
        {
            time += Time.deltaTime;
            _background.color = Color.Lerp(targetColor, bgColor, time);
            _debugText.color = Color.Lerp(targetColor, txtColor, time);
            yield return null;
        }
        _background.color = bgColor;
        _debugText.color = txtColor;
    }
    public enum Result
    {
        Success,
        Failed,
        NameAlreadyExists
    }
    public Result TryToSave(out string presetName, bool shouldUpdate = false)
    {
        presetName = string.Empty;
        if(string.IsNullOrEmpty(_desiredName.text)) return Result.Failed;
        else if(PresetFileManager.NameExists(_desiredName.text) && !shouldUpdate) return Result.NameAlreadyExists;
        presetName = _desiredName.text;
        return Result.Success;
    }
    public void SetActive(bool active)
    {
        _rootPresetSave.SetActive(active);
    }
}
