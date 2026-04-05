using UnityEngine;  
using System;

public enum RuntimeUIEvent
{
    SpeedChanged,
    ImpulseEnergyChanged,
    InertiaStabilizerChanged,
    ImpulseForceChanged,
    ImpulseRechargeDurationChanged,
    ImpulseModeChanged,
    ThrustForceChanged,
    MinThrustAssistChanged,
    InertiaDampTimeChanged,
    StabilizerMaxThrustSpeedChanged
}
public class UIEventHandler : MonoBehaviour
{
    public static event Action<RuntimeUIEvent> UIRuntimeEvent;
    public static event Action<PanelEnum> UIPanelEvent;
    [SerializeField] PlayerData _playerData;
    [SerializeField] bool _debug = false;
    void OnEnable()
    {
        SubscribeResourceEventsToUIEvent();
    }
    void OnDisable()
    {
        UnsubscribeResourceEventsFromUIEvent();
    }

    public void SubscribeResourceEventsToUIEvent()
    {
        _playerData.ImpulseResource.ImpulseSettingsChanged += HandleImpulseSettingsChanged;
        _playerData.InertiaResource.InertiaSettingsChanged += HandleInertiaSettingsChanged;
        _playerData.ThrusterResource.ThrusterSettingsChanged += HandleThrusterSettingsChanged;
        PlayerController.OnPanelToggled += HandlePanelToggled;
    }
    public void UnsubscribeResourceEventsFromUIEvent()
    {
        _playerData.ImpulseResource.ImpulseSettingsChanged -= HandleImpulseSettingsChanged;
        _playerData.InertiaResource.InertiaSettingsChanged -= HandleInertiaSettingsChanged;
        _playerData.ThrusterResource.ThrusterSettingsChanged -= HandleThrusterSettingsChanged;
        PlayerController.OnPanelToggled -= HandlePanelToggled;
    }
    void HandleImpulseSettingsChanged(ImpulseSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleImpulseSettingsChanged: " + changeType);
        switch (changeType)
        {
            case ImpulseSettingsChangeType.EnergyChanged:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.ImpulseEnergyChanged);
                break;
            case ImpulseSettingsChangeType.ImpulseForce:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.ImpulseForceChanged);
                break;
            case ImpulseSettingsChangeType.RechargeDuration:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.ImpulseRechargeDurationChanged);
                break;
            case ImpulseSettingsChangeType.ImpulseMode:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.ImpulseModeChanged);
                break;
        }
    }
    void HandleInertiaSettingsChanged(InertiaSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleInertiaSettingsChanged: " + changeType);
        switch (changeType)
        {
            case InertiaSettingsChangeType.InertiaStabilizer:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.InertiaStabilizerChanged);
                break;
            case InertiaSettingsChangeType.InertiaDampTime:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.InertiaDampTimeChanged);
                break;
            case InertiaSettingsChangeType.StabilizerMaxThrustSpeed:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.StabilizerMaxThrustSpeedChanged);
                break;
        }
    }
    void HandleThrusterSettingsChanged(ThrusterSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleThrusterSettingsChanged: " + changeType);
        switch (changeType)
        {
            case ThrusterSettingsChangeType.ThrustForce:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.ThrustForceChanged);
                break;
            case ThrusterSettingsChangeType.MinThrustAssist:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.MinThrustAssistChanged);
                break;
            case ThrusterSettingsChangeType.SpeedChanged:
                UIRuntimeEvent?.Invoke(RuntimeUIEvent.SpeedChanged);
                break;
        }
    }
    void HandlePanelToggled(PanelEnum panel)
    {
        UIPanelEvent?.Invoke(panel);
    }
}