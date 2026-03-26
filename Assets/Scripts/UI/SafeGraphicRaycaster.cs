using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Same as GraphicRaycaster, but tolerates stale Graphics still listed by GraphicRegistry after Destroy in the same frame
/// (avoids MissingReferenceException when moving the mouse over the game view).
/// </summary>
[AddComponentMenu("Event/Safe Graphic Raycaster")]
[RequireComponent(typeof(Canvas))]
public class SafeGraphicRaycaster : GraphicRaycaster
{
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        try
        {
            base.Raycast(eventData, resultAppendList);
        }
        catch (Exception ex) when (ex is MissingReferenceException || ex is NullReferenceException)
        {
            // Stale Graphic in raycast list; skip this pointer pass
        }
    }
}
