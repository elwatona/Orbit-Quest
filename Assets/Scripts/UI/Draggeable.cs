using UnityEngine;
using UnityEngine.EventSystems;

public class Draggeable : MonoBehaviour, IDragHandler
{
    [SerializeField] UIManager _uiManager;

    void Awake()
    {
        if (_uiManager == null)
            _uiManager = FindObjectOfType<UIManager>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_uiManager == null)
            _uiManager = FindObjectOfType<UIManager>();
        if (_uiManager == null || !_uiManager.IsDeveloperMode)
            return;
        transform.position = eventData.position;
    }
}
