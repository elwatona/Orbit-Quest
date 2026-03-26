using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrbiterTargetsView : MonoBehaviour
{
    [SerializeField] Transform _container;
    [SerializeField] GameObject _rowPrefab;
    [SerializeField] GroupHeaderView _groupHeaderPrefab;

    private TransformOrbiter _orbiter;
    private readonly List<GameObject> _rows = new List<GameObject>();

    public void Bind(TransformOrbiter orbiter)
    {
        _orbiter = orbiter;
        Refresh();
    }

    public void Refresh()
    {
        DestroyRowInstances();

        if (_orbiter == null || _container == null || _rowPrefab == null) return;

        _orbiter.EnsureTargetsClean();

        bool worldPositionStays = false;

        if (_groupHeaderPrefab != null)
        {
            GroupHeaderView header = Object.Instantiate(_groupHeaderPrefab, _container, worldPositionStays);
            header.SetTitle("Targets");
            header.gameObject.SetActive(true);
            _rows.Add(header.gameObject);
        }

        int count = _orbiter.GetTargetCount();
        for (int i = 0; i < count; i++)
        {
            Transform t = _orbiter.GetTarget(i);
            string label = t != null ? t.name : "—";
            GameObject row = Object.Instantiate(_rowPrefab, _container, worldPositionStays);
            var tmp = row.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
                tmp.text = $"Target {i}: {label}";

            var rowView = row.GetComponent<TargetRowView>();
            if (rowView == null)
                rowView = row.AddComponent<TargetRowView>();

            int index = i;
            rowView.Setup(index, _ => { _orbiter.RemoveTargetAt(index); Refresh(); });

            row.SetActive(true);
            _rows.Add(row);
        }

        if (_container is RectTransform contentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    public void Clear()
    {
        _orbiter = null;
        DestroyRowInstances();
    }

    void DestroyRowInstances()
    {
        if (_container == null)
        {
            foreach (GameObject go in _rows)
            {
                if (go == null) continue;
                UiDestroyRaycastHelper.DeactivateStripAndDestroy(go);
            }
            _rows.Clear();
            return;
        }

        bool shouldToggleContainer = _container.gameObject.activeInHierarchy;
        bool previousSelf = _container.gameObject.activeSelf;
        if (shouldToggleContainer)
            _container.gameObject.SetActive(false);

        try
        {
            foreach (GameObject go in _rows)
            {
                if (go == null) continue;
                UiDestroyRaycastHelper.DeactivateStripAndDestroy(go);
            }

            _rows.Clear();
        }
        finally
        {
            if (shouldToggleContainer && _container != null)
                _container.gameObject.SetActive(previousSelf);
        }
    }
}
