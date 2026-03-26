using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelController
{
    readonly Transform _container;
    readonly PropertyRow _rowPrefab;
    readonly GroupHeaderView _groupHeaderPrefab;
    readonly List<GameObject> _instantiated = new();

    public PanelController(Transform container, PropertyRow rowPrefab, GroupHeaderView groupHeaderPrefab = null)
    {
        _container = container;
        _rowPrefab = rowPrefab;
        _groupHeaderPrefab = groupHeaderPrefab;
    }

    public void Bind(IEditable target)
    {
        Bind(target.GetProperties());
    }

    public void Bind(List<PropertyDefinition> properties)
    {
        Clear();

        if (properties == null) return;

        string lastGroup = null;

        foreach (PropertyDefinition property in properties)
        {
            if (!string.IsNullOrEmpty(property.group) && property.group != lastGroup)
            {
                lastGroup = property.group;
                if (_groupHeaderPrefab != null)
                {
                    GroupHeaderView header = Object.Instantiate(_groupHeaderPrefab, _container);
                    header.SetTitle(property.group);
                    header.gameObject.SetActive(true);
                    _instantiated.Add(header.gameObject);
                }
            }

            PropertyRow row = Object.Instantiate(_rowPrefab, _container);
            row.Bind(property);
            row.gameObject.SetActive(true);
            _instantiated.Add(row.gameObject);
        }
    }

    public void Clear()
    {
        if (_container == null)
        {
            _instantiated.Clear();
            return;
        }

        // Use activeInHierarchy: when parent panel is inactive, children may still have activeSelf true.
        bool shouldToggleContainer = _container.gameObject.activeInHierarchy;
        bool previousSelf = _container.gameObject.activeSelf;
        if (shouldToggleContainer)
            _container.gameObject.SetActive(false);

        try
        {
            foreach (GameObject go in _instantiated)
            {
                if (go == null) continue;
                if (go.TryGetComponent(out PropertyRow row))
                    row.Unbind();
                UiDestroyRaycastHelper.DeactivateStripAndDestroy(go);
            }

            _instantiated.Clear();
        }
        finally
        {
            if (shouldToggleContainer && _container != null)
                _container.gameObject.SetActive(previousSelf);
        }
    }
}

/// <summary>Stops GraphicRaycaster from touching Graphics scheduled for Destroy (avoids MissingReferenceException).</summary>
public static class UiDestroyRaycastHelper
{
    public static void StripRaycastTargetsBeforeDestroy(GameObject go)
    {
        if (go == null) return;
        Graphic[] graphics = go.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic g = graphics[i];
            if (g != null)
                g.raycastTarget = false;
        }
    }

    /// <summary>Deactivate first so GraphicRaycaster drops the hierarchy branch before Destroy schedules teardown.</summary>
    public static void DeactivateStripAndDestroy(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        StripRaycastTargetsBeforeDestroy(go);
        Object.Destroy(go);
    }
}
