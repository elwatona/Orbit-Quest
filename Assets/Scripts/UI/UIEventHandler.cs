using UnityEngine;  
using System;

public enum RuntimeUIEvent
{
    None,
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
    public static event Action<UIEvent> UIRuntimeEvent;
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
        PlayerInputController.OnPanelToggled += HandlePanelToggled;
        Astro.OnEditableClicked += HandleAstroEditableClicked;
    }
    public void UnsubscribeResourceEventsFromUIEvent()
    {
        _playerData.ImpulseResource.ImpulseSettingsChanged -= HandleImpulseSettingsChanged;
        _playerData.InertiaResource.InertiaSettingsChanged -= HandleInertiaSettingsChanged;
        _playerData.ThrusterResource.ThrusterSettingsChanged -= HandleThrusterSettingsChanged;
        PlayerInputController.OnPanelToggled -= HandlePanelToggled;
        Astro.OnEditableClicked -= HandleAstroEditableClicked;
    }
    void HandleImpulseSettingsChanged(ImpulseSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleImpulseSettingsChanged: " + changeType);
        RuntimeUIEvent runtimeUIEvent = GetRuntimeUIEventForImpulseSettingsChange(changeType);
        UIRuntimeEvent?.Invoke(new UIEvent(runtimeUIEvent));
    }
    void HandleInertiaSettingsChanged(InertiaSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleInertiaSettingsChanged: " + changeType);
        RuntimeUIEvent runtimeUIEvent = GetRuntimeUIEventForInertiaSettingsChange(changeType);
        UIRuntimeEvent?.Invoke(new UIEvent(runtimeUIEvent));
    }
    void HandleThrusterSettingsChanged(ThrusterSettingsChangeType changeType)
    {
        if (_debug) Debug.Log("HandleThrusterSettingsChanged: " + changeType);
        RuntimeUIEvent runtimeUIEvent = GetRuntimeUIEventForThrusterSettingsChange(changeType);
        UIRuntimeEvent?.Invoke(new UIEvent(runtimeUIEvent));
    }
    void HandlePanelToggled(PanelEnum panel)
    {
        UIRuntimeEvent?.Invoke(new UIEvent(panelEnum: panel));
    }
    void HandleAstroEditableClicked(IEditable editable)
    {
        UIRuntimeEvent?.Invoke(new UIEvent(panelEnum: PanelEnum.AstroInfo, editable: editable));
    }
    static RuntimeUIEvent GetRuntimeUIEventForImpulseSettingsChange(ImpulseSettingsChangeType changeType)
    {
        return changeType switch 
        {
            ImpulseSettingsChangeType.ImpulseForce => RuntimeUIEvent.ImpulseForceChanged,
            ImpulseSettingsChangeType.RechargeDuration => RuntimeUIEvent.ImpulseRechargeDurationChanged,
            ImpulseSettingsChangeType.ImpulseMode => RuntimeUIEvent.ImpulseModeChanged,
            ImpulseSettingsChangeType.EnergyChanged => RuntimeUIEvent.ImpulseEnergyChanged
        };
    }
    static RuntimeUIEvent GetRuntimeUIEventForInertiaSettingsChange(InertiaSettingsChangeType changeType)
    {
        return changeType switch
        {
            InertiaSettingsChangeType.InertiaStabilizer => RuntimeUIEvent.InertiaStabilizerChanged,
            InertiaSettingsChangeType.InertiaDampTime => RuntimeUIEvent.InertiaDampTimeChanged,
            InertiaSettingsChangeType.StabilizerMaxThrustSpeed => RuntimeUIEvent.StabilizerMaxThrustSpeedChanged
        };
    }
    static RuntimeUIEvent GetRuntimeUIEventForThrusterSettingsChange(ThrusterSettingsChangeType changeType)
    {
        return changeType switch
        {
            ThrusterSettingsChangeType.ThrustForce => RuntimeUIEvent.ThrustForceChanged,
            ThrusterSettingsChangeType.MinThrustAssist => RuntimeUIEvent.MinThrustAssistChanged,
            ThrusterSettingsChangeType.SpeedChanged => RuntimeUIEvent.SpeedChanged
        };
    }
}
public class UIEvent
{
    public readonly RuntimeUIEvent RuntimeUIEvent;
    public readonly PanelEnum PanelEnum;
    public readonly IEditable Editable;
    public UIEvent(RuntimeUIEvent runtimeUIEvent = RuntimeUIEvent.None, PanelEnum panelEnum = PanelEnum.None, IEditable editable = null)
    {
        RuntimeUIEvent = runtimeUIEvent;
        PanelEnum = panelEnum;
        Editable = editable;
    }
}