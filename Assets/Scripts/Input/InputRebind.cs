using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public readonly struct InputRebindResult
{
    public InputRebindResult(string keyPath, string modifierPath = null)
    {
        KeyPath = keyPath;
        ModifierPath = modifierPath;
    }

    public string KeyPath { get; }
    public string ModifierPath { get; }
    public bool HasModifier => !string.IsNullOrEmpty(ModifierPath);
}

/// <summary>
/// Static rebind capture: confirms on key release; Ctrl/Alt/Shift alone is a key,
/// Ctrl/Alt/Shift + another key is a chord. No UI or persistence.
/// </summary>
public static class InputRebind
{
    static Action<InputRebindResult> _onComplete;
    static Action _onCancel;
    static Action<string> _onPreview;
    static bool _capturing;
    static bool _armed;
    static readonly HashSet<string> _gesturePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    static readonly HashSet<string> _downPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    static readonly HashSet<string> _suppressUntilRelease = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    static readonly List<string> _scratch = new List<string>();

    public static bool IsCapturing => _capturing;

    public static bool IsModifierPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        string p = path.ToLowerInvariant();
        return p.Contains("/leftctrl") || p.Contains("/rightctrl") || p.EndsWith("/ctrl")
            || p.Contains("/leftalt") || p.Contains("/rightalt") || p.EndsWith("/alt")
            || p.Contains("/leftshift") || p.Contains("/rightshift") || p.EndsWith("/shift");
    }

    public static string NormalizeModifierLabel(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        string p = path.ToLowerInvariant();
        if (p.Contains("ctrl")) return "Ctrl";
        if (p.Contains("alt")) return "Alt";
        if (p.Contains("shift")) return "Shift";
        return FormatControlLeaf(path);
    }

    public static string FormatKeyDisplay(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "Not Set";

        if (path.IndexOf("scroll/up", StringComparison.OrdinalIgnoreCase) >= 0)
            return "Scroll Up";
        if (path.IndexOf("scroll/down", StringComparison.OrdinalIgnoreCase) >= 0)
            return "Scroll Down";

        if (IsModifierPath(path))
            return NormalizeModifierLabel(path);

        return FormatControlLeaf(path);
    }

    public static string FormatChordDisplay(string modifierPath, string keyPath)
    {
        if (string.IsNullOrEmpty(modifierPath))
            return FormatKeyDisplay(keyPath);
        if (string.IsNullOrEmpty(keyPath))
            return FormatKeyDisplay(modifierPath);

        return NormalizeModifierLabel(modifierPath) + "+" + FormatKeyDisplay(keyPath);
    }

    public static string FormatCapturePreview(IReadOnlyCollection<string> pressedPaths)
    {
        InputRebindResult result = ResolveCapture(pressedPaths);
        if (string.IsNullOrEmpty(result.KeyPath))
            return "...";
        if (result.HasModifier)
            return FormatChordDisplay(result.ModifierPath, result.KeyPath);
        return FormatKeyDisplay(result.KeyPath);
    }

    public static InputRebindResult ResolveCapture(IReadOnlyCollection<string> pressedPaths)
    {
        if (pressedPaths == null || pressedPaths.Count == 0)
            return new InputRebindResult(null);

        string modifier = null;
        var nonModifiers = new List<string>();

        foreach (string path in pressedPaths)
        {
            if (string.IsNullOrEmpty(path)) continue;
            string resolved = ResolveScrollDirectionPath(path);
            if (IsModifierPath(resolved))
            {
                if (modifier == null)
                    modifier = PreferLeftModifier(resolved);
            }
            else
            {
                nonModifiers.Add(resolved);
            }
        }

        if (nonModifiers.Count > 0)
        {
            string key = PreferPrimaryKey(nonModifiers);
            if (modifier != null)
                return new InputRebindResult(key, modifier);
            return new InputRebindResult(key);
        }

        if (modifier != null)
            return new InputRebindResult(modifier);

        return new InputRebindResult(null);
    }

    public static void StartCapture(
        Action<InputRebindResult> onComplete,
        Action onCancel = null,
        bool expectAxis = false,
        Action<string> onPreview = null)
    {
        CancelCapture();

        _onComplete = onComplete;
        _onCancel = onCancel;
        _onPreview = onPreview;
        _ = expectAxis;
        _capturing = true;
        _armed = false;
        _gesturePaths.Clear();
        _downPaths.Clear();
        _suppressUntilRelease.Clear();

        _scratch.Clear();
        CollectDownControls(_scratch);
        for (int i = 0; i < _scratch.Count; i++)
            _suppressUntilRelease.Add(_scratch[i]);
        _armed = _suppressUntilRelease.Count == 0;

        _onPreview?.Invoke("...");
        InputSystem.onAfterUpdate += OnAfterUpdate;
    }

    public static void CancelCapture()
    {
        if (!_capturing) return;

        InputSystem.onAfterUpdate -= OnAfterUpdate;
        _capturing = false;
        _armed = false;
        _gesturePaths.Clear();
        _downPaths.Clear();
        _suppressUntilRelease.Clear();

        Action cancel = _onCancel;
        _onComplete = null;
        _onCancel = null;
        _onPreview = null;
        cancel?.Invoke();
    }

    static void CompleteCapture(InputRebindResult result)
    {
        if (!_capturing) return;

        InputSystem.onAfterUpdate -= OnAfterUpdate;
        _capturing = false;
        _armed = false;
        _gesturePaths.Clear();
        _downPaths.Clear();
        _suppressUntilRelease.Clear();

        Action<InputRebindResult> complete = _onComplete;
        _onComplete = null;
        _onCancel = null;
        _onPreview = null;
        complete?.Invoke(result);
    }

    static void OnAfterUpdate()
    {
        if (!_capturing) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelCapture();
            return;
        }

        _scratch.Clear();
        CollectDownControls(_scratch);

        _downPaths.Clear();
        for (int i = 0; i < _scratch.Count; i++)
            _downPaths.Add(_scratch[i]);

        if (!_armed)
        {
            _suppressUntilRelease.RemoveWhere(path => !_downPaths.Contains(path));
            if (_suppressUntilRelease.Count > 0)
                return;

            _armed = true;
            _gesturePaths.Clear();
            _onPreview?.Invoke("...");
        }

        foreach (string path in _downPaths)
            _gesturePaths.Add(path);

        EmitPreview();

        foreach (string path in _gesturePaths)
        {
            if (IsImmediateCapturePath(path))
            {
                CompleteCapture(ResolveCapture(_gesturePaths));
                return;
            }
        }

        if (_gesturePaths.Count > 0 && _downPaths.Count == 0)
            CompleteCapture(ResolveCapture(_gesturePaths));
    }

    static void EmitPreview()
    {
        if (_onPreview == null) return;

        if (_downPaths.Count > 0)
        {
            _onPreview.Invoke(FormatCapturePreview(_downPaths));
            return;
        }

        if (_gesturePaths.Count > 0)
        {
            _onPreview.Invoke(FormatCapturePreview(_gesturePaths));
            return;
        }

        _onPreview.Invoke("...");
    }

    static void CollectDownControls(List<string> into)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            foreach (InputControl control in keyboard.allControls)
            {
                if (control is not ButtonControl button) continue;
                if (!button.isPressed) continue;
                if (IsExcluded(control.path)) continue;
                if (control is KeyControl key && key.keyCode == Key.Escape) continue;
                into.Add(NormalizeControlPath(control));
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            TryAddButton(into, mouse.leftButton);
            TryAddButton(into, mouse.rightButton);
            TryAddButton(into, mouse.middleButton);
            TryAddButton(into, mouse.forwardButton);
            TryAddButton(into, mouse.backButton);

            float scrollY = mouse.scroll.ReadValue().y;
            if (scrollY > 0f)
                into.Add("<Mouse>/scroll/up");
            else if (scrollY < 0f)
                into.Add("<Mouse>/scroll/down");
        }
    }

    static void TryAddButton(List<string> into, ButtonControl button)
    {
        if (button == null || !button.isPressed) return;
        if (IsExcluded(button.path)) return;
        into.Add(NormalizeControlPath(button));
    }

    static bool IsImmediateCapturePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (path.IndexOf("scroll/up", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("scroll/down", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("/leftButton", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("/rightButton", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("/middleButton", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("/forwardButton", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("/backButton", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        return false;
    }

    static bool IsExcluded(string path)
    {
        if (string.IsNullOrEmpty(path)) return true;
        if (path.IndexOf("/anyKey", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("/position", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.IndexOf("/delta", StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (path.EndsWith("/scroll/y", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.EndsWith("/scroll/x", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    static string NormalizeControlPath(InputControl control)
    {
        if (control == null) return null;
        if (deviceHasName(control, out string layoutPath))
            return layoutPath;
        return control.path;
    }

    static bool deviceHasName(InputControl control, out string layoutPath)
    {
        layoutPath = null;
        InputDevice device = control.device;
        if (device == null || string.IsNullOrEmpty(control.name)) return false;
        if (device is Keyboard)
        {
            layoutPath = "<Keyboard>/" + control.name;
            return true;
        }
        if (device is Mouse)
        {
            layoutPath = "<Mouse>/" + control.name;
            return true;
        }
        return false;
    }

    static string ResolveScrollDirectionPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        if (path.EndsWith("/scroll/down", StringComparison.OrdinalIgnoreCase))
            return "<Mouse>/scroll/down";
        if (path.EndsWith("/scroll/up", StringComparison.OrdinalIgnoreCase))
            return "<Mouse>/scroll/up";
        return path;
    }

    static string PreferLeftModifier(string path)
    {
        string label = NormalizeModifierLabel(path);
        return label switch
        {
            "Ctrl" => "<Keyboard>/leftCtrl",
            "Alt" => "<Keyboard>/leftAlt",
            "Shift" => "<Keyboard>/leftShift",
            _ => path
        };
    }

    static string PreferPrimaryKey(List<string> keys)
    {
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].IndexOf("scroll/", StringComparison.OrdinalIgnoreCase) >= 0)
                return keys[i];
        }
        return keys[keys.Count - 1];
    }

    static string FormatControlLeaf(string path)
    {
        if (string.IsNullOrEmpty(path)) return "Not Set";

        int slash = path.LastIndexOf('/');
        string leaf = slash >= 0 && slash < path.Length - 1 ? path.Substring(slash + 1) : path;

        if (leaf.Equals("leftButton", StringComparison.OrdinalIgnoreCase)) return "LMB";
        if (leaf.Equals("rightButton", StringComparison.OrdinalIgnoreCase)) return "RMB";
        if (leaf.Equals("middleButton", StringComparison.OrdinalIgnoreCase)) return "MMB";

        if (leaf.Length == 1)
            return leaf.ToUpperInvariant();

        if (leaf.StartsWith("left", StringComparison.OrdinalIgnoreCase) && leaf.Length > 4)
            return "Left " + Capitalize(leaf.Substring(4));
        if (leaf.StartsWith("right", StringComparison.OrdinalIgnoreCase) && leaf.Length > 5)
            return "Right " + Capitalize(leaf.Substring(5));

        return Capitalize(leaf);
    }

    static string Capitalize(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length == 1) return value.ToUpperInvariant();
        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }
}
