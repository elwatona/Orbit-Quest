using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

public class EditorPanelsMediator
{
    readonly PlayerData _playerData;
    readonly PanelController[] _panelControllers;
    readonly ScrollComponent _targetsToOrbitScrollComponent;
    readonly GameObject _rootPanelButtons;
    private EditableOrbitData _editableOrbitData;
    private EditableBodyData _editableBodyData;
    private EditableOrbiterData _editableOrbiterData;
    public IEditable SelectedEditable { get; private set; }
    public List<IEditable> TargetsToOrbit { get; private set; } = new List<IEditable>();
    public EditorPanelsMediator(PanelController[] panelControllers, PlayerData playerData, ScrollComponent targetsToOrbitScrollComponent, GameObject rootPanelButtons)
    {
        _panelControllers = panelControllers;
        _playerData = playerData;
        _targetsToOrbitScrollComponent = targetsToOrbitScrollComponent;
        _rootPanelButtons = rootPanelButtons;
        Initialize();
    }
    void Initialize()
    {
        foreach (PanelController panelController in _panelControllers)
        {
            panelController.Initialize();
        }
    }
    public void InyectEditable(IEditable editable)
    {
        if(SelectedEditable != null)
        {
            SelectedEditable.Deselected();
            DisconnectAstroDataFromUI();
        }

        SelectedEditable = editable;
        _editableOrbitData = SelectedEditable.Data.orbit;
        _editableBodyData = SelectedEditable.Data.body;
        _editableOrbiterData = SelectedEditable.Data.orbiter;

        SelectedEditable.Selected();
        ConnectAstroDataToUI();
    }
    void ConnectAstroDataToUI()
    {
        if(SelectedEditable.HasOrbiter()) LoadOrbiterDataToUI();
        LoadBodyOrbitDataToUI();
        TogglePanels(true);
    }
    void DisconnectAstroDataFromUI()
    {
        if(SelectedEditable.HasOrbiter()) DisconnectOrbiterDataFromUI();
        DisconnectBodyOrbitDataFromUI();
    }
    void LoadOrbiterDataToUI()
    {
        PanelController panel = _panelControllers[1];

        panel.SliderComponents[0].OnValueChanged += SelectedEditable.UpdateOrbiterRadius;
        panel.SliderComponents[1].OnValueChanged += SelectedEditable.UpdateOrbiterSpeed;
        panel.SliderComponents[2].OnValueChanged += SelectedEditable.UpdateOrbiterEccentricity;

        panel.SliderComponents[0].UpdateValue(SelectedEditable.Data.orbiter.radius);
        panel.SliderComponents[1].UpdateValue(SelectedEditable.Data.orbiter.speed);
        panel.SliderComponents[2].UpdateValue(SelectedEditable.Data.orbiter.eccentricity);

        panel.SliderComponents[0].UpdateValueRange(AstroTuning.OrbiterRadiusMin, AstroTuning.OrbiterRadiusMax);
        panel.SliderComponents[1].UpdateValueRange(AstroTuning.OrbiterSpeedMin, AstroTuning.OrbiterSpeedMax);
        panel.SliderComponents[2].UpdateValueRange(AstroTuning.OrbiterEccentricityMin, AstroTuning.OrbiterEccentricityMax);

        LoadTargetsToOrbitDataToUI();
    }
    void LoadTargetsToOrbitDataToUI()
    {
        _targetsToOrbitScrollComponent.ClearItems();
        TargetsToOrbit = SelectedEditable.Data.orbiter.targets.ToList();
        if(TargetsToOrbit.Count == 0) return;
        else if (TargetsToOrbit.Count == 1)
            _targetsToOrbitScrollComponent.AddItem(TargetsToOrbit[0].transform.name);
        else if(TargetsToOrbit.Count > 1) 
            _targetsToOrbitScrollComponent.AddItems(TargetsToOrbit.Select(target => target.transform.name).ToArray());
    }
    void LoadBodyOrbitDataToUI()
    {
        PanelController panel = _panelControllers[2];

        panel.SliderComponents[0].OnValueChanged += SelectedEditable.UpdateOrbitGravity;
        panel.SliderComponents[1].OnValueChanged += SelectedEditable.UpdateOrbitRadius;
        panel.SliderComponents[2].OnValueChanged += SelectedEditable.UpdateBodyRadius;
        panel.SliderComponents[3].OnValueChanged += SelectedEditable.UpdateBodyRotationSpeed;

        panel.DropdownComponents[0].UpdateValue((int)SelectedEditable.Data.type);
        panel.SliderComponents[0].UpdateValue(SelectedEditable.Data.orbit.gravity);
        panel.SliderComponents[1].UpdateValue(SelectedEditable.Data.orbit.radius);
        panel.SliderComponents[2].UpdateValue(SelectedEditable.Data.body.radius);
        panel.SliderComponents[3].UpdateValue(SelectedEditable.Data.body.rotationSpeed);

        panel.SliderComponents[0].UpdateValueRange(AstroTuning.GravityMin, AstroTuning.GravityMax);
        panel.SliderComponents[1].UpdateValueRange(AstroTuning.OrbitRadiusMin, AstroTuning.OrbitRadiusMax);
        panel.SliderComponents[2].UpdateValueRange(AstroTuning.BodyRadiusMin, AstroTuning.BodyRadiusMax);
        panel.SliderComponents[3].UpdateValueRange(AstroTuning.RotationSpeedMin, AstroTuning.RotationSpeedMax);
    }
    void DisconnectOrbiterDataFromUI()
    {
        PanelController panel = _panelControllers[1];

        panel.SliderComponents[0].OnValueChanged -= SelectedEditable.UpdateOrbiterRadius;
        panel.SliderComponents[1].OnValueChanged -= SelectedEditable.UpdateOrbiterSpeed;
        panel.SliderComponents[2].OnValueChanged -= SelectedEditable.UpdateOrbiterEccentricity;
    }
    void DisconnectBodyOrbitDataFromUI()
    {
        PanelController panel = _panelControllers[2];   

        panel.SliderComponents[0].OnValueChanged -= SelectedEditable.UpdateOrbitGravity;
        panel.SliderComponents[1].OnValueChanged -= SelectedEditable.UpdateOrbitRadius;
        panel.SliderComponents[2].OnValueChanged -= SelectedEditable.UpdateBodyRadius;
        panel.SliderComponents[3].OnValueChanged -= SelectedEditable.UpdateBodyRotationSpeed;
    }
    public void ConnectPlayerDataToUI()
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
    public void DisconnectPlayerDataFromUI()
    {
        PanelController panel = _panelControllers[0];

        panel.DropdownComponents[0].OnValueChanged -= _playerData.UpdateImpulseMode;
        panel.SliderComponents[0].OnValueChanged -= _playerData.UpdateImpulseForce;
        panel.SliderComponents[1].OnValueChanged -= _playerData.UpdateImpuseCooldown;
        panel.SliderComponents[2].OnValueChanged -= _playerData.UpdateInertiaDampTime;
        panel.SliderComponents[3].OnValueChanged -= _playerData.UpdateThrustForce;
        panel.SliderComponents[4].OnValueChanged -= _playerData.UpdateStabilizerMaxThrustSpeed;
    }
    public void ClearTargetsToOrbit()
    {
        TargetsToOrbit = new List<IEditable>();
        _targetsToOrbitScrollComponent.ClearItems();
        SelectedEditable.UpdateOrbiterTargets(TargetsToOrbit.ToArray());
    }
    public void UpdateTargetsToOrbit(IEditable[] targets)
    {
        SelectedEditable.UpdateOrbiterTargets(targets);
        LoadTargetsToOrbitDataToUI();
    }
    public void TogglePanels(bool active)
    {
        _rootPanelButtons.SetActive(active);
        for (int i = 0; i < _panelControllers.Length; i++)
        {
            PanelStrategy(i, active);
            TogglePanel(i, active);
        }
    }
    private void PanelStrategy(int index, bool active)
    {
        switch (index)
        {
            case 1:
                _panelControllers[index].SetComponentsActiveValue(PanelController.ComponentType.Slider, 2, active & active ? SelectedEditable.Data.orbiter.targets.Length > 1 : false);
                break;
            case 2:
                _panelControllers[index].SetComponentsActiveValue(PanelController.ComponentType.Slider, 0, active & active ? SelectedEditable.Data.type != AstroType.Sun : false);
                _panelControllers[index].SetComponentsActiveValue(PanelController.ComponentType.Slider, 1, active & active ? SelectedEditable.Data.type != AstroType.Sun : false);
                break;
        }
    }
    public void TogglePanel(int index, bool active)
    {
            bool hasOrbiter = SelectedEditable != null ? SelectedEditable.HasOrbiter() : false;
            active = index == 1 ? active & hasOrbiter : active;
        _panelControllers[index].SetActive(active);
    }
    public void TogglePanel(int index)
    {
        _panelControllers[index].ToggleActive();
    }
}