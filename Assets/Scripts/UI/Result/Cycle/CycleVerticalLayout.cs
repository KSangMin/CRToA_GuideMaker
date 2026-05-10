using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CycleVerticalLayout : MonoBehaviour
{
    [Header("Setting")]
    [Range(4, 12)]
    [SerializeField] private int maxSlotCount = 8;

    [Header("References")]
    [SerializeField] private GameObject rowPanelPrefab;
    [SerializeField] private IntEventChannel onRowLengthChangedEvent;

    private List<CycleHorizontalLayout> _rows = new();
    private Transform _verticalLayout;

    private void Awake()
    {
        _verticalLayout = GetComponent<Transform>();

        onRowLengthChangedEvent.UnregisterListener(OnRowLengthChanged);
        onRowLengthChangedEvent.RegisterListener(OnRowLengthChanged);
    }

    public void AddSlotToLast(CycleSlot slot)
    {
        CycleHorizontalLayout row = _rows.Count <= 0 ? CreateRow() : _rows.Last();
        row.AddSlotJust(slot);
        RebuildLayout();
    }

    public void AddSlotToNewRow(CycleSlot slot)
    {
        CycleHorizontalLayout row = CreateRow();
        row.AddSlotJust(slot);
        RebuildLayout();
    }

    public CycleHorizontalLayout GetRow(int id = -1)
    {
        if (_rows.Count <= 0 || id >= _rows.Count)
        {
            return CreateRow();
        }

        if (id == -1)
        {
            return _rows[_rows.Count - 1];
        }

        return _rows[id];
    }

    public CycleHorizontalLayout CreateRow(int id = -1)
    {
        if (id == -1)
        {
            id = _rows.Count;
        }
        CycleHorizontalLayout row = Instantiate(rowPanelPrefab, _verticalLayout).GetComponent<CycleHorizontalLayout>();
        row.name = $"Row_{id}";
        row.Init(id, this, maxSlotCount);
        InsertRow(id, row);

        return row;
    }

    public void InsertRow(int id, CycleHorizontalLayout row)
    {
        _rows.Insert(id, row);
        for (int i = 0; i < _rows.Count; i++)
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

    private void OnRowLengthChanged(int value)
    {
        maxSlotCount = value;
    }

    public void ResetCycle()
    {
        foreach(Transform child in transform)
        {
            if(child.TryGetComponent(out CycleHorizontalLayout row))
            {
                row.ResetCycle();
                Destroy(row.gameObject);
            }
        }

        _rows.Clear();
    }

    private void OnDestroy()
    {
        onRowLengthChangedEvent.UnregisterListener(OnRowLengthChanged);
    }
}
