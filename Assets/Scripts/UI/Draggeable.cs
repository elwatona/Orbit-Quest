using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Draggeable : MonoBehaviour, IDragHandler
{
    [FormerlySerializedAs("_uiManager")]
    [SerializeField] DeveloperToolsUI _developerTools;

    void Awake()
    {
        if (_developerTools == null)
            _developerTools = FindFirstObjectByType<DeveloperToolsUI>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_developerTools == null)
            _developerTools = FindFirstObjectByType<DeveloperToolsUI>();
        if (_developerTools == null || !_developerTools.IsAvailable || !_developerTools.IsDeveloperModeActive)
            return;
        transform.position = eventData.position;
    }
}
