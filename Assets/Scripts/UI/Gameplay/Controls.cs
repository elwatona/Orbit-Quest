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
        _actions = dependencies.Actions;
        _titlePrefab = dependencies.TitlePrefab;

        if (dependencies.Actions == null)
            Debug.LogError("ControlsDependencies.Actions must reference the same InputActionAsset used by PlayerInput.");
        if (dependencies.BindingRowPrefab == null)
            Debug.LogError("ControlsDependencies.BindingRowPrefab is required.");
        if (dependencies.TitlePrefab == null)
            Debug.LogError("ControlsDependencies.TitlePrefab is required.");
        if (dependencies.BindingsScroll == null)
            Debug.LogError("ControlsDependencies.BindingsScroll is required.");

        _bindingsService = new InputBindingsService(dependencies.Actions);
        if (dependencies.Actions != null)
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
        Action onCancel = null)
    {
        _bindingsService.StartRebind(actionName, bindingIndex, onComplete, onCancel);
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

            foreach (InputBindingInfo info in bindings)
            {
                BindingRow row = _scroll.AddItem(t => new BindingRow(t));
                row.Bind(
                    info,
                    ResolveTitle(info),
                    ResolveKeyDisplay(info),
                    () => OnRebind(row),
                    () => OnReset(row));
                _rows.Add(row);
            }
        }
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
            case "Rotate":
                return ControlsCategory.Camera;
            case "Instantiate Astro":
            case "Set Spawn Point":
            case "Toggle Inspectors":
            case "Game State":
                return ControlsCategory.Editor;
            default:
                return ControlsCategory.Orb;
        }
    }

    void OnRebind(BindingRow row)
    {
        row.SetListening(true);
        BeginRebind(
            row.Info.ActionName,
            row.Info.BindingIndex,
            onComplete: info => row.SetKey(ResolveKeyDisplay(info)),
            onCancel: () => row.SetListening(false));
    }

    void OnReset(BindingRow row)
    {
        ResetBinding(row.Info.ActionName, row.Info.BindingIndex);
    }

    string ResolveTitle(InputBindingInfo info)
    {
        switch (info.ActionName)
        {
            case "Game State":
                return info.BindingIndex switch
                {
                    0 => "Edition",
                    1 => "Precision",
                    2 => "Contemplative",
                    _ => info.ActionName
                };
            case "Instantiate Astro":
                return info.BindingIndex switch
                {
                    0 => "Planet",
                    1 => "Asteroid",
                    2 => "Sun",
                    _ => info.ActionName
                };
            case "Toggle Inspectors":
                return info.BindingIndex switch
                {
                    0 => "Controls",
                    1 => "Player Data",
                    2 => "Console",
                    _ => info.ActionName
                };
            case "Move":
                return ResolveMoveTitle(info);
            case "Zoom":
                return ResolveZoomTitle(info);
            case "Rotate":
                return ResolveRotateTitle(info);
            default:
                return info.ActionName;
        }
    }

    string ResolveKeyDisplay(InputBindingInfo info)
    {
        string path = GetBindingPath(info);
        if (!string.IsNullOrEmpty(path))
        {
            if (path.IndexOf("scroll/up", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Scroll Up";
            if (path.IndexOf("scroll/down", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Scroll Down";
        }

        string display = info.DisplayName ?? string.Empty;
        if (display.IndexOf("Scroll Up", StringComparison.OrdinalIgnoreCase) >= 0
            || display.IndexOf("scroll/up", StringComparison.OrdinalIgnoreCase) >= 0)
            return "Scroll Up";
        if (display.IndexOf("Scroll Down", StringComparison.OrdinalIgnoreCase) >= 0
            || display.IndexOf("scroll/down", StringComparison.OrdinalIgnoreCase) >= 0)
            return "Scroll Down";

        return info.DisplayName;
    }

    string ResolveMoveTitle(InputBindingInfo info)
    {
        string partName = GetBindingPartName(info);
        return partName switch
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
        string partName = GetBindingPartName(info);
        return partName switch
        {
            "positive" => "Zoom In",
            "negative" => "Zoom Out",
            _ => info.ActionName
        };
    }

    string ResolveRotateTitle(InputBindingInfo info)
    {
        string partName = GetBindingPartName(info);
        return partName switch
        {
            "positive" => "Rotate Right",
            "negative" => "Rotate Left",
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
}

[Serializable]
public class ControlsDependencies : PanelDependencies
{
    public InputActionAsset Actions;
    public Transform BindingsScroll;
    public GameObject BindingRowPrefab;
    public GameObject TitlePrefab;
}
