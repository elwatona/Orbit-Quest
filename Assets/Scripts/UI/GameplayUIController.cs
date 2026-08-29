using UnityEngine;
using System;
using System.Collections.Generic;
public class GameplayUIController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] PlayerData _playerData;
    [SerializeField] LevelData _levelData;
    [SerializeField] GameSettings _gameSettings;
    [Header("Dependencies")]
    [SerializeField] PlayerInfoDependencies _playerInfoDependencies;
    [SerializeField] ConsoleDependencies _consoleDependencies;
    [SerializeField] ControlsDependencies _controlsDependencies;
    [SerializeField] AstroInfoDependencies _astroInfoDependencies;
    [SerializeField] InGameMenuDependencies _inGameMenuDependencies;

    private PlayerInfo _playerInfo;
    private Console _console;
    private Controls _controls;
    private AstroInfo _astroInfo;
    private InGameMenu _inGameMenu;
    private IEditable _trackedEditable;

    
    Dictionary<RuntimeUIEvent, Action> _eventHandlers => new Dictionary<RuntimeUIEvent, Action>
    {
        { RuntimeUIEvent.SpeedChanged, HandleSpeedChanged },
        { RuntimeUIEvent.ImpulseEnergyChanged, HandleImpulseEnergyChanged },
        { RuntimeUIEvent.InertiaStabilizerChanged, HandleInertiaStabilizerChanged }
    };
    void Awake()
    {
        if (_gameSettings != null)
            _gameSettings.Initialize();

        _playerInfo = new PlayerInfo(_playerInfoDependencies);
        _console = new Console(_consoleDependencies);
        _controls = new Controls(_controlsDependencies);
        _astroInfo = new AstroInfo(_astroInfoDependencies);
        _inGameMenu = new InGameMenu(
            _inGameMenuDependencies,
            transform,
            _gameSettings,
            onResume: CloseMenu,
            onControlsVisible: visible => _controls.Toggle(visible));
        _inGameMenu.AttachControls(_controls);
    }
    void OnEnable()
    {
        Application.logMessageReceived += _console.Log;
        UIEventHandler.OnUIEvent += HandleUIEvent;
    }
    void OnDisable()
    {
        Application.logMessageReceived -= _console.Log;
        UIEventHandler.OnUIEvent -= HandleUIEvent;
        if (_levelData != null)
            _levelData.SetPaused(false);
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

    void LateUpdate()
    {
        if (_trackedEditable == null || !_astroInfo.Root.activeSelf)
            return;

        if (_trackedEditable is not UnityEngine.Object trackedObject || trackedObject == null)
        {
            ClearTrackedAstro();
            return;
        }

        _astroInfo.Follow(_trackedEditable);
    }
    
    void HandleUIEvent(UIEvent uiEvent)
    {
        switch (uiEvent.Kind)
        {
            case UIEvent.EventKind.Runtime:
                HandleRuntimeEvent(uiEvent);
                break;
            case UIEvent.EventKind.Panel:
                HandlePanelEvent(uiEvent);
                break;
            case UIEvent.EventKind.StateEntered:
                HandleStateEntered(uiEvent.GameState);
                break;
            case UIEvent.EventKind.StateExited:
                HandleStateExited(uiEvent.GameState);
                break;
        }
    }
    void HandleRuntimeEvent(UIEvent uiEvent)
    {
        if (_eventHandlers.TryGetValue(uiEvent.RuntimeUIEvent, out Action handler))
            handler.Invoke();
    }
    void HandlePanelEvent(UIEvent uiEvent)
    {
        HandlePanelEvent(uiEvent.PanelEnum);
        if (uiEvent.Editable != null)
        {
            _trackedEditable = uiEvent.Editable;
            _astroInfo.Update(uiEvent.Editable);
        }
    }
    void HandleStateEntered(GameState gameState)
    {
        _playerInfo.Toggle(gameState == GameState.Precision);
        ClearTrackedAstro();
        if (gameState == GameState.Edition)
            CloseMenu();
    }
    void HandleStateExited(GameState gameState)
    {
        _playerInfo.Toggle(!(gameState == GameState.Precision));
        ClearTrackedAstro();
    }
    void ClearTrackedAstro()
    {
        _trackedEditable = null;
        _astroInfo.Toggle(false);
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
                OpenMenuOnControlsTab();
                break;
            case PanelEnum.Console:
                _console.Toggle(!_console.Root.activeSelf);
                break;
            case PanelEnum.AstroInfo:
                _astroInfo.Toggle(true);
                break;
            case PanelEnum.InGameMenu:
                ToggleMenu();
                break;
        }
    }

    void ToggleMenu()
    {
        bool open = !_inGameMenu.Root.activeSelf;
        if (open)
            OpenMenu();
        else
            CloseMenu();
    }

    void OpenMenu()
    {
        _console.Toggle(false);
        _inGameMenu.Toggle(true);
        _levelData?.SetPaused(true);
    }

    void OpenMenuOnControlsTab()
    {
        _console.Toggle(false);
        if (!_inGameMenu.Root.activeSelf)
        {
            _inGameMenu.OpenOnTab(InGameMenu.ControlsTabIndex);
            _levelData?.SetPaused(true);
            return;
        }

        _inGameMenu.ShowTab(InGameMenu.ControlsTabIndex);
    }

    void CloseMenu()
    {
        bool wasOpen = _inGameMenu != null && _inGameMenu.Root.activeSelf;
        if (wasOpen)
            _inGameMenu.Toggle(false);
        _levelData?.SetPaused(false);
    }
}
