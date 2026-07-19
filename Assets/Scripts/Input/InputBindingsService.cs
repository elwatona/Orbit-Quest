using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public readonly struct InputBindingInfo
{
    public InputBindingInfo(
        string mapName,
        string actionName,
        int bindingIndex,
        string displayName,
        bool isComposite,
        bool isPartOfComposite)
    {
        MapName = mapName;
        ActionName = actionName;
        BindingIndex = bindingIndex;
        DisplayName = displayName;
        IsComposite = isComposite;
        IsPartOfComposite = isPartOfComposite;
    }

    public string MapName { get; }
    public string ActionName { get; }
    public int BindingIndex { get; }
    public string DisplayName { get; }
    public bool IsComposite { get; }
    public bool IsPartOfComposite { get; }
}

public class InputBindingsService
{
    const string PlayerPrefsKey = "InputBindingOverrides";
    const string DefaultMapName = "Player";
    const string ExcludedMapName = "UI";

    readonly InputActionAsset _actions;
    InputActionRebindingExtensions.RebindingOperation _rebindOperation;
    bool _loaded;

    public event Action BindingsChanged;

    public InputBindingsService(InputActionAsset actions)
    {
        _actions = actions;
    }

    public bool IsRebinding => _rebindOperation != null;

    public IReadOnlyList<InputBindingInfo> GetPlayerBindings()
        => GetBindings(DefaultMapName);

    public IReadOnlyList<InputBindingInfo> GetBindings(string mapName = null)
    {
        var results = new List<InputBindingInfo>();
        if (_actions == null) return results;

        foreach (InputActionMap map in _actions.actionMaps)
        {
            if (map.name == ExcludedMapName) continue;
            if (!string.IsNullOrEmpty(mapName) && map.name != mapName) continue;

            foreach (InputAction action in map.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];
                    if (binding.isComposite) continue;

                    results.Add(new InputBindingInfo(
                        map.name,
                        action.name,
                        i,
                        action.GetBindingDisplayString(i),
                        binding.isComposite,
                        binding.isPartOfComposite));
                }
            }
        }

        return results;
    }

    public void Load()
    {
        if (_actions == null) return;

        string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
            _actions.LoadBindingOverridesFromJson(json);

        _loaded = true;
        BindingsChanged?.Invoke();
    }

    public void EnsureLoaded()
    {
        if (!_loaded)
            Load();
    }

    public void Save()
    {
        if (_actions == null) return;

        string json = _actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
        BindingsChanged?.Invoke();
    }

    public void ResetToDefault(string actionName, int bindingIndex, string mapName = DefaultMapName)
    {
        InputAction action = FindAction(mapName, actionName);
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;

        action.RemoveBindingOverride(bindingIndex);
        BindingsChanged?.Invoke();
    }

    public void ResetAll()
    {
        if (_actions == null) return;

        _actions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PlayerPrefsKey);
        PlayerPrefs.Save();
        BindingsChanged?.Invoke();
    }

    public void StartRebind(
        string actionName,
        int bindingIndex,
        Action<InputBindingInfo> onComplete = null,
        Action onCancel = null,
        string mapName = DefaultMapName)
    {
        CancelRebind();

        InputAction action = FindAction(mapName, actionName);
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;
        if (action.bindings[bindingIndex].isComposite) return;

        EnsureLoaded();

        bool wasEnabled = action.enabled;
        if (wasEnabled)
            action.Disable();

        _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                CleanupRebind(wasEnabled, action);
                Save();
                InputBindingInfo info = new InputBindingInfo(
                    mapName,
                    actionName,
                    bindingIndex,
                    action.GetBindingDisplayString(bindingIndex),
                    false,
                    action.bindings[bindingIndex].isPartOfComposite);
                BindingsChanged?.Invoke();
                onComplete?.Invoke(info);
            })
            .OnCancel(operation =>
            {
                CleanupRebind(wasEnabled, action);
                onCancel?.Invoke();
            });

        _rebindOperation.Start();
    }

    public void CancelRebind()
    {
        if (_rebindOperation == null) return;
        _rebindOperation.Cancel();
    }

    InputAction FindAction(string mapName, string actionName)
    {
        if (_actions == null) return null;
        InputActionMap map = _actions.FindActionMap(mapName, throwIfNotFound: false);
        return map?.FindAction(actionName, throwIfNotFound: false);
    }

    void CleanupRebind(bool wasEnabled, InputAction action)
    {
        if (_rebindOperation != null)
        {
            _rebindOperation.Dispose();
            _rebindOperation = null;
        }

        if (wasEnabled && action != null && !action.enabled)
            action.Enable();
    }
}
