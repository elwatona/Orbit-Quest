using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

public class GameplayUIMediator : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    [SerializeField] Transform _orbPanelTransform;
    [SerializeField] GameObject _controlsPanel;
    [SerializeField] TextMeshProUGUI _editModeText, _versionText;

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
        _versionText.text = $"{Application.version} {Application.unityVersion}";
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
        HandleEditModeToggled();
    }

    void OnEnable()
    {
        UIEventHandler.UIRuntimeEvent += HandleRuntimeUiEvent;
        UIEventHandler.UIPanelEvent += HandlePanelToggled;
        _playerData.IsInEditModeUpdated += HandleEditModeToggled;
    }

    void OnDisable()
    {
        UIEventHandler.UIRuntimeEvent -= HandleRuntimeUiEvent;
        UIEventHandler.UIPanelEvent -= HandlePanelToggled;
        _playerData.IsInEditModeUpdated -= HandleEditModeToggled;
    }

    void HandleRuntimeUiEvent(RuntimeUIEvent runtimeUiEvent)
    {
        if (_eventHandlers.TryGetValue(runtimeUiEvent, out Action handler))
            handler.Invoke();
    }

    void HandlePanelToggled(PanelEnum panel)
    {
        if(panel == PanelEnum.Controls) _controlsPanel.SetActive(!_controlsPanel.activeSelf);
    }

    void HandleSpeedChanged()
    {
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

    void HandleEditModeToggled()
    {
        _editModeText.color = _playerData.IsInEditMode ? Color.green : Color.white;
        _editModeText.text = _playerData.IsInEditMode ? "Edit Mode On" : "Edit Mode Off";
    }
}