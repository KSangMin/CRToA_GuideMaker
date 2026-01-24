using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

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
        RectTransform parentRect = _targetRect.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, eventData.pressEventCamera, out Vector2 mouseLocalPos);

        // 1. 고정점(Anchor Point) 설정 (Pivot 0,1 기준)
        Vector2 fixedPoint = Vector2.zero;
        Vector2 currentPos = _targetRect.anchoredPosition;
        Vector2 currentSize = _targetRect.sizeDelta;
        float _startAspectRatio = currentSize.x / currentSize.y;

        switch (handlePosition)
        {
            case HandlePosition.BottomRight: fixedPoint = new Vector2(currentPos.x, currentPos.y); break;
            case HandlePosition.TopRight: fixedPoint = new Vector2(currentPos.x, currentPos.y - currentSize.y); break;
            case HandlePosition.BottomLeft: fixedPoint = new Vector2(currentPos.x + currentSize.x, currentPos.y); break;
            case HandlePosition.TopLeft: fixedPoint = new Vector2(currentPos.x + currentSize.x, currentPos.y - currentSize.y); break;
        }

        // 2. 방향성을 가진 거리 계산 (Abs 제거)
        float rawDiffX = mouseLocalPos.x - fixedPoint.x;
        float rawDiffY = mouseLocalPos.y - fixedPoint.y;

        // 3. 핸들 위치에 따라 '의도된' 확장 크기(Distance) 계산
        float distX = 0, distY = 0;
        switch (handlePosition)
        {
            case HandlePosition.BottomRight: distX = rawDiffX; distY = -rawDiffY; break;
            case HandlePosition.TopRight: distX = rawDiffX; distY = rawDiffY; break;
            case HandlePosition.BottomLeft: distX = -rawDiffX; distY = -rawDiffY; break;
            case HandlePosition.TopLeft: distX = -rawDiffX; distY = rawDiffY; break;
        }

        // 4. 비율 유지 로직 (더 많이 끌어당긴 축을 기준으로 나머지 축 결정)
        float newWidth, newHeight;
        if (distX > distY * _startAspectRatio)
        {
            newWidth = distX;
            newHeight = newWidth / _startAspectRatio;
        }
        else
        {
            newHeight = distY;
            newWidth = newHeight * _startAspectRatio;
        }

        // 5. 격자 스냅 및 최소 크기 제한
        newWidth = Mathf.Max(_minSize, Mathf.Round(newWidth / _minSize) * _minSize);
        newHeight = newWidth / _startAspectRatio; // 스냅 후에도 비율 엄격 유지

        // 6. 결과 적용 및 위치 보정 (Pivot 0,1 기준)
        _targetRect.sizeDelta = new Vector2(newWidth, newHeight);

        Vector2 newPos = currentPos;

        // 좌측 핸들(Left)일 때는 늘어난 만큼 왼쪽으로 밀어줘야 함
        newPos.x = (handlePosition == HandlePosition.TopLeft
            || handlePosition == HandlePosition.BottomLeft)
            ? fixedPoint.x - newWidth
            : fixedPoint.x;

        // 상단 핸들(Top)일 때는 늘어난 만큼 위쪽으로 밀어줘야 함
        newPos.y = (handlePosition == HandlePosition.TopLeft
            || handlePosition == HandlePosition.TopRight)
            ? fixedPoint.y + 0// Pivot(0,1)이 이미 상단이므로 fixedPoint.y가 곧 Top 위치
            : fixedPoint.y;// Bottom 핸들이면 고정점(Top) 위치 유지

        // Y축 보정: 상단 핸들을 잡고 늘리면 위쪽(y+)으로 이동해야 함
        // Pivot이 (0,1)이므로 y좌표는 항상 사각형의 'Top' 라인입니다.
        newPos.y = (handlePosition == HandlePosition.TopLeft
            || handlePosition == HandlePosition.TopRight)
            ? fixedPoint.y + newHeight// 고정점이 아래이므로 위로 밀어줌
            : fixedPoint.y;// 고정점이 위이므로 그대로 유지

        _targetRect.anchoredPosition = newPos;
    }
}
