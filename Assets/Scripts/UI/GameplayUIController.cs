using UnityEngine;
using System;
using System.Collections.Generic;
public class GameplayUIController : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    [SerializeField] PlayerInfoDependencies _playerInfoDependencies;
    [SerializeField] ConsoleDependencies _consoleDependencies;
    [SerializeField] ControlsDependencies _controlsDependencies;
    [SerializeField] AstroInfoDependencies _astroInfoDependencies;

    private PlayerInfo _playerInfo;
    private Console _console;
    private Controls _controls;
    private AstroInfo _astroInfo;

    
    Dictionary<RuntimeUIEvent, Action> _eventHandlers => new Dictionary<RuntimeUIEvent, Action>
    {
        { RuntimeUIEvent.SpeedChanged, HandleSpeedChanged },
        { RuntimeUIEvent.ImpulseEnergyChanged, HandleImpulseEnergyChanged },
        { RuntimeUIEvent.InertiaStabilizerChanged, HandleInertiaStabilizerChanged }
    };
    void Awake()
    {
        _playerInfo = new PlayerInfo(_playerInfoDependencies);
        _console = new Console(_consoleDependencies);
        _controls = new Controls(_controlsDependencies);
        _astroInfo = new AstroInfo(_astroInfoDependencies);
    }
    void OnEnable()
    {
        Application.logMessageReceived += _console.Log;
        UIEventHandler.UIRuntimeEvent += HandleRuntimeUiEvent;
    }
    void OnDisable()
    {
        Application.logMessageReceived -= _console.Log;
        UIEventHandler.UIRuntimeEvent -= HandleRuntimeUiEvent;
    }
    void Start()
    {
        if (_playerData == null || _playerData.ThrusterResource == null || _playerData.ImpulseResource == null || _playerData.InertiaResource == null)
        {
            Debug.LogError("GameplayUIController requires PlayerData, ThrusterResource, ImpulseResource, and InertiaResource references.", this);
            return;
        }
        _playerInfo.UpdateSpeedText(_playerData.ThrusterResource.Speed);
        _playerInfo.UpdateImpulseBar(_playerData.ImpulseResource.NormalizedEnergy);
        _playerInfo.UpdateInertiaStabilizerText(_playerData.InertiaResource.InertiaStabilizer);
    }
    
    void HandleRuntimeUiEvent(UIEvent uiEvent)
    {
        if (_eventHandlers.TryGetValue(uiEvent.RuntimeUIEvent, out Action handler))
            handler.Invoke();
        if (uiEvent.PanelEnum != PanelEnum.None)
            HandlePanelEvent(uiEvent.PanelEnum);
        if (uiEvent.Editable != null)
            _astroInfo.Update(uiEvent.Editable);
    }
    void HandleSpeedChanged()
    {
        _playerInfo.UpdateSpeedText(_playerData.ThrusterResource.Speed);
    }
    void HandleImpulseEnergyChanged()
    {
        _playerInfo.UpdateImpulseBar(_playerData.ImpulseResource.NormalizedEnergy);
    }
    void HandleInertiaStabilizerChanged()
    {
        _playerInfo.UpdateInertiaStabilizerText(_playerData.InertiaResource.InertiaStabilizer);
    }
    void HandlePanelEvent(PanelEnum panelEnum)
    {
        switch (panelEnum)
        {
            case PanelEnum.Controls:
                _controls.Toggle(!_controls.Root.activeSelf);
                break;
            case PanelEnum.Console:
                _console.Toggle(!_console.Root.activeSelf);
                break;
            case PanelEnum.AstroInfo:
                _astroInfo.Toggle(true);
                break;
        }
    }
}