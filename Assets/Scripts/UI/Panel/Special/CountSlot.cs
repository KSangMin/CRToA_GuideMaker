public class CountSlot : BaseCountSlot
{
    protected override void ApplyToCycleSlot(CycleSlot cycleSlot, int count, int maxCount)
    {
        cycleSlot.SetSlotCount(count);
    }
}
