using UnityEngine;
using System;
using System.Collections.Generic;

public class GameplayUIMediator : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    [SerializeField] Transform _orbPanelTransform;

    private OrbPanel _orbPanel;
    Dictionary<RuntimeUIEvent, Action> _eventHandlers => new Dictionary<RuntimeUIEvent, Action>
    {
        { RuntimeUIEvent.SpeedChanged, HandleSpeedChanged },
        { RuntimeUIEvent.ImpulseEnergyChanged, HandleImpulseEnergyChanged },
        { RuntimeUIEvent.InertiaStabilizerChanged, HandleInertiaStabilizerChanged }
    };

    void Awake()
    {
        _orbPanel = new OrbPanel(_orbPanelTransform);
    }

    void Start()
    {   
        if (_playerData == null || _playerData.ThrusterResource == null || _playerData.ImpulseResource == null || _playerData.InertiaResource == null)
        {
            Debug.LogError("GameplayUIMediator requires PlayerData, ThrusterResource, ImpulseResource, and InertiaResource references.", this);
            return;
        }
        _orbPanel.UpdateSpeedText(_playerData.ThrusterResource.Speed);
        _orbPanel.UpdateImpulseBar(_playerData.ImpulseResource.NormalizedEnergy);
        _orbPanel.UpdateInertiaStabilizerText(_playerData.InertiaResource.InertiaStabilizer);
    }

    void OnEnable()
    {
        UIEventHandler.UIEvent += HandleRuntimeUiEvent;
    }

    void OnDisable()
    {
        UIEventHandler.UIEvent -= HandleRuntimeUiEvent;
    }

    void HandleRuntimeUiEvent(RuntimeUIEvent runtimeUiEvent)
    {
        Debug.Log("HandleRuntimeUiEvent: " + runtimeUiEvent);
        if (_eventHandlers.TryGetValue(runtimeUiEvent, out Action handler))
            handler.Invoke();
    }

    void HandleSpeedChanged()
    {
        Debug.Log("HandleSpeedChanged");
        _orbPanel.UpdateSpeedText(_playerData.ThrusterResource.Speed);
    }

    void HandleImpulseEnergyChanged()
    {
        _orbPanel.UpdateImpulseBar(_playerData.ImpulseResource.NormalizedEnergy);
    }

    void HandleInertiaStabilizerChanged()
    {
        _orbPanel.UpdateInertiaStabilizerText(_playerData.InertiaResource.InertiaStabilizer);
    }
}