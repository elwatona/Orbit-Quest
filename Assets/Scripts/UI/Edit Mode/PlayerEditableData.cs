using UnityEngine;
using System;

public class PlayerEditableData : IPanel
{
    public GameObject Root { get; private set; }
    readonly public DropdownComponent ImpulseMode;
    readonly public SliderComponent ImpulseForce;
    readonly public SliderComponent ImpuseCooldown;
    readonly public SliderComponent InertiaDampTime;
    readonly public SliderComponent ThrusterForce;
    readonly public SliderComponent MaxThrustSpeed;
    readonly public PlayerData PlayerData;
    public PlayerEditableData(PlayerEditableDataDependencies dependencies)
    {
        Root = dependencies.Root;
        PlayerData = dependencies.PlayerData;
        ImpulseMode = new DropdownComponent(dependencies.ImpulseMode);
        ImpulseForce = new SliderComponent(dependencies.ImpulseForce);
        ImpuseCooldown = new SliderComponent(dependencies.ImpuseCooldown);
        InertiaDampTime = new SliderComponent(dependencies.InertiaDampTime);
        ThrusterForce = new SliderComponent(dependencies.ThrusterForce);
        MaxThrustSpeed = new SliderComponent(dependencies.MaxThrustSpeed);
    }
    public void Toggle(bool active)
    {
        Root.SetActive(active);
        HandlePlayerDataUpdated(active);
    }
    private void HandlePlayerDataUpdated(bool value)
    {
        if(value)
        {
            ImpulseMode.OnValueChanged += PlayerData.UpdateImpulseMode;
            ImpulseForce.OnValueChanged += PlayerData.UpdateImpulseForce;
            ImpuseCooldown.OnValueChanged += PlayerData.UpdateImpuseCooldown;
            InertiaDampTime.OnValueChanged += PlayerData.UpdateInertiaDampTime;
            ThrusterForce.OnValueChanged += PlayerData.UpdateThrustForce;
            MaxThrustSpeed.OnValueChanged += PlayerData.UpdateStabilizerMaxThrustSpeed;
        }
        else
        {
            ImpulseMode.OnValueChanged -= PlayerData.UpdateImpulseMode;
            ImpulseForce.OnValueChanged -= PlayerData.UpdateImpulseForce;
            ImpuseCooldown.OnValueChanged -= PlayerData.UpdateImpuseCooldown;
            InertiaDampTime.OnValueChanged -= PlayerData.UpdateInertiaDampTime;
            ThrusterForce.OnValueChanged -= PlayerData.UpdateThrustForce;
            MaxThrustSpeed.OnValueChanged -= PlayerData.UpdateStabilizerMaxThrustSpeed;
        }
    }
    public void ConnectPlayerDataToUI()
    {
        ImpulseMode.UpdateValue((int)PlayerData.ImpulseResource.ImpulseMode);
        ImpulseForce.UpdateValue(PlayerData.ImpulseResource.ImpulseForce);
        ImpuseCooldown.UpdateValue(PlayerData.ImpulseResource.RechargeDuration);
        InertiaDampTime.UpdateValue(PlayerData.InertiaResource.InertiaDampTime);
        ThrusterForce.UpdateValue(PlayerData.ThrusterResource.ThrustForce);
        MaxThrustSpeed.UpdateValue(PlayerData.InertiaResource.StabilizerMaxThrustSpeed);

        ImpulseForce.UpdateValueRange(OrbiterTuning.ImpulseForceMin, OrbiterTuning.ImpulseForceMax);
        ImpuseCooldown.UpdateValueRange(OrbiterTuning.RechargeDurationMin, OrbiterTuning.RechargeDurationMax);
        InertiaDampTime.UpdateValueRange(OrbiterTuning.InertiaDampTimeMin, OrbiterTuning.InertiaDampTimeMax);
        ThrusterForce.UpdateValueRange(OrbiterTuning.ThrustForceMin, OrbiterTuning.ThrustForceMax);
        MaxThrustSpeed.UpdateValueRange(OrbiterTuning.StabilizerMaxThrustSpeedMin, OrbiterTuning.StabilizerMaxThrustSpeedMax);
    }
}

[Serializable]
public class PlayerEditableDataDependencies : PanelDependencies
{
    public PlayerData PlayerData;
    public Transform ImpulseMode;
    public Transform ImpulseForce;
    public Transform ImpuseCooldown;
    public Transform InertiaDampTime;
    public Transform ThrusterForce;
    public Transform MaxThrustSpeed;
}