using UnityEngine.EventSystems;

public class CommentSlot : SelfGhostSlot
{
    protected override bool ProcessDrop(PointerEventData eventData)
    {
        return TryRaycastCycleSlot(eventData, cycleSlot => cycleSlot.EnableComment());
    }
}
