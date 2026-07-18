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
    public static event Action<UIEvent> OnUIEvent;
    [SerializeField] PlayerData _playerData;
    [SerializeField] LevelData _levelData;
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
        _levelData.StateEntered += HandleStateEntered;
        _levelData.StateExited += HandleStateExited;
    }
    public void UnsubscribeResourceEventsFromUIEvent()
    {
        _playerData.ImpulseResource.ImpulseSettingsChanged -= HandleImpulseSettingsChanged;
        _playerData.InertiaResource.InertiaSettingsChanged -= HandleInertiaSettingsChanged;
        _playerData.ThrusterResource.ThrusterSettingsChanged -= HandleThrusterSettingsChanged;
        PlayerInputController.OnPanelToggled -= HandlePanelToggled;
        Astro.OnEditableClicked -= HandleAstroEditableClicked;
        _levelData.StateEntered -= HandleStateEntered;
        _levelData.StateExited -= HandleStateExited;
    }
    void HandleStateEntered(GameState gameState)
    {
        OnUIEvent?.Invoke(new UIEvent(UIEvent.EventKind.StateEntered, gameState));
    }
    void HandleStateExited(GameState gameState)
    {
        OnUIEvent?.Invoke(new UIEvent(UIEvent.EventKind.StateExited, gameState));
    }
    void HandleImpulseSettingsChanged(ImpulseSettingsChangeType changeType)
    {
        if(_levelData.CurrentState != GameState.Precision) return;
        if (_debug) Debug.Log("HandleImpulseSettingsChanged: " + changeType);
        RuntimeUIEvent runtimeUIEvent = GetRuntimeUIEventForImpulseSettingsChange(changeType);
        OnUIEvent?.Invoke(new UIEvent(runtimeUIEvent));
    }
    void HandleInertiaSettingsChanged(InertiaSettingsChangeType changeType)
    {
        if(_levelData.CurrentState != GameState.Precision) return;
        if (_debug) Debug.Log("HandleInertiaSettingsChanged: " + changeType);
        RuntimeUIEvent runtimeUIEvent = GetRuntimeUIEventForInertiaSettingsChange(changeType);
        OnUIEvent?.Invoke(new UIEvent(runtimeUIEvent));
    }
    void HandleThrusterSettingsChanged(ThrusterSettingsChangeType changeType)
    {
        if(_levelData.CurrentState != GameState.Precision) return;
        if (_debug) Debug.Log("HandleThrusterSettingsChanged: " + changeType);
        RuntimeUIEvent runtimeUIEvent = GetRuntimeUIEventForThrusterSettingsChange(changeType);
        OnUIEvent?.Invoke(new UIEvent(runtimeUIEvent));
    }
    void HandlePanelToggled(PanelEnum panel)
    {
        OnUIEvent?.Invoke(new UIEvent(panel));
    }
    void HandleAstroEditableClicked(IEditable editable)
    {
        if(_levelData.CurrentState != GameState.Precision) return;
        OnUIEvent?.Invoke(new UIEvent(PanelEnum.AstroInfo, editable));
    }
    static RuntimeUIEvent GetRuntimeUIEventForImpulseSettingsChange(ImpulseSettingsChangeType changeType)
    {
        return changeType switch 
        {
            ImpulseSettingsChangeType.ImpulseForce => RuntimeUIEvent.ImpulseForceChanged,
            ImpulseSettingsChangeType.RechargeDuration => RuntimeUIEvent.ImpulseRechargeDurationChanged,
            ImpulseSettingsChangeType.ImpulseMode => RuntimeUIEvent.ImpulseModeChanged,
            ImpulseSettingsChangeType.EnergyChanged => RuntimeUIEvent.ImpulseEnergyChanged,
            _ => RuntimeUIEvent.None
        };
    }
    static RuntimeUIEvent GetRuntimeUIEventForInertiaSettingsChange(InertiaSettingsChangeType changeType)
    {
        return changeType switch
        {
            InertiaSettingsChangeType.InertiaStabilizer => RuntimeUIEvent.InertiaStabilizerChanged,
            InertiaSettingsChangeType.InertiaDampTime => RuntimeUIEvent.InertiaDampTimeChanged,
            InertiaSettingsChangeType.StabilizerMaxThrustSpeed => RuntimeUIEvent.StabilizerMaxThrustSpeedChanged,
            _ => RuntimeUIEvent.None
        };
    }
    static RuntimeUIEvent GetRuntimeUIEventForThrusterSettingsChange(ThrusterSettingsChangeType changeType)
    {
        return changeType switch
        {
            ThrusterSettingsChangeType.ThrustForce => RuntimeUIEvent.ThrustForceChanged,
            ThrusterSettingsChangeType.MinThrustAssist => RuntimeUIEvent.MinThrustAssistChanged,
            ThrusterSettingsChangeType.SpeedChanged => RuntimeUIEvent.SpeedChanged,
            _ => RuntimeUIEvent.None
        };
    }
}
public class UIEvent
{
    public enum EventKind { Runtime, Panel, StateEntered, StateExited }
    public readonly EventKind Kind;
    public readonly RuntimeUIEvent RuntimeUIEvent;
    public readonly PanelEnum PanelEnum;
    public readonly IEditable Editable;
    public readonly GameState GameState;
    public UIEvent(RuntimeUIEvent runtimeUIEvent)
    {
        Kind = EventKind.Runtime;
        RuntimeUIEvent = runtimeUIEvent;
        PanelEnum = default;
        Editable = null;
        GameState = default;
    }
    public UIEvent(PanelEnum panelEnum, IEditable editable = null)
    {
        Kind = EventKind.Panel;
        RuntimeUIEvent = default;
        PanelEnum = panelEnum;
        Editable = editable;
        GameState = default;
    }
    public UIEvent(EventKind kind, GameState gameState)
    {
        Kind = kind; // StateEntered o StateExited
        RuntimeUIEvent = default;
        PanelEnum = default;
        Editable = null;
        GameState = gameState;
    }
}
