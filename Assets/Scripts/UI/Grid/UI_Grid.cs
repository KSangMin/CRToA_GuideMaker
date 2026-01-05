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
    private int _startRow = 20;
    private int _startCol = 20;
    private int _spacing = 5;

    protected override void Awake()
    {
        base.Awake();

        _gridParent = panel.transform;
        _slotPrefab = Resources.Load<GameObject>($"Prefabs/UI/{typeof(Slot)}");
    }

    protected override void Start()
    {
        base.Start();

        for(int i = 0; i < _startRow; i++)
        {
            GameObject row = new("Row");
            HorizontalLayoutGroup hlGroup = row.AddComponent<HorizontalLayoutGroup>();
            hlGroup.childControlWidth = false;
            hlGroup.spacing = _spacing;
            ContentSizeFitter fitter = row.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            row.transform.SetParent(_gridParent);
            _rowGOs.Add(row);

            List<Slot> tempSlots = new();
            for(int j = 0; j < _startCol; j++)
            {
                GameObject slot = Instantiate(_slotPrefab, row.transform);
                tempSlots.Add(slot.GetComponent<Slot>());
            }

            _slots.Add(tempSlots);
        }
    }
}
