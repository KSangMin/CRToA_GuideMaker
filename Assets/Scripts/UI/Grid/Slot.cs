using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private UI_Grid _grid;

    private int _row = -1;
    private int _col = -1;
    private Slot _mainSlot;
    private List<Slot> _subSlots = new();
    private Icon _occupyingIcon;

    public void InitSlot(UI_Grid grid, int r, int c)
    {
        _grid = grid;
        _row = r;
        _col = c;
    }

    public void SetIconToSlot(GameObject ghost, KeyValuePair<int, int> wh)
    {
        ClearSlot();

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

        _mainSlot = this;
        _occupyingIcon = ghost.GetComponent<Icon>();
        _occupyingIcon.SetParentSlot(this);
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

        _occupyingIcon = null;

        foreach (Transform child in transform)
        {
            Debug.Log($"{name}의 자식: {child.name} 삭제");
            Destroy(child.gameObject);
        }

        for (int i = _subSlots.Count - 1; i >= 0; i--)
        {
            _subSlots[i].ClearSlot();
        }
        _subSlots.Clear();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(_mainSlot != null && _mainSlot != this)
        {
            _mainSlot.OnPointerDown(eventData);
            return;
        }
        _occupyingIcon?.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_mainSlot != null && _mainSlot != this)
        {
            _mainSlot.OnPointerUp(eventData);
            return;
        }
        _occupyingIcon?.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_mainSlot != null && _mainSlot != this)
        {
            _mainSlot.OnDrag(eventData);
            return;
        }
        _occupyingIcon?.OnDrag(eventData);
    }
}
