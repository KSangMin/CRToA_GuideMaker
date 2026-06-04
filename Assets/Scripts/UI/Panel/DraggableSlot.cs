using UnityEngine.EventSystems;

public abstract class DraggableSlot : Slot
{
    #region Protected Methods

    protected static bool TryDropOnCycleLayouts(CycleSlot slot, int targetIndex, PointerEventData eventData)
    {
        RaycastAll(eventData);

        foreach (RaycastResult result in RaycastBuffer)
        {
            if (result.gameObject.CompareTag("CycleHorizontalLayout"))
            {
                CycleHorizontalLayout horizontalLayout = result.gameObject.GetComponent<CycleHorizontalLayout>();
                horizontalLayout.AddSlotAndCheckExplode(slot, targetIndex);
                return true;
            }
            else if (result.gameObject.CompareTag("CyclePanel"))
            {
                CyclePanel cyclePanel = result.gameObject.GetComponent<CyclePanel>();
                cyclePanel.AddSlotToNewRow(slot);
                return true;
            }
        }

        return false;
    }

    #endregion
}
