using UnityEngine;
using UnityEngine.EventSystems;

public enum HandlePosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

public class ResizeHandle : MonoBehaviour, IDragHandler
{
    private HandlePosition handlePosition;
    private Vector2 anchor;
    private RectTransform _rect;
    private RectTransform _targetRect;

    private int _minSize = 100;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    public void InitHandle(HandlePosition position, int pivotX, int pivotY)
    {
        handlePosition = position;
        anchor = new(pivotX, pivotY);
    }

    public void SetHandle(RectTransform target)
    {
        _targetRect = target;
        _rect.SetParent(_targetRect);

        _rect.anchorMin = anchor;
        _rect.anchorMax = anchor;
        _rect.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 1. 마우스 위치를 부모(Content)의 로컬 좌표로 변환
        RectTransform parentRect = _targetRect.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 mouseLocalPos);

        // 2. 현재 슬롯의 고정된 모서리(Pivot 0,1 기준으로는 좌측 상단)의 좌표를 가져옴
        // 하지만 핸들에 따라 '고정점'이 달라져야 하므로 상대적 계산이 필요합니다.

        Vector2 currentPos = _targetRect.anchoredPosition; // 좌측 상단 좌표
        Vector2 size = _targetRect.sizeDelta;

        switch (handlePosition)
        {
            case HandlePosition.BottomRight:
                // 고정점: 좌측 상단 (currentPos)
                // 새로운 크기 = 마우스 좌표 - 고정점 좌표
                size.x = mouseLocalPos.x - currentPos.x;
                size.y = currentPos.y - mouseLocalPos.y;
                break;

            case HandlePosition.TopRight:
                // 고정점: 좌측 하단 (currentPos.x, currentPos.y - size.y)
                size.x = mouseLocalPos.x - currentPos.x;
                float bottomY = currentPos.y - size.y;
                size.y = mouseLocalPos.y - bottomY;
                // 상단이 늘어난 만큼 좌표(y) 이동
                currentPos.y = mouseLocalPos.y;
                break;

            case HandlePosition.BottomLeft:
                // 고정점: 우측 상단 (currentPos.x + size.x, currentPos.y)
                float rightX = currentPos.x + size.x;
                size.x = rightX - mouseLocalPos.x;
                size.y = currentPos.y - mouseLocalPos.y;
                // 왼쪽이 늘어난 만큼 좌표(x) 이동
                currentPos.x = mouseLocalPos.x;
                break;

            case HandlePosition.TopLeft:
                // 고정점: 우측 하단 (currentPos.x + size.x, currentPos.y - size.y)
                float rX = currentPos.x + size.x;
                float bY = currentPos.y - size.y;
                size.x = rX - mouseLocalPos.x;
                size.y = mouseLocalPos.y - bY;
                // 왼쪽/위가 늘어난 만큼 좌표 이동
                currentPos.x = mouseLocalPos.x;
                currentPos.y = mouseLocalPos.y;
                break;
        }

        // 3. 최소 크기 제한
        float finalX = Mathf.Max(size.x, _minSize);
        float finalY = Mathf.Max(size.y, _minSize);

        // 4. 크기가 제한(minSize)에 걸렸을 때 좌표가 마우스를 따라가지 않도록 보정
        // (이 부분이 질문하신 '멀어짐 현상'을 해결하는 핵심입니다)
        if (handlePosition == HandlePosition.TopLeft || handlePosition == HandlePosition.BottomLeft)
        {
            // 왼쪽 변을 조정할 때: 우측 고정점으로부터 minSize만큼 떨어진 곳에 x좌표 고정
            float rightEdge = _targetRect.anchoredPosition.x + _targetRect.sizeDelta.x;
            if (size.x < _minSize) currentPos.x = rightEdge - _minSize;
        }
        if (handlePosition == HandlePosition.TopLeft || handlePosition == HandlePosition.TopRight)
        {
            // 위쪽 변을 조정할 때: 아래쪽 고정점으로부터 minSize만큼 떨어진 곳에 y좌표 고정
            float bottomEdge = _targetRect.anchoredPosition.y - _targetRect.sizeDelta.y;
            if (size.y < _minSize) currentPos.y = bottomEdge + _minSize;
        }

        _targetRect.sizeDelta = new Vector2(finalX, finalY);
        _targetRect.anchoredPosition = currentPos;
    }
}
