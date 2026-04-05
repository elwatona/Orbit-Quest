using UnityEngine;
using UnityEngine.EventSystems;
public class DraggeableUI : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
}
