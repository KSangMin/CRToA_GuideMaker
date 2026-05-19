using UnityEngine.EventSystems;

public class ResetSlot : SelfGhostSlot
{
    #region Protected Methods

    protected override bool ProcessDrop(PointerEventData eventData)
    {
        return TryRaycastCycleSlot(eventData, cycleSlot => cycleSlot.ResetSlot());
    }

    #endregion
}
