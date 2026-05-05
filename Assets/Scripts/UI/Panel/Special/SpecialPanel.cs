using UnityEngine;
using UnityEngine.UI;

public class SpecialPanel : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ResetSlot resetSlot;
    [SerializeField] private CountSlot countSlot;

    public void SetPanel(ScrollRect menuScroll)
    {
        scrollRect.GetComponent<PanelScrollRect>().Init(menuScroll);

        resetSlot.SetSlot(scrollRect);
        countSlot.SetSlot(scrollRect);
    }
}
