using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerInfo : IPanel
{
    public GameObject Root {get; private set;}
    public readonly TextMeshProUGUI SpeedText;
    public readonly Image ImpulseBar;
    public readonly Image InertiaStabilizer;


    public PlayerInfo(PlayerInfoDependencies dependencies)
    {
        Root = dependencies.Root;
        SpeedText = dependencies.SpeedText;
        ImpulseBar = dependencies.ImpulseBar;
        InertiaStabilizer = dependencies.InertiaStabilizer;
    }
    public void UpdateSpeedText(float speed)
    {
        SpeedText.text = speed.ToString("F1");
    }
    public void UpdateImpulseBar(float impulse)
    {
        Color status = impulse == 1f ? Color.white : Color.gray;
        ImpulseBar.color = status;
        ImpulseBar.fillAmount = impulse;
    }
    public void UpdateInertiaStabilizerText(bool inertiaStabilizer)
    {
        Color status = inertiaStabilizer ? Color.green : Color.yellow;
        InertiaStabilizer.color = status;
    }

    public void Toggle(bool active)
    {
        Root.SetActive(active);
    }
}
[Serializable]
public class PlayerInfoDependencies : PanelDependencies
{
    public TextMeshProUGUI SpeedText;
    public Image ImpulseBar;
    public Image InertiaStabilizer;
}