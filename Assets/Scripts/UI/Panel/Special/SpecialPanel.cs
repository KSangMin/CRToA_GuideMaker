using UnityEngine;
using UnityEngine.UI;

public class SpecialPanel : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ResetSlot resetSlot;
    [SerializeField] private CountSlot countSlot;

    private void Start()
    {
        resetSlot.SetSlot(scrollRect);
        countSlot.SetSlot(scrollRect);
    }
}
