using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

public enum ControlsCategory
{
    Orb,
    Camera,
    Editor
}

public class Controls : IPanel
{
    static readonly ControlsCategory[] CategoryOrder =
    {
        ControlsCategory.Orb,
        ControlsCategory.Camera,
        ControlsCategory.Editor
    };

    public GameObject Root { get; private set; }

    readonly InputBindingsService _bindingsService;
    readonly InputActionAsset _actions;
    readonly ScrollComponent _scroll;
    readonly GameObject _titlePrefab;
    readonly List<BindingRow> _rows = new List<BindingRow>();

    public InputBindingsService Bindings => _bindingsService;

    public Controls(ControlsDependencies dependencies)
    {
        Root = dependencies.Root;
        _actions = ResolveLiveActions(dependencies.Actions);
        _titlePrefab = dependencies.TitlePrefab;

        if (_actions == null)
            Debug.LogError("Controls requires PlayerInput.actions or ControlsDependencies.Actions.");
        if (dependencies.BindingRowPrefab == null)
            Debug.LogError("ControlsDependencies.BindingRowPrefab is required.");
        if (dependencies.TitlePrefab == null)
            Debug.LogError("ControlsDependencies.TitlePrefab is required.");
        if (dependencies.BindingsScroll == null)
            Debug.LogError("ControlsDependencies.BindingsScroll is required.");

        _bindingsService = new InputBindingsService(_actions);
        if (_actions != null)
            _bindingsService.EnsureLoaded();

        _scroll = new ScrollComponent(dependencies.BindingsScroll, dependencies.BindingRowPrefab);
        _bindingsService.BindingsChanged += RefreshRows;
    }

    public void Toggle(bool active)
    {
        if (!active)
            _bindingsService.CancelRebind();
        else
            RefreshRows();

        Root.SetActive(active);
    }

    public IReadOnlyList<InputBindingInfo> GetBindings()
        => _bindingsService.GetPlayerBindings();

    public void BeginRebind(
        string actionName,
        int bindingIndex,
        Action<InputBindingInfo> onComplete = null,
        Action onCancel = null,
        Action<string> onPreview = null)
    {
        _bindingsService.StartRebind(
            actionName,
            bindingIndex,
            onComplete,
            onCancel,
            onPreview: onPreview);
    }

    public void ResetBinding(string actionName, int bindingIndex)
    {
        _bindingsService.ResetToDefault(actionName, bindingIndex);
        _bindingsService.Save();
    }

    public void SaveBindings() => _bindingsService.Save();

    public void ResetAllBindings() => _bindingsService.ResetAll();

    void RefreshRows()
    {
        _scroll.ClearItems();
        _rows.Clear();

        IReadOnlyList<InputBindingInfo> all = GetBindings();

        foreach (ControlsCategory category in CategoryOrder)
        {
            List<InputBindingInfo> bindings = FilterByCategory(all, category);
            if (bindings.Count == 0) continue;

            string categoryTitle = category.ToString();
            _scroll.AddItem(_titlePrefab, t =>
            {
                var header = new TitleHeader(t);
                header.SetTitle(categoryTitle);
                return header;
            });

            List<BindingRowEntry> entries = BuildRowEntries(bindings);
            foreach (BindingRowEntry entry in entries)
            {
                BindingRow row = _scroll.AddItem(t => new BindingRow(t));
                bool isLook = entry.Primary.ActionName == "Look";
                BindingPressType pressType = isLook
                    ? BindingPressType.Hold
                    : _bindingsService.GetPressType(
                        entry.Primary.ActionName,
                        entry.Primary.BindingIndex,
                        entry.Primary.MapName);

                row.Bind(
                    entry.Primary,
                    entry.Alt,
                    ResolveTitle(entry.Primary),
                    ResolveKeyDisplay(entry.Primary),
                    entry.Alt.HasValue ? ResolveKeyDisplay(entry.Alt.Value) : "Not Set",
                    () => OnRebind(row, alt: false),
                    () => OnRebind(row, alt: true),
                    () => OnReset(row),
                    pressType,
                    showPressType: true,
                    showAlt: true,
                    isLook ? null : value => OnPressTypeChanged(row, value),
                    pressTypeLocked: isLook);
                _rows.Add(row);
            }
        }
    }

