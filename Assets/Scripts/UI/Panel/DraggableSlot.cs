using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect 전달과 Cycle 레이아웃에 대한 드롭 처리 공통 로직.
/// </summary>
public class DraggableSlot : Slot
{
    private readonly List<RaycastResult> _raycastBuffer = new();

    private bool _forwardingScroll;
    private ScrollRect _forwardScrollRect;

    protected static bool TryDropOnCycleLayouts(CycleSlot slot, int targetIndex, PointerEventData eventData,
        List<RaycastResult> resultsBuffer)
    {
        if (slot == null)
        {
            return false;
        }

        resultsBuffer.Clear();
        EventSystem.current.RaycastAll(eventData, resultsBuffer);

        foreach (RaycastResult result in resultsBuffer)
        {
            if (result.gameObject.CompareTag("CycleHorizontalLayout"))
            {
                CycleHorizontalLayout hz = result.gameObject.GetComponent<CycleHorizontalLayout>();
                hz.AddSlotAndCheckExplode(slot, targetIndex);
                return true;
            }

            if (result.gameObject.CompareTag("CyclePanel"))
            {
                CyclePanel cyclePanel = result.gameObject.GetComponent<CyclePanel>();
                cyclePanel.AddSlotToNewRow(slot);
                return true;
            }
        }

        return false;
    }

    protected bool TryDropOnCycleLayouts(CycleSlot slot, int targetIndex, PointerEventData eventData)
    {
        return TryDropOnCycleLayouts(slot, targetIndex, eventData, _raycastBuffer);
    }

    /// <summary>
    /// 서브클래스에서 선행 처리 후 호출: ScrollRect 전달 또는 <see cref="OnDraggableBeginDrag"/>로 분기.
    /// </summary>
    protected void TryForwardScrollOrDraggable(PointerEventData eventData)
    {
        if (ShouldForwardScrollDrag(eventData))
        {
            _forwardScrollRect = ResolveScrollRectForDrag(eventData);
            if (_forwardScrollRect != null)
            {
                _forwardingScroll = true;
                OnBeforeScrollDragForwarded(eventData);
                _forwardScrollRect.OnBeginDrag(eventData);
                return;
            }
        }

        _forwardingScroll = false;
        _forwardScrollRect = null;
        OnDraggableBeginDrag(eventData);
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        TryForwardScrollOrDraggable(eventData);
    }

    protected override void OnSlotDrag(PointerEventData eventData)
    {
        if (_forwardingScroll && _forwardScrollRect != null)
        {
            _forwardScrollRect.OnDrag(eventData);
            return;
        }

        OnDraggableDrag(eventData);
    }

    protected override void OnSlotEndDrag(PointerEventData eventData)
    {
        if (_forwardingScroll && _forwardScrollRect != null)
        {
            _forwardScrollRect.OnEndDrag(eventData);
            _forwardingScroll = false;
            _forwardScrollRect = null;
            return;
        }

        OnDraggableEndDrag(eventData);
    }

    protected virtual bool ShouldForwardScrollDrag(PointerEventData eventData) => false;

    protected virtual ScrollRect ResolveScrollRectForDrag(PointerEventData eventData) => null;

    protected virtual void OnBeforeScrollDragForwarded(PointerEventData eventData) { }

    protected virtual void OnDraggableBeginDrag(PointerEventData eventData) { }

    protected virtual void OnDraggableDrag(PointerEventData eventData) { }

    protected virtual void OnDraggableEndDrag(PointerEventData eventData) { }
}
