using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour
{
    private UI_Grid _grid;
    private int _row = -1;
    private int _col = -1;
    private Slot masterSlot;

    public void SetSlot(UI_Grid grid, int r, int c)
    {
        _grid = grid;
        _row = r;
        _col = c;
    }

    public void CheckSlot(KeyValuePair<int, int> wh)
    {
        ClearSlot();

        for(int i = 0; i < wh.Value; i++)
        {
            for(int j = 0; j < wh.Key; j++)
            {
                int targetRow = _row + i;
                int targetCol = _col + j;
                _grid.CheckOccupiedSlot(this, targetRow, targetCol);
            }
        }
    }

    public void OccupySlot(Slot master)
    {
        masterSlot = master;
    }

    public void ClearSlot()
    {
        if(masterSlot != null)
        {
            Slot temp = masterSlot;
            masterSlot = null;
            temp.ClearSlot();
        }

        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
