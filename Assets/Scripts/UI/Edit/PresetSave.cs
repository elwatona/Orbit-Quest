using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PresetSave : IPanel
{
    public GameObject Root { get; private set; }
    readonly public TMP_InputField DesiredName;
    readonly public TMP_Text DebugText;
    readonly public Image Background;

    private Color bgColor;
    private Color txtColor;
    
    public enum Result
    {
        Success,
        Failed,
        NameAlreadyExists
    }

    public PresetSave(PresetSaveDependencies dependencies)
    {
        Root = dependencies.Root;
        DesiredName = dependencies.DesiredName;
        DebugText = dependencies.DebugText;
        Background = dependencies.Background;
        
        bgColor = Background.color;
        txtColor = DebugText.color;
    }
    public void Start()
    {
    }
    public void SetDebugText(string text)
    {
        DebugText.text = text;
    }
    public IEnumerator LerpAlertColor(Color targetColor)
    {
        float time = 0;
        while(time < 1)
        {
            time += Time.deltaTime;
            Background.color = Color.Lerp(targetColor, bgColor, time);
            DebugText.color = Color.Lerp(targetColor, txtColor, time);
            yield return null;
        }
        Background.color = bgColor;
        DebugText.color = txtColor;
    }
    public Result TryToSave(out string presetName, bool shouldUpdate = false)
    {
        presetName = string.Empty;
        if(string.IsNullOrEmpty(DesiredName.text)) return Result.Failed;
        else if(PresetFileManager.NameExists(DesiredName.text) && !shouldUpdate) return Result.NameAlreadyExists;
        presetName = DesiredName.text;
        return Result.Success;
    }
    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
}

[Serializable]
public class PresetSaveDependencies : PanelDependencies
{
    public TMP_InputField DesiredName;
    public TMP_Text DebugText;
    public Image Background;
}