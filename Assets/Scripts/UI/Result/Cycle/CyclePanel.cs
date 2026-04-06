using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyclePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform verticalLayout;
    private CycleVerticalLayout _vert;

    private void Awake()
    {
        _vert = verticalLayout.GetComponent<CycleVerticalLayout>();
    }

    public void AddSlotToLast(CycleSlot slot)
    {
        _vert.AddSlotToLast(slot);
    }

    public void CheckRowEmpty()
    {
        _vert.CheckRowEmpty();
    }

    public void RebuildLayout()
    {
        _vert.RebuildLayout();
    }
}
