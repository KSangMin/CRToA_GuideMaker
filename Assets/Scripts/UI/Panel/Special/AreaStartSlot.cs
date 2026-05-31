using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AreaStartSlot : SelfGhostSlot
{
    protected override bool ProcessDrop(PointerEventData eventData)
    {
        return TryRaycastCycleSlot(eventData, cycleSlot =>
        {
            // 다중 마커 중첩 허용: 기존 영역을 초기화하지 않음

            var areaOverlayPanel = UIManager.Instance.GetUI<UI_Result>().cyclePanel.area;
            if (areaOverlayPanel != null)
            {
                areaOverlayPanel.HandleMarkerDropped(cycleSlot, true);
            }
        });
    }
}
