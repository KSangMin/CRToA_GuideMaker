using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CycleHorizontalLayout : MonoBehaviour
{
    private int _id = -1;
    private int _maxSlotCount;
    private CycleVerticalLayout _cycleVerticalLayout;
    private List<CycleSlot> _slots = new();

    [SerializeField] private Button addRowButton;

    private void Awake()
    {
        //addRowButton.onClick.AddListener(AddRow);

        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Init(int id, CycleVerticalLayout vert, int maxSlotCount)
    {
        SetId(id);
        _cycleVerticalLayout = vert;
        _maxSlotCount = maxSlotCount;
    }

    public void SetId(int id)
    {
        _id = id;
    }

    private void AddSlot(CycleSlot slot, int index = -1)
    {
        _slots.Add(slot);
        slot.transform.SetParent(transform);

        if (index != -1)
        {
            slot.transform.SetSiblingIndex(index);
        }

        slot.originalParent = slot.transform.parent;

        _cycleVerticalLayout.CheckRowEmpty();
    }

    public void AddSlotJust(CycleSlot slot, int index = -1)
    {
        if (_slots.Count >= _maxSlotCount)
        {
            CycleHorizontalLayout row = _cycleVerticalLayout.CreateRow();
            row.AddSlotJust(slot);
            return;
        }

        AddSlot(slot, index);
    }

    public void AddSlotAndCheckExplode(CycleSlot slot, int index = -1)
    {
        AddSlot(slot, index);

        if (_slots.Count > _maxSlotCount)
        {
            CycleHorizontalLayout row = _cycleVerticalLayout.GetRow(_id + 1);
            CycleSlot lastSlot = transform.GetChild(transform.childCount - 1).GetComponent<CycleSlot>();
            RemoveFromHorizontalLayout(lastSlot);
            row.AddSlotAndCheckExplode(lastSlot, 0);
        }
    }

    public void RemoveFromHorizontalLayout(CycleSlot slot)
    {
        _slots.Remove(slot);
    }

    public void ReBuildLayout()
    {
        _cycleVerticalLayout.RebuildLayout();
    }
}