    static List<BindingRowEntry> BuildRowEntries(List<InputBindingInfo> bindings)
    {
        var entries = new List<BindingRowEntry>();
        InputBindingInfo? pendingPrimary = null;
        string pendingKey = null;
        var pendingParts = new Dictionary<string, InputBindingInfo>(StringComparer.Ordinal);

        foreach (InputBindingInfo info in bindings)
        {
            if (info.IsPartOfComposite && !info.IsModifierChord)
            {
                FlushPendingPrimary(entries, ref pendingPrimary, ref pendingKey);

                string partKey = info.MapName + "/" + info.ActionName + "/" + (info.PartName ?? string.Empty);
                if (pendingParts.TryGetValue(partKey, out InputBindingInfo primaryPart))
                {
                    entries.Add(new BindingRowEntry(primaryPart, info));
                    pendingParts.Remove(partKey);
                }
                else
                {
                    pendingParts[partKey] = info;
                }
                continue;
            }

            string key = info.MapName + "/" + info.ActionName;
            if (!pendingPrimary.HasValue)
            {
                pendingPrimary = info;
                pendingKey = key;
                continue;
            }

            if (pendingKey == key)
            {
                entries.Add(new BindingRowEntry(pendingPrimary.Value, info));
                pendingPrimary = null;
                pendingKey = null;
                continue;
            }

            FlushPendingPrimary(entries, ref pendingPrimary, ref pendingKey);
            pendingPrimary = info;
            pendingKey = key;
        }

        FlushPendingPrimary(entries, ref pendingPrimary, ref pendingKey);
        foreach (KeyValuePair<string, InputBindingInfo> pair in pendingParts)
            entries.Add(new BindingRowEntry(pair.Value, null));

        return entries;
    }

    static void FlushPendingPrimary(
        List<BindingRowEntry> entries,
        ref InputBindingInfo? pendingPrimary,
        ref string pendingKey)
    {
        if (!pendingPrimary.HasValue) return;
        entries.Add(new BindingRowEntry(pendingPrimary.Value, null));
        pendingPrimary = null;
        pendingKey = null;
    }

    readonly struct BindingRowEntry
    {
        public BindingRowEntry(InputBindingInfo primary, InputBindingInfo? alt)
        {
            Primary = primary;
            Alt = alt;
        }

        public InputBindingInfo Primary { get; }
        public InputBindingInfo? Alt { get; }
    }

    static List<InputBindingInfo> FilterByCategory(
        IReadOnlyList<InputBindingInfo> bindings,
        ControlsCategory category)
    {
        var filtered = new List<InputBindingInfo>();
        foreach (InputBindingInfo info in bindings)
        {
            if (GetCategory(info.ActionName) == category)
                filtered.Add(info);
        }
        return filtered;
    }

    static ControlsCategory GetCategory(string actionName)
    {
        switch (actionName)
        {
            case "Zoom":
            case "Look":
                return ControlsCategory.Camera;
            case "Spawn Planet":
            case "Spawn Asteroid":
            case "Spawn Sun":
            case "Set Spawn Point":
            case "Enter Edition":
            case "Toggle Play Mode":
            case "Toggle Controls":
            case "Toggle Player Data":
            case "Toggle Console":
            case "Toggle Menu":
                return ControlsCategory.Editor;
            default:
                return ControlsCategory.Orb;
        }
    }

