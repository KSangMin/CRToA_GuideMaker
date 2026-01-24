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
        // 마우스 이동량을 캔버스 스케일에 맞춰 보정
        Vector2 delta = eventData.delta / GetComponentInParent<Canvas>().scaleFactor;

        // 부모인 Content의 줌 배율로 한 번 더 나누어 정확한 크기 계산
        delta /= _targetRect.parent.localScale.x;

        Vector2 size = _targetRect.sizeDelta;
        Vector2 pos = _targetRect.anchoredPosition;

        switch (handlePosition)
        {
            case HandlePosition.TopRight:
                // 우측 확장: 크기만 증가
                // 상단 확장: 크기 증가 + 좌표 위로 이동
                size += new Vector2(delta.x, delta.y);
                pos += new Vector2(0, delta.y);
                break;

            case HandlePosition.BottomRight:
                // 우측 확장: 크기 증가
                // 하단 확장: 크기 증가 (좌표는 고정)
                size += new Vector2(delta.x, -delta.y);
                break;

            case HandlePosition.BottomLeft:
                // 좌측 확장: 크기 증가 + 좌표 왼쪽 이동
                // 하단 확장: 크기 증가 (좌표 고정)
                size += new Vector2(-delta.x, -delta.y);
                pos += new Vector2(delta.x, 0);
                break;

            case HandlePosition.TopLeft:
                // 좌측 확장: 크기 증가 + 좌표 왼쪽 이동
                // 상단 확장: 크기 증가 + 좌표 위로 이동
                size += new Vector2(-delta.x, delta.y);
                pos += new Vector2(delta.x, delta.y);
                break;
        }

        // 최소 크기 제한 (아이콘들이 잘리지 않게)
        size.x = Mathf.Max(size.x, _minSize);
        size.y = Mathf.Max(size.y, _minSize);
        _targetRect.sizeDelta = size;
        if(size.x > _minSize)
        {
            _targetRect.anchoredPosition = new(pos.x, _targetRect.anchoredPosition.y);
        }
        if (size.y > _minSize)
        {
            _targetRect.anchoredPosition = new(_targetRect.anchoredPosition.x, pos.y);
        }

        // 배경 크기가 변했으므로 바깥쪽 자석 스냅 등이 필요하면 여기서 호출
    }
}
