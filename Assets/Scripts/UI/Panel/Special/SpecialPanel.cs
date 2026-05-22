using UnityEngine;
using UnityEngine.UI;

public class SpecialPanel : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ResetSlot resetSlot;
    [SerializeField] private CountSlot countSlot;
    [SerializeField] private CommentSlot commentSlot;
    [SerializeField] private AreaStartSlot areaStartSlot;
    [SerializeField] private AreaEndSlot areaEndSlot;

    private void Start()
    {
        resetSlot.SetSlot(scrollRect);
        countSlot.SetSlot(scrollRect);
        commentSlot.SetSlot(scrollRect);
        areaStartSlot.SetSlot(scrollRect);
        areaEndSlot.SetSlot(scrollRect);
    }
}
