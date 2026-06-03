public class ChargeCountSlot : BaseCountSlot
{
    protected override void ApplyToCycleSlot(CycleSlot cycleSlot, int count, int maxCount)
    {
        cycleSlot.SetChargeCount(count, maxCount);
    }
}
