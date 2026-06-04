using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SelectBaseSlot : DraggableSlot
{
    [SerializeField] protected GameObject ghostPrefab;

    protected string _id = "";
    private CycleSlot _ghostSlot;
    private int _targetIndex = -1;

    protected abstract Sprite GhostSkillIcon { get; }
    protected abstract ControlType GhostControlType { get; }
    protected abstract Sprite GhostHeadIcon { get; }
    protected abstract string GhostName { get; }

    protected override void OnSlotPointerDown(PointerEventData eventData)
    {
        WaitHoldThen(eventData, () => _ghostSlot = CreateSlot(eventData));
    }

    protected override void OnSlotPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (_ghostSlot == null && !eventData.dragging)
        {
            UIManager.Instance.GetUI<UI_Result>().cyclePanel.AddSlotToLast(CreateSlot(eventData));
        }

        if (_ghostSlot != null)
        {
            _targetIndex = _ghostSlot.GetPlaceHolderIndex();
            _ghostSlot.ClearPlaceHolder();

            if (!TryDropOnCycleLayouts(_ghostSlot, _targetIndex, eventData))
            {
                Destroy(_ghostSlot.gameObject);
            }

            _ghostSlot = null;
        }
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        TryBeginPanelScrollDrag(eventData, _ghostSlot != null);
    }

    protected override void OnSlotDrag(PointerEventData eventData)
    {
        if (_ghostSlot != null)
        {
            _ghostSlot.Drag(eventData);
            return;
        }

        ForwardPanelScrollDrag(eventData, _ghostSlot != null);
    }

    protected override void OnSlotEndDrag(PointerEventData eventData)
    {
        if (EndPanelScrollDrag(eventData))
        {
            return;
        }

        CancelHold();
    }

    protected CycleSlot CreateSlot(PointerEventData eventData)
    {
        CycleSlot slot = Instantiate(ghostPrefab, GetGhostParent())
            .GetComponent<CycleSlot>();
        
        slot.name = $"Slot_{GhostHeadIcon.name}_{GhostName}";
        slot.SetSlot(GhostSkillIcon, GhostControlType, GhostHeadIcon, GhostName);
        slot.SetPositionToPointer(eventData);

        return slot;
    }
}
