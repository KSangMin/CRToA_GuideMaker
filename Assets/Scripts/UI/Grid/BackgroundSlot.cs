using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundSlot : MonoBehaviour, IDragHandler, IEndDragHandler
{
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

        // 자식이 자기 자신밖에 없으면 최소 크기로 설정
        if (children.Length <= 1) return;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        // 모든 자식 아이콘의 외곽 경계 계산
        foreach (var child in children)
        {
            if (child == _rectTransform) continue; // 자기 자신 제외

            // 아이콘의 중심점이 아닌 '영역'을 계산하기 위해 sizeDelta 활용
            float halfW = (child.rect.width * child.localScale.x) / 2f;
            float halfH = (child.rect.height * child.localScale.y) / 2f;

            minX = Mathf.Min(minX, child.localPosition.x - halfW);
            maxX = Mathf.Max(maxX, child.localPosition.x + halfW);
            minY = Mathf.Min(minY, child.localPosition.y - halfH);
            maxY = Mathf.Max(maxY, child.localPosition.y + halfH);
        }

        // 4. 배경 크기 설정 (가장 먼 아이콘들 + 패딩)
        float newWidth = (maxX - minX) + (padding * 2);
        float newHeight = (maxY - minY) + (padding * 2);
        _rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

        // 5. 아이콘들이 중앙에 오도록 위치 보정 (선택 사항)
        // 이 단계는 Pivot 설정에 따라 달라질 수 있습니다.
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_pressHandler.isLongPress) return;

        // 2. 가이드 UI 설정
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUI<UI_Grid>().content
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
            UIManager.Instance.GetUI<UI_Grid>().content.GetComponent<RectTransform>()
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