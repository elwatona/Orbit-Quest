using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BindingPressType
{
    Press,
    Hold,
    Release
}

public readonly struct InputBindingInfo
{
    public InputBindingInfo(
        string mapName,
        string actionName,
        int bindingIndex,
        string displayName,
        bool isComposite,
        bool isPartOfComposite,
        bool isModifierChord = false,
        int modifierBindingIndex = -1,
        int compositeBindingIndex = -1,
        string partName = null)
    {
        MapName = mapName;
        ActionName = actionName;
        BindingIndex = bindingIndex;
        DisplayName = displayName;
        IsComposite = isComposite;
        IsPartOfComposite = isPartOfComposite;
        IsModifierChord = isModifierChord;
        ModifierBindingIndex = modifierBindingIndex;
        CompositeBindingIndex = compositeBindingIndex;
        PartName = partName;
    }

    public string MapName { get; }
    public string ActionName { get; }
    public int BindingIndex { get; }
    public string DisplayName { get; }
    public bool IsComposite { get; }
    public bool IsPartOfComposite { get; }
    public bool IsModifierChord { get; }
    public int ModifierBindingIndex { get; }
    public int CompositeBindingIndex { get; }
    public string PartName { get; }
}

public readonly struct OneModifierBinding
{
    public OneModifierBinding(int compositeIndex, int modifierIndex, int bindingIndex)
    {
        CompositeIndex = compositeIndex;
        ModifierIndex = modifierIndex;
        BindingIndex = bindingIndex;
    }

    public int CompositeIndex { get; }
    public int ModifierIndex { get; }
    public int BindingIndex { get; }
}

public class InputBindingsService
{
    const string PlayerPrefsKey = "InputBindingOverrides";
    const string DefaultMapName = "Player";
    const string ExcludedMapName = "UI";
    static readonly string[] HiddenActions = { "Look X", "Look Y" };
    const string PressInteraction = "Press";
    const string HoldInteraction = "Hold";
    const string ReleaseInteraction = "Press(behavior=1)";

    readonly InputActionAsset _actions;
    bool _loaded;
    bool _rebinding;
    InputAction _disabledAction;
    bool _wasEnabled;

    public event Action BindingsChanged;

    public InputBindingsService(InputActionAsset actions)
    {
        _actions = actions;
    }

    public bool IsRebinding => _rebinding || InputRebind.IsCapturing;

    static bool IsHiddenAction(string actionName)
    {
        for (int i = 0; i < HiddenActions.Length; i++)
        {
            if (HiddenActions[i] == actionName)
                return true;
        }
        return false;
    }

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
                if (IsHiddenAction(action.name)) continue;

                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];
                    if (binding.isComposite) continue;

                    if (TryGetOneModifier(action, i, out OneModifierBinding chord))
                    {
                        // Emit once on the Binding part; skip the modifier part.
                        if (i != chord.BindingIndex) continue;

                        string display = GetChordDisplay(action, chord);
                        results.Add(new InputBindingInfo(
                            map.name,
                            action.name,
                            chord.BindingIndex,
                            display,
                            false,
                            true,
                            isModifierChord: true,
                            modifierBindingIndex: chord.ModifierIndex,
                            compositeBindingIndex: chord.CompositeIndex,
                            partName: "Binding"));
                        continue;
                    }

                    results.Add(new InputBindingInfo(
                        map.name,
                        action.name,
                        i,
                        action.GetBindingDisplayString(i),
                        binding.isComposite,
                        binding.isPartOfComposite,
                        partName: binding.name));
                }
            }
        }

        return results;
    }

    public void Load()
    {
        if (_actions == null) return;

        string prefsJson = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
        if (!string.IsNullOrEmpty(prefsJson))
            _actions.LoadBindingOverridesFromJson(prefsJson);

        RestoreHiddenLookAxes();

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

        if (TryGetOneModifier(action, bindingIndex, out OneModifierBinding chord))
        {
            action.RemoveBindingOverride(chord.ModifierIndex);
            action.RemoveBindingOverride(chord.BindingIndex);
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }

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

    public BindingPressType GetPressType(string actionName, int bindingIndex, string mapName = DefaultMapName)
    {
        InputAction action = FindAction(mapName, actionName);
        if (action == null) return BindingPressType.Press;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return BindingPressType.Press;

        InputBinding binding = action.bindings[bindingIndex];
        string interactions = binding.effectiveInteractions;
        if (string.IsNullOrEmpty(interactions))
            interactions = binding.interactions;

        return ParsePressType(interactions);
    }

    public void SetPressType(
        string actionName,
        int bindingIndex,
        BindingPressType type,
        string mapName = DefaultMapName,
        bool persist = true)
    {
        InputAction action = FindAction(mapName, actionName);
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;
        if (action.bindings[bindingIndex].isComposite) return;

        EnsureLoaded();
        InputBinding binding = action.bindings[bindingIndex];
        binding.overrideInteractions = ToInteractionString(type);
        action.ApplyBindingOverride(bindingIndex, binding);
        if (persist)
            Save();
    }

    public string GetDisplayString(string actionName, int bindingIndex, string mapName = DefaultMapName)
    {
        InputAction action = FindAction(mapName, actionName);
        if (action == null) return "Not Set";
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return "Not Set";

        if (TryGetOneModifier(action, bindingIndex, out OneModifierBinding chord))
            return GetChordDisplay(action, chord);

        InputBinding binding = action.bindings[bindingIndex];
        string path = binding.effectivePath ?? binding.path;
        if (string.IsNullOrEmpty(path))
            return "Not Set";

        return InputRebind.FormatKeyDisplay(path);
    }

    public void StartRebind(
        string actionName,
        int bindingIndex,
        Action<InputBindingInfo> onComplete = null,
        Action onCancel = null,
        string mapName = DefaultMapName,
        Action<string> onPreview = null)
    {
        CancelRebind();

        InputAction action = FindAction(mapName, actionName);
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;
        if (action.bindings[bindingIndex].isComposite) return;

        EnsureLoaded();

        bool isChord = TryGetOneModifier(action, bindingIndex, out OneModifierBinding chord);
        bool expectAxis = action.type == InputActionType.Value
            && string.Equals(action.expectedControlType, "Axis", StringComparison.OrdinalIgnoreCase);

        _wasEnabled = action.enabled;
        _disabledAction = action;
        if (_wasEnabled)
            action.Disable();

        _rebinding = true;

        InputRebind.StartCapture(
            result =>
            {
                if (string.IsNullOrEmpty(result.KeyPath))
                {
                    FinishRebindCancel(onCancel);
                    return;
                }

                if (isChord)
                    ApplyChordResult(action, chord, result);
                else
                    ApplySimpleResult(action, bindingIndex, result);

                CleanupRebindState();
                Save();

                int infoIndex = isChord ? chord.BindingIndex : bindingIndex;
                InputBindingInfo info = new InputBindingInfo(
                    mapName,
                    actionName,
                    infoIndex,
                    GetDisplayString(actionName, infoIndex, mapName),
                    false,
                    action.bindings[infoIndex].isPartOfComposite,
                    isChord,
                    isChord ? chord.ModifierIndex : -1,
                    isChord ? chord.CompositeIndex : -1);

                BindingsChanged?.Invoke();
                onComplete?.Invoke(info);
            },
            onCancel: () => FinishRebindCancel(onCancel),
            expectAxis: expectAxis,
            onPreview: onPreview);
    }

    public void CancelRebind()
    {
        if (InputRebind.IsCapturing)
        {
            InputRebind.CancelCapture();
            return;
        }

        CleanupRebindState();
    }

    public static bool TryGetOneModifier(InputAction action, int bindingIndex, out OneModifierBinding chord)
    {
        chord = default;
        if (action == null) return false;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return false;

        InputBinding binding = action.bindings[bindingIndex];
        if (!binding.isPartOfComposite && !binding.isComposite) return false;

        int compositeIndex = binding.isComposite
            ? bindingIndex
            : FindCompositeRootIndex(action, bindingIndex);
        if (compositeIndex < 0) return false;

        InputBinding composite = action.bindings[compositeIndex];
        if (!IsOneModifierComposite(composite)) return false;

        int modifierIndex = -1;
        int partBindingIndex = -1;
        for (int i = compositeIndex + 1; i < action.bindings.Count; i++)
        {
            InputBinding part = action.bindings[i];
            if (!part.isPartOfComposite) break;

            if (string.Equals(part.name, "modifier", StringComparison.OrdinalIgnoreCase))
                modifierIndex = i;
            else if (string.Equals(part.name, "binding", StringComparison.OrdinalIgnoreCase))
                partBindingIndex = i;
        }

        if (modifierIndex < 0 || partBindingIndex < 0) return false;

        chord = new OneModifierBinding(compositeIndex, modifierIndex, partBindingIndex);
        return true;
    }

    static bool IsOneModifierComposite(InputBinding composite)
    {
        if (!composite.isComposite) return false;
        string path = composite.path ?? string.Empty;
        string name = composite.name ?? string.Empty;
        return path.IndexOf("OneModifier", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("One Modifier", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("OneModifier", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static int FindCompositeRootIndex(InputAction action, int partIndex)
    {
        for (int i = partIndex - 1; i >= 0; i--)
        {
            if (action.bindings[i].isComposite)
                return i;
            if (!action.bindings[i].isPartOfComposite)
                break;
        }
        return -1;
    }

    static string GetChordDisplay(InputAction action, OneModifierBinding chord)
    {
        string modifierPath = GetEffectivePath(action.bindings[chord.ModifierIndex]);
        string keyPath = GetEffectivePath(action.bindings[chord.BindingIndex]);

        if (string.IsNullOrEmpty(modifierPath))
            return InputRebind.FormatKeyDisplay(keyPath);
        if (string.IsNullOrEmpty(keyPath))
            return InputRebind.FormatKeyDisplay(modifierPath);

        // Same path on both parts = plain key, not a chord.
        if (string.Equals(modifierPath, keyPath, StringComparison.OrdinalIgnoreCase))
            return InputRebind.FormatKeyDisplay(keyPath);

        return InputRebind.FormatChordDisplay(modifierPath, keyPath);
    }

    static string GetEffectivePath(InputBinding binding)
        => binding.effectivePath ?? binding.path;

    void ApplyChordResult(InputAction action, OneModifierBinding chord, InputRebindResult result)
    {
        if (result.HasModifier)
        {
            action.ApplyBindingOverride(chord.ModifierIndex, result.ModifierPath);
            action.ApplyBindingOverride(chord.BindingIndex, result.KeyPath);
            return;
        }

        // Plain key (incl. Ctrl/Alt/Shift alone): same path on both parts so OneModifier still fires.
        action.ApplyBindingOverride(chord.ModifierIndex, result.KeyPath);
        action.ApplyBindingOverride(chord.BindingIndex, result.KeyPath);
    }

    void ApplySimpleResult(InputAction action, int bindingIndex, InputRebindResult result)
    {
        // Simple slots cannot become OneModifier at runtime; use key only.
        // If capture was a chord, KeyPath is the non-modifier key.
        action.ApplyBindingOverride(bindingIndex, result.KeyPath);
    }

    void FinishRebindCancel(Action onCancel)
    {
        CleanupRebindState();
        onCancel?.Invoke();
    }

    void CleanupRebindState()
    {
        _rebinding = false;

        if (_disabledAction != null)
        {
            if (_wasEnabled && !_disabledAction.enabled)
                _disabledAction.Enable();
            _disabledAction = null;
        }

        _wasEnabled = false;
    }

    static BindingPressType ParsePressType(string interactions)
    {
        if (string.IsNullOrEmpty(interactions))
            return BindingPressType.Press;

        string value = interactions.Trim();
        if (value.StartsWith(HoldInteraction, StringComparison.OrdinalIgnoreCase))
            return BindingPressType.Hold;
        if (value.IndexOf("behavior=1", StringComparison.OrdinalIgnoreCase) >= 0
            || value.IndexOf("ReleaseOnly", StringComparison.OrdinalIgnoreCase) >= 0)
            return BindingPressType.Release;
        return BindingPressType.Press;
    }

    static string ToInteractionString(BindingPressType type)
    {
        return type switch
        {
            BindingPressType.Hold => HoldInteraction,
            BindingPressType.Release => ReleaseInteraction,
            _ => PressInteraction
        };
    }

    void RestoreHiddenLookAxes()
    {
        RestoreAxisBinding("Look X", "<Mouse>/delta/x");
        RestoreAxisBinding("Look Y", "<Mouse>/delta/y");
    }

    void RestoreAxisBinding(string actionName, string path)
    {
        InputAction action = FindAction(DefaultMapName, actionName);
        if (action == null) return;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (binding.isComposite || binding.isPartOfComposite) continue;

            action.RemoveBindingOverride(i);
            binding = action.bindings[i];
            if (!string.Equals(binding.effectivePath, path, StringComparison.Ordinal))
                action.ApplyBindingOverride(i, path);
        }
    }

    InputAction FindAction(string mapName, string actionName)
    {
        if (_actions == null) return null;
        InputActionMap map = _actions.FindActionMap(mapName, throwIfNotFound: false);
        return map?.FindAction(actionName, throwIfNotFound: false);
    }
}
