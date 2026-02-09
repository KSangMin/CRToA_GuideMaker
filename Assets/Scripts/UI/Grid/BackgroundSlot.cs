using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundSlot : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Vector2Int gridIndex; // 왼쪽 상단 기준 인덱스
    private Vector2Int _gridWH = new Vector2Int(1, 1); // 차지하는 칸 수
    public Vector2Int GridWH => _gridWH;

    private float gridSize = 100f;   // 내부 격자 단위
    private float padding = 10f; // 안쪽 여백
    private float _snapThreshold = 120f; // 자석 스냅 허용 오차

    private RectTransform _rectTransform;
    private PressHandler _pressHandler;

    private Vector2 _snapPos = Vector2.zero;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _pressHandler = GetComponent<PressHandler>();

        _snapThreshold = gridSize + padding * 2;
    }

    public Vector2 GetSnapPosition(Vector2 localPos)
    {
        float snapX = Mathf.Round(localPos.x / gridSize) * gridSize;
        float snapY = Mathf.Round(localPos.y / gridSize) * gridSize;
        return new Vector2(snapX + padding, snapY - padding);
    }

    public void SetSlot(int x, int y, int w, int h)
    {
        gridIndex = new(x, y);
        _gridWH = new(w, h);
    }

    // 아이콘이 이 배경에 드롭되었을 때 호출할 함수
    public void AddIcon(RectTransform iconRect)
    {
        iconRect.SetParent(transform);
        iconRect.localPosition = GetSnapPosition(iconRect.localPosition);
        UpdateSize();
    }

    public void UpdateSize()
    {
        RectTransform[] children = GetComponentsInChildren<RectTransform>();

        // 1. 자식이 없으면 기본 1x1 크기로 설정
        if (children.Length <= 1)
        {
            _gridWH = new Vector2Int(1, 1);
            UpdatePhysicalSize();
            return;
        }

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var child in children)
        {
            // 자기 자신 및 가이드 UI 등 제외
            if (child == _rectTransform || child.name.Contains("Guide")) continue;

            float halfW = (child.rect.width * child.localScale.x) / 2f;
            float halfH = (child.rect.height * child.localScale.y) / 2f;

            minX = Mathf.Min(minX, child.localPosition.x - halfW);
            maxX = Mathf.Max(maxX, child.localPosition.x + halfW);
            minY = Mathf.Min(minY, child.localPosition.y - halfH);
            maxY = Mathf.Max(maxY, child.localPosition.y + halfH);
        }

        // 2. 순수 콘텐츠 영역 계산
        float contentWidth = maxX - minX;
        float contentHeight = maxY - minY;

        // 3. [핵심] 픽셀 크기를 기반으로 논리적 gridSize(칸 수) 계산
        // gridUnit(100)으로 나누고 올림(Ceil)하여 최소 몇 칸이 필요한지 구합니다.
        int cols = Mathf.CeilToInt(contentWidth / UIManager.Instance.GetUI<UI_Grid>().content.GridUnit);
        int rows = Mathf.CeilToInt(contentHeight / UIManager.Instance.GetUI<UI_Grid>().content.GridUnit);

        // 최소 1x1은 유지
        _gridWH = new Vector2Int(Mathf.Max(1, cols), Mathf.Max(1, rows));

        // 4. 최종 물리적 크기 적용
        UpdatePhysicalSize();
    }

    private void UpdatePhysicalSize()
    {
        // 그리드 칸 수 기반 크기 + 양쪽 패딩
        float finalW = (_gridWH.x * UIManager.Instance.GetUI<UI_Grid>().content.GridUnit)
            + (UIManager.Instance.GetUI<UI_Grid>().content.Padding * 2);
        float finalH = (_gridWH.y * UIManager.Instance.GetUI<UI_Grid>().content.GridUnit)
            + (UIManager.Instance.GetUI<UI_Grid>().content.Padding * 2);

        _rectTransform.sizeDelta = new Vector2(finalW, finalH);
    }

    public void UpdateVisualPosition()
    {
        _rectTransform.localPosition = UIManager.Instance.GetUI<UI_Grid>().content.GetPosFromIndex(gridIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_pressHandler.isLongPress) return;

        // 2. 가이드 UI 설정
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUI<UI_Grid>().contentRect
            , eventData.position
            , eventData.pressEventCamera
            , out Vector2 localMousePos);

        // 3. 자석 스냅 위치 계산 (기존 OnEndDrag 로직 활용)
        Vector2 snapPos = localMousePos; // 기본은 마우스 위치

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, _snapThreshold * 5);
        RectTransform myRect = GetComponent<RectTransform>();

        List<BackgroundSlot> slots = new();
        foreach (var collider in overlaps)
        {
            if (collider.gameObject == gameObject)
            {
                continue;
            }
            if (collider.TryGetComponent(out BackgroundSlot slot))
            {
                slots.Add(slot);
            }
        }

        bool isSnapped = false;
        foreach (var slot in slots)
        {
            RectTransform otherRect = slot.GetComponent<RectTransform>();

            // 여기서부터는 기존의 자석 스냅 로직 동일
            Vector2 offset = localMousePos - (Vector2)otherRect.localPosition;
            float targetDistX = (otherRect.rect.width + myRect.rect.width) / 2f + padding;
            float targetDistY = (otherRect.rect.height + myRect.rect.height) / 2f + padding;

            // X축 자석 체크
            if (Mathf.Abs(Mathf.Abs(offset.x) - targetDistX) < _snapThreshold
                && Mathf.Abs(offset.y) < _snapThreshold)
            {
                snapPos = new Vector2(
                    otherRect.localPosition.x + (Mathf.Sign(offset.x) * targetDistX)
                    , otherRect.localPosition.y);
                isSnapped = true;
                break;
            }

            // Y축 자석 체크
            if (Mathf.Abs(Mathf.Abs(offset.y) - targetDistY) < _snapThreshold
                && Mathf.Abs(offset.x) < _snapThreshold)
            {
                snapPos = new Vector2(
                    otherRect.localPosition.x
                    , otherRect.localPosition.y + (Mathf.Sign(offset.y) * targetDistY));
                isSnapped = true;
                break;
            }
        }

        //가이드 위치
        if (isSnapped)
        {
            UIManager.Instance.GetUI<UI_Grid>().SetSnapGuide(
            UIManager.Instance.GetUI<UI_Grid>().contentRect.GetComponent<RectTransform>()
            , snapPos
            , _rectTransform);
            _snapPos = snapPos;
        }
        else
        {
            UIManager.Instance.GetUI<UI_Grid>().HideSnapGuide();
            _snapPos = Vector2.zero;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_pressHandler.isLongPress) return;

        if (_snapPos != Vector2.zero)
        {
            transform.localPosition = _snapPos;
        }

        UIManager.Instance.GetUI<UI_Grid>().HideSnapGuide();
        _pressHandler.isLongPress = false;
        transform.localScale = Vector3.one;
    }
}