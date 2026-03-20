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

    private RectTransform _rectTransform;
    private PressHandler _pressHandler;

    private Vector2 _snapPos = Vector2.zero;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _pressHandler = GetComponent<PressHandler>();
    }

    public Vector2 GetSnapPosition(Vector2 localPos)
    {
        float snapX = Mathf.FloorToInt(localPos.x / gridSize) * gridSize;
        float snapY = Mathf.FloorToInt(localPos.y / -gridSize) * gridSize;
        return new Vector2(snapX + padding, snapY - padding);
    }

    public void SetSlot(int x, int y, int w, int h)
    {
        gridIndex = new(x, y);
        _gridWH = new(w, h);
    }

    // 아이콘이 이 배경에 드롭되었을 때 호출할 함수
    public void AddIcon(RectTransform iconRect, bool isFirst = false)
    {
        iconRect.SetParent(transform);
        Vector2 pos = isFirst
            ? new(iconRect.localPosition.x + gridSize / 2f
                , iconRect.localPosition.y - gridSize / 2f)
            : iconRect.localPosition;
        iconRect.localPosition = GetSnapPosition(pos);
        UpdateSize();
    }

    public void UpdateSize()
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (RectTransform child in _rectTransform)
        {
            //가이드 제외
            if (child.name.Contains("SnapGuide")) continue;

            Vector2 wh = child.GetComponent<Icon>().GetWH();

            minX = Mathf.Min(minX, child.localPosition.x);
            maxX = Mathf.Max(maxX, child.localPosition.x + wh.x);
            minY = Mathf.Min(minY, child.localPosition.y);
            maxY = Mathf.Max(maxY, child.localPosition.y + wh.y);
        }

        // 2. 순수 콘텐츠 영역 계산
        float contentWidth = maxX - minX;
        float contentHeight = maxY - minY;

        // 3. [핵심] 픽셀 크기를 기반으로 논리적 gridSize(칸 수) 계산
        // gridUnit(100)으로 나누고 올림(Ceil)하여 최소 몇 칸이 필요한지 구합니다.
        int cols = Mathf.CeilToInt(contentWidth / UIManager.Instance.GetUI<UI_Grid>().content.ItemUnit);
        int rows = Mathf.CeilToInt(contentHeight / UIManager.Instance.GetUI<UI_Grid>().content.ItemUnit);

        // 최소 1x1은 유지
        _gridWH = new Vector2Int(Mathf.Max(1, cols), Mathf.Max(1, rows));

        UpdatePhysicalSize();
    }

    private void UpdatePhysicalSize()
    {
        // 그리드 칸 수 기반 크기 + 양쪽 패딩
        float finalW = (_gridWH.x * UIManager.Instance.GetUI<UI_Grid>().content.ItemUnit)
            + (UIManager.Instance.GetUI<UI_Grid>().content.Padding * 2);
        float finalH = (_gridWH.y * UIManager.Instance.GetUI<UI_Grid>().content.ItemUnit)
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

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUI<UI_Grid>().contentRect
            , eventData.position
            , eventData.pressEventCamera
            , out Vector2 localMousePos);

        UI_Grid gridui = UIManager.Instance.GetUI<UI_Grid>();
        Content content = gridui.content;

        // 2. 가상 그리드 단위로 인덱스 계산 (Round 사용)
        int tx = Mathf.RoundToInt(localMousePos.x / content.SlotUnit);
        int ty = Mathf.RoundToInt(localMousePos.y / -content.SlotUnit);

        // 3. 확장 체크 (필요시)
        content.CheckAndExpand(new Vector2Int(tx, ty));

        // 4. 계산된 인덱스를 실제 좌표로 다시 변환
        _snapPos = content.GetPosFromIndex(new Vector2Int(Mathf.Max(0, tx), Mathf.Max(0, ty)));

        // 5. 가이드 표시 (다른 슬롯 고려 없이 그리드에만 맞춤)
        gridui.SetSnapGuide(gridui.contentRect, _snapPos, _rectTransform);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_pressHandler.isLongPress) return;

        if (_snapPos != Vector2.zero)
        {
            _rectTransform.anchoredPosition = _snapPos;
        }

        UIManager.Instance.GetUI<UI_Grid>().HideSnapGuide();
        _pressHandler.isLongPress = false;
        transform.localScale = Vector3.one;
    }
}