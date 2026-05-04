using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectPanelScrollRect : ScrollRect
{
    private ScrollRect _verticalScrollRect;

    private Vector2 _startPos;
    private bool _directionDecided;
    private bool _isVertical;

    public void Init(ScrollRect menuScroll)
    {
        _verticalScrollRect = menuScroll;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        _startPos = eventData.position;
        _directionDecided = false;
        _isVertical = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!_directionDecided)
        {
            Vector2 diff = eventData.position - _startPos;
            if (diff.magnitude < EventSystem.current.pixelDragThreshold)
            {
                return;
            }

            _directionDecided = true;
            _isVertical = Mathf.Abs(diff.y) > Mathf.Abs(diff.x);

            if (_isVertical)
            {
                _verticalScrollRect.OnBeginDrag(eventData);
            }
            else
            {
                base.OnBeginDrag(eventData);
            }
        }

        if (_isVertical)
        {
            _verticalScrollRect.OnDrag(eventData);
        }
        else
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (_isVertical)
        {
            _verticalScrollRect.OnEndDrag(eventData);
        }
        else
        {
            base.OnEndDrag(eventData);
        }

        _directionDecided = false;
        _isVertical = false;
    }
}
