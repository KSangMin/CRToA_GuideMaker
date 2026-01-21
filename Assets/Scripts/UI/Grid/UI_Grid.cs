using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UI_Grid : UI
{
    private GameObject _backgroundSlotPrefab;
    private GameObject _foregroundSlotPrefab;
    [SerializeField] private Transform _backgroundGridParent;
    [SerializeField] private Transform _foregroundSlotGridParent;

    private List<List<Slot>> _backgroundSlots = new();
    private List<GameObject> _backgroundSlotsRowGOs = new();
    private List<List<Slot>> _foregroundSlots = new();
    private List<GameObject> _foregroundSlotsRowGOs = new();
    private int _curRow = 20;
    private int _curCol = 20;
    private int _spacing = 0;

    protected override void Awake()
    {
        base.Awake();

        _backgroundSlotPrefab = Resources.Load<GameObject>($"Prefabs/UI/BackgroundSlot");
        _foregroundSlotPrefab = Resources.Load<GameObject>($"Prefabs/UI/ForegroundSlot");
    }

    protected override void Start()
    {
        base.Start();

        CreateBackground();
        CreateSlot();
    }

    private void CreateBackground()
    {
        for (int i = 0; i < _curRow; i++)
        {
            GameObject row = new($"Row_{i}");
            HorizontalLayoutGroup hlGroup = row.AddComponent<HorizontalLayoutGroup>();
            hlGroup.childControlWidth = false;
            hlGroup.childControlHeight = false;
            hlGroup.spacing = _spacing;
            ContentSizeFitter fitter = row.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            row.transform.SetParent(_backgroundGridParent);
            _backgroundSlotsRowGOs.Add(row);

            List<Slot> tempSlots = new();
            for (int j = 0; j < _curCol; j++)
            {
                Slot slot = Instantiate(_backgroundSlotPrefab, row.transform).GetComponent<Slot>();
                slot.gameObject.name = $"BackgroundSlot_{i}_{j}";
                slot.InitSlot(this, i, j);
                tempSlots.Add(slot);
            }

            _backgroundSlots.Add(tempSlots);
        }
    }

    private void CreateSlot()
    {
        for (int i = 0; i < _curRow; i++)
        {
            GameObject row = new($"Row_{i}");
            HorizontalLayoutGroup hlGroup = row.AddComponent<HorizontalLayoutGroup>();
            hlGroup.childControlWidth = false;
            hlGroup.childControlHeight = false;
            hlGroup.spacing = _spacing;
            ContentSizeFitter fitter = row.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            row.transform.SetParent(_foregroundSlotGridParent);
            _foregroundSlotsRowGOs.Add(row);

            List<Slot> tempSlots = new();
            for (int j = 0; j < _curCol; j++)
            {
                Slot slot = Instantiate(_foregroundSlotPrefab, row.transform).GetComponent<Slot>();
                slot.gameObject.name = $"ForegroundSlot_{i}_{j}";
                slot.InitSlot(this, i, j);
                tempSlots.Add(slot);
            }

            _foregroundSlots.Add(tempSlots);
        }
    }

    public void CheckOccupiedSlot(Slot source, int r, int c)
    {
        if(r < 0 || r >= _curRow || c < 0 || c >= _curCol)
        {
            return;
        }

        Slot cur = _foregroundSlots[r][c];

        cur.ClearSlot();
        cur.OccupySlot(source);
    }
}
