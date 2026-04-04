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

    public void AddSlot(CycleSlot slot)
    {
        if (_slots.Count >= _maxSlotCount)
        {
            CycleHorizontalLayout row = _cycleVerticalLayout.CreateRow();
            row.AddSlot(slot);
        }
        else
        {
            _slots.Add(slot);
            slot.transform.SetParent(transform);
        }
    }

    public void AddRow()
    {
        _cycleVerticalLayout.CreateRow(_id + 1);
    }
}
