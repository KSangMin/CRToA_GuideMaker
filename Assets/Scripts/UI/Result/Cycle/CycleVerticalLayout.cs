using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CycleVerticalLayout : MonoBehaviour
{
    [Header("Setting")]
    [Range(1, 12)]
    [SerializeField] private int maxSlotCount = 8;
    [Header("References")]
    [SerializeField] private GameObject rowPanelPrefab;
    private List<CycleHorizontalLayout> _rows = new();
    private Transform _verticalLayout;

    private void Awake()
    {
        _verticalLayout = GetComponent<Transform>();
    }

    public void AddSlotToLast(CycleSlot slot)
    {
        CycleHorizontalLayout row = _rows.Count <= 0 ? CreateRow() : _rows.Last();
        row.AddSlot(slot);
        RebuildLayout();
    }

    public CycleHorizontalLayout CreateRow(int id = -1)
    {
        if(id == -1)
        {
            id = _rows.Count;
        }
        CycleHorizontalLayout row = Instantiate(rowPanelPrefab, _verticalLayout).GetComponent<CycleHorizontalLayout>();
        row.Init(id, this, maxSlotCount);
        InsertRow(id, row);

        return row;
    }

    public void InsertRow(int id, CycleHorizontalLayout row)
    {
        _rows.Insert(id, row);
        for(int i = 0; i < _rows.Count; i++)
        {
            _rows[i].SetId(i);
        }
    }

    public void CheckRowEmpty()
    {
        for (int i = _rows.Count - 1; i >= 0; i--)
        {
            CycleHorizontalLayout row = _rows[i];
            if (row.transform.childCount <= 0)
            {
                _rows.RemoveAt(i);
                Destroy(row.gameObject);
            }
        }
    }

    public void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
