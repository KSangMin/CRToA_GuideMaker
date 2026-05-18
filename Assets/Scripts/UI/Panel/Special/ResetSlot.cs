using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResetSlot : Slot
{
    [SerializeField] private GameObject ghostObject;

    private ScrollRect _panelScroll;
    private bool _isDraggingScroll;

    private GameObject _ghost;
    private RectTransform _ghostRect;

    public void SetSlot(ScrollRect panelScroll)
    {
        _panelScroll = panelScroll;
    }

    protected override void OnSlotPointerDown(PointerEventData eventData)
    {
        holdCanceled = false;
        holdCoroutine = StartCoroutine(WaitHoldThen(eventData, _ => CreateGhostAfterHold(eventData)));
    }

    private void CreateGhostAfterHold(PointerEventData eventData)
    {
        InstantiateGhostUnderPanel(ghostObject, eventData, out _ghost, out _ghostRect);
    }

    protected override void OnSlotPointerUp(PointerEventData eventData)
    {
        ResolveClickOrDragDropPointerUp(eventData);
    }

    protected override void BeforeSlotPointerUp(PointerEventData eventData)
    {
        CancelHoldTracking();
    }

    protected override bool IsDragDropActive()
    {
        return _ghost != null;
    }

    protected override void OnSlotDragDrop(PointerEventData eventData)
    {
        ProcessDrop(eventData);
        DestroyGhostRoot(ref _ghost, ref _ghostRect);
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        TryBeginPanelScrollDragUnlessGhost(eventData, _panelScroll, ref _isDraggingScroll, _ghost != null);
    }

    protected override void OnSlotDrag(PointerEventData eventData)
    {
        ResolveDragGhostOrPanelScroll(eventData, _ghostRect, _panelScroll, ref _isDraggingScroll, _ghost != null);
    }

    protected override void OnSlotEndDrag(PointerEventData eventData)
    {
        ResolveEndDragPanelScrollOrCancelHold(eventData, _panelScroll, ref _isDraggingScroll);
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("CycleSlot"))
            {
                CycleSlot cs = result.gameObject.GetComponent<CycleSlot>();
                cs.ResetSlot();
                return true;
            }
        }

        return false;
    }
}
