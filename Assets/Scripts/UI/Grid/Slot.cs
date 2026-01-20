using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    private UI_Grid _grid;

    private int _row = -1;
    private int _col = -1;
    private Slot _mainSlot;
    private List<Slot> _subSlots = new();

    public void InitSlot(UI_Grid grid, int r, int c)
    {
        _grid = grid;
        _row = r;
        _col = c;
    }

    public void SetIconToSlot(GameObject ghost, KeyValuePair<int, int> wh)
    {
        ClearSlot();

        ghost.GetComponent<Icon>().SetParentSlot(this);
        _mainSlot = this;

        for (int i = 0; i < wh.Value; i++)
        {
            for (int j = 0; j < wh.Key; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                int targetRow = _row + i;
                int targetCol = _col + j;
                _grid.CheckOccupiedSlot(this, targetRow, targetCol);
            }
        }
    }

    public void OccupySlot(Slot main)
    {
        _mainSlot = main;
        main.SubmitOccupiedSlot(this);
    }

    public void SubmitOccupiedSlot(Slot sub)
    {
        _subSlots.Add(sub);
    }

    public void ClearSlot()
    {
        if (_mainSlot != null)
        {
            Slot temp = _mainSlot;
            _mainSlot = null;
            temp.ClearSlot();
        }

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = _subSlots.Count - 1; i >= 0; i--)
        {
            _subSlots[i].ClearSlot();
        }
        _subSlots.Clear();
    }
}