    void OnRebind(BindingRow row, bool alt)
    {
        if (alt && !row.AltInfo.HasValue)
            return;

        InputBindingInfo info = alt
            ? row.AltInfo.Value
            : row.Info;

        row.SetListening(true, alt);
        BeginRebind(
            info.ActionName,
            info.BindingIndex,
            onComplete: _ => row.SetListening(false, alt),
            onCancel: () => row.SetListening(false, alt),
            onPreview: preview => row.SetListeningPreview(preview, alt));
    }

    void OnReset(BindingRow row)
    {
        _bindingsService.ResetToDefault(row.Info.ActionName, row.Info.BindingIndex, row.Info.MapName);
        if (row.AltInfo.HasValue)
        {
            InputBindingInfo alt = row.AltInfo.Value;
            _bindingsService.ResetToDefault(alt.ActionName, alt.BindingIndex, alt.MapName);
        }
        _bindingsService.Save();
    }

    void OnPressTypeChanged(BindingRow row, int value)
    {
        if (value < 0 || value > (int)BindingPressType.Release) return;
        BindingPressType type = (BindingPressType)value;

        bool hasAlt = row.AltInfo.HasValue;
        _bindingsService.SetPressType(
            row.Info.ActionName,
            row.Info.BindingIndex,
            type,
            row.Info.MapName,
            persist: !hasAlt);

        if (hasAlt)
        {
            InputBindingInfo alt = row.AltInfo.Value;
            _bindingsService.SetPressType(
                alt.ActionName,
                alt.BindingIndex,
                type,
                alt.MapName,
                persist: true);
        }
    }

    string ResolveTitle(InputBindingInfo info)
    {
        switch (info.ActionName)
        {
            case "Move":
                return ResolveMoveTitle(info);
            case "Zoom":
                return ResolveZoomTitle(info);
        }

        return info.ActionName;
    }

    string ResolveKeyDisplay(InputBindingInfo info)
    {
        if (info.IsModifierChord)
            return _bindingsService.GetDisplayString(info.ActionName, info.BindingIndex, info.MapName);

        string path = GetBindingPath(info);
        if (string.IsNullOrEmpty(path))
            return "Not Set";

        return InputRebind.FormatKeyDisplay(path);
    }

    string ResolveMoveTitle(InputBindingInfo info)
    {
        return (info.PartName ?? GetBindingPartName(info)) switch
        {
            "up" => "Forward",
            "down" => "Back",
            "left" => "Left",
            "right" => "Right",
            _ => info.ActionName
        };
    }

    string ResolveZoomTitle(InputBindingInfo info)
    {
        return (info.PartName ?? GetBindingPartName(info)) switch
        {
            "positive" => "Zoom In",
            "negative" => "Zoom Out",
            _ => info.ActionName
        };
    }

    string GetBindingPartName(InputBindingInfo info)
    {
        InputBinding? binding = GetBinding(info);
        return binding?.name;
    }

    string GetBindingPath(InputBindingInfo info)
    {
        InputBinding? binding = GetBinding(info);
        return binding?.effectivePath ?? binding?.path;
    }

    InputBinding? GetBinding(InputBindingInfo info)
    {
        if (_actions == null) return null;

        InputActionMap map = _actions.FindActionMap(info.MapName, throwIfNotFound: false);
        InputAction action = map?.FindAction(info.ActionName, throwIfNotFound: false);
        if (action == null) return null;
        if (info.BindingIndex < 0 || info.BindingIndex >= action.bindings.Count) return null;

        return action.bindings[info.BindingIndex];
    }

    static InputActionAsset ResolveLiveActions(InputActionAsset fallback)
    {
        PlayerInput playerInput = UnityEngine.Object.FindFirstObjectByType<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
            return playerInput.actions;
        return fallback;
    }
}

[Serializable]
public class ControlsDependencies : PanelDependencies
{
    public InputActionAsset Actions;
    public Transform BindingsScroll;
    public GameObject BindingRowPrefab;
    public GameObject TitlePrefab;
}
