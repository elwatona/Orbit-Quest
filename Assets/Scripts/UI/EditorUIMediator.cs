using UnityEngine;

public class EditorUIMediator : MonoBehaviour
{
    [SerializeField] PlayerData _playerData;
    [SerializeField] PanelController[] _panelControllers;
    [SerializeField] Transform _targetsToOrbitContainer;
    void Awake()
    {
        foreach (PanelController panelController in _panelControllers)
        {
            panelController.Initialize();
        }

    }
    void OnEnable()
    {
        UIEventHandler.UIEvent += HandleRuntimeUiEvent;
        ConnectPlayerDataToUI();
    }
    void OnDisable()
    {
        UIEventHandler.UIEvent -= HandleRuntimeUiEvent;
        DisconnectPlayerDataFromUI();
    }
    void HandleRuntimeUiEvent(RuntimeUIEvent runtimeUiEvent)
    {
        switch (runtimeUiEvent)
        {
            case RuntimeUIEvent.SpeedChanged:
                break;
            case RuntimeUIEvent.ImpulseEnergyChanged:
                break;
            case RuntimeUIEvent.InertiaStabilizerChanged:
                break;
        }
    }
    void ConnectPlayerDataToUI()
    {
        PanelController panel = _panelControllers[0];
        panel.DropdownComponents[0].OnValueChanged += _playerData.UpdateImpulseMode;
        panel.SliderComponents[0].OnValueChanged += _playerData.UpdateImpulseForce;
        panel.SliderComponents[1].OnValueChanged += _playerData.UpdateImpuseCooldown;
        panel.SliderComponents[2].OnValueChanged += _playerData.UpdateInertiaDampTime;
        panel.SliderComponents[3].OnValueChanged += _playerData.UpdateThrustForce;
        panel.SliderComponents[4].OnValueChanged += _playerData.UpdateStabilizerMaxThrustSpeed;

        panel.DropdownComponents[0].UpdateValue((int)_playerData.ImpulseResource.ImpulseMode);
        panel.SliderComponents[0].UpdateValue(_playerData.ImpulseResource.ImpulseForce);
        panel.SliderComponents[1].UpdateValue(_playerData.ImpulseResource.RechargeDuration);
        panel.SliderComponents[2].UpdateValue(_playerData.InertiaResource.InertiaDampTime);
        panel.SliderComponents[3].UpdateValue(_playerData.ThrusterResource.ThrustForce);
        panel.SliderComponents[4].UpdateValue(_playerData.InertiaResource.StabilizerMaxThrustSpeed);

        panel.SliderComponents[0].UpdateValueRange(OrbiterTuning.ImpulseForceMin, OrbiterTuning.ImpulseForceMax);
        panel.SliderComponents[1].UpdateValueRange(OrbiterTuning.RechargeDurationMin, OrbiterTuning.RechargeDurationMax);
        panel.SliderComponents[2].UpdateValueRange(OrbiterTuning.InertiaDampTimeMin, OrbiterTuning.InertiaDampTimeMax);
        panel.SliderComponents[3].UpdateValueRange(OrbiterTuning.ThrustForceMin, OrbiterTuning.ThrustForceMax);
        panel.SliderComponents[4].UpdateValueRange(OrbiterTuning.StabilizerMaxThrustSpeedMin, OrbiterTuning.StabilizerMaxThrustSpeedMax);
    }
    void DisconnectPlayerDataFromUI()
    {
        PanelController panel = _panelControllers[0];
        panel.DropdownComponents[0].OnValueChanged -= _playerData.UpdateImpulseMode;
        panel.SliderComponents[0].OnValueChanged -= _playerData.UpdateImpulseForce;
        panel.SliderComponents[1].OnValueChanged -= _playerData.UpdateImpuseCooldown;
        panel.SliderComponents[2].OnValueChanged -= _playerData.UpdateInertiaDampTime;
        panel.SliderComponents[3].OnValueChanged -= _playerData.UpdateThrustForce;
        panel.SliderComponents[4].OnValueChanged -= _playerData.UpdateStabilizerMaxThrustSpeed;
    }
}