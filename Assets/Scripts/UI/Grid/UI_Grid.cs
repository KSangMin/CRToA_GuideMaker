using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UI_Grid : UI
{
    private GameObject _slotPrefab;
    private Transform _gridParent;

    private List<List<Slot>> _slots = new();
    private List<GameObject> _rowGOs = new();
    private int _curRow = 20;
    private int _curCol = 20;
    private int _spacing = 0;

    protected override void Awake()
    {
        base.Awake();

        _gridParent = panel.transform;
        _slotPrefab = Resources.Load<GameObject>($"Prefabs/UI/{typeof(Slot)}");
    }

    protected override void Start()
    {
        base.Start();

        for(int i = 0; i < _curRow; i++)
        {
            GameObject row = new("Row");
            HorizontalLayoutGroup hlGroup = row.AddComponent<HorizontalLayoutGroup>();
            hlGroup.childControlWidth = false;
            hlGroup.childControlHeight = false;
            hlGroup.spacing = _spacing;
            ContentSizeFitter fitter = row.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            row.transform.SetParent(_gridParent);
            _rowGOs.Add(row);

            List<Slot> tempSlots = new();
            for(int j = 0; j < _curCol; j++)
            {
                Slot slot = Instantiate(_slotPrefab, row.transform).GetComponent<Slot>();
                slot.SetSlot(this, i, j);
                tempSlots.Add(slot);
            }

            _slots.Add(tempSlots);
        }
    }

    public void CheckOccupiedSlot(Slot source, int r, int c)
    {
        if(r < 0 || r >= _curRow || c < 0 || c >= _curCol)
        {
            return;
        }

        _slots[r][c].ClearSlot();
        _slots[r][c].OccupySlot(source);
    }
}
