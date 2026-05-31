using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArrowEndSlot : SelfGhostSlot
{
    protected override bool ProcessDrop(PointerEventData eventData)
    {
        return TryRaycastCycleSlot(eventData, cycleSlot =>
        {
            var arrowOverlayPanel = UIManager.Instance.GetUI<UI_Result>().cyclePanel.arrow;
            if (arrowOverlayPanel != null)
            {
                arrowOverlayPanel.HandleMarkerDropped(cycleSlot, false);
            }
        });
    }
}
