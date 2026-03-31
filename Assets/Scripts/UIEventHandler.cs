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
    public static event Action<RuntimeUIEvent> UIEvent;

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
    }
    public void UnsubscribeResourceEventsFromUIEvent()
    {
        _playerData.ImpulseResource.ImpulseSettingsChanged -= HandleImpulseSettingsChanged;
        _playerData.InertiaResource.InertiaSettingsChanged -= HandleInertiaSettingsChanged;
        _playerData.ThrusterResource.ThrusterSettingsChanged -= HandleThrusterSettingsChanged;
    }
    void HandleImpulseSettingsChanged(ImpulseSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleImpulseSettingsChanged: " + changeType);
        switch (changeType)
        {
            case ImpulseSettingsChangeType.EnergyChanged:
                UIEvent?.Invoke(RuntimeUIEvent.ImpulseEnergyChanged);
                break;
            case ImpulseSettingsChangeType.ImpulseForce:
                UIEvent?.Invoke(RuntimeUIEvent.ImpulseForceChanged);
                break;
            case ImpulseSettingsChangeType.RechargeDuration:
                UIEvent?.Invoke(RuntimeUIEvent.ImpulseRechargeDurationChanged);
                break;
            case ImpulseSettingsChangeType.ImpulseMode:
                UIEvent?.Invoke(RuntimeUIEvent.ImpulseModeChanged);
                break;
        }
    }
    void HandleInertiaSettingsChanged(InertiaSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleInertiaSettingsChanged: " + changeType);
        switch (changeType)
        {
            case InertiaSettingsChangeType.InertiaStabilizer:
                UIEvent?.Invoke(RuntimeUIEvent.InertiaStabilizerChanged);
                break;
            case InertiaSettingsChangeType.InertiaDampTime:
                UIEvent?.Invoke(RuntimeUIEvent.InertiaDampTimeChanged);
                break;
            case InertiaSettingsChangeType.StabilizerMaxThrustSpeed:
                UIEvent?.Invoke(RuntimeUIEvent.StabilizerMaxThrustSpeedChanged);
                break;
        }
    }
    void HandleThrusterSettingsChanged(ThrusterSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleThrusterSettingsChanged: " + changeType);
        switch (changeType)
        {
            case ThrusterSettingsChangeType.ThrustForce:
                UIEvent?.Invoke(RuntimeUIEvent.ThrustForceChanged);
                break;
            case ThrusterSettingsChangeType.MinThrustAssist:
                UIEvent?.Invoke(RuntimeUIEvent.MinThrustAssistChanged);
                break;
            case ThrusterSettingsChangeType.SpeedChanged:
                UIEvent?.Invoke(RuntimeUIEvent.SpeedChanged);
                break;
        }
    }
}