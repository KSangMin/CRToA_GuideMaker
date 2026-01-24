using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundSlot : MonoBehaviour, IEndDragHandler
{
    [Header("설정")]
    [SerializeField] private float gridSize = 100f;   // 내부 격자 단위
    [SerializeField] private Vector2 padding = new Vector2(10f, 10f); // 안쪽 여백

    private RectTransform _rectTransform;
    private PressHandler _pressHandler;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _pressHandler = GetComponent<PressHandler>();
    }

    // 아이콘이 이 배경에 드롭되었을 때 호출할 함수
    public void AddIcon(RectTransform iconRect)
    {
        // 1. 부모를 이 배경 슬롯으로 설정
        iconRect.SetParent(transform);

        // 2. 현재 좌표를 격자에 맞춰 스냅 (Local 좌표 기준)
        Vector2 localPos = iconRect.localPosition;
        float snapX = Mathf.Round(localPos.x / gridSize) * gridSize;
        float snapY = Mathf.Round(localPos.y / gridSize) * gridSize;
        iconRect.localPosition = new Vector3(snapX + padding.x, snapY - padding.y, 0);

        // 3. 자식 아이콘들의 위치에 맞춰 배경 크기 재계산
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
        float newWidth = (maxX - minX) + (padding.x * 2);
        float newHeight = (maxY - minY) + (padding.y * 2);
        _rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

        // 5. 아이콘들이 중앙에 오도록 위치 보정 (선택 사항)
        // 이 단계는 Pivot 설정에 따라 달라질 수 있습니다.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_pressHandler.isLongPress) return;

        float outerMargin = 10f;   // 슬롯 사이 유지할 간격
        float snapThreshold = 120f; // 자석처럼 끌어당길 거리 (여유 있게 설정)

        // 씬 내의 모든 BackgroundSlot 검색
        BackgroundSlot[] allSlots = FindObjectsByType<BackgroundSlot>(FindObjectsSortMode.None);
        RectTransform myRect = GetComponent<RectTransform>();

        foreach (var other in allSlots)
        {
            if (other == this) continue;

            RectTransform otherRect = other.GetComponent<RectTransform>();

            // 1. 상대방과의 거리 계산 (중심점 기준)
            Vector2 offset = myRect.localPosition - otherRect.localPosition;

            // 2. 각 방향별 스냅 목표 좌표 계산
            // 오른쪽 스냅: (상대 너비 + 내 너비) / 2 + 간격
            float targetDistX = (otherRect.rect.width + myRect.rect.width) / 2f + outerMargin;
            float targetDistY = (otherRect.rect.height + myRect.rect.height) / 2f + outerMargin;

            // X축 스냅 체크 (왼쪽/오른쪽)
            if (Mathf.Abs(Mathf.Abs(offset.x) - targetDistX) < snapThreshold && Mathf.Abs(offset.y) < snapThreshold)
            {
                float snapX = otherRect.localPosition.x + (Mathf.Sign(offset.x) * targetDistX);
                // Y축은 상대방과 높이를 맞춰주면 정렬이 더 깔끔합니다.
                myRect.localPosition = new Vector2(snapX, otherRect.localPosition.y);
                break;
            }

            // Y축 스냅 체크 (위/아래)
            if (Mathf.Abs(Mathf.Abs(offset.y) - targetDistY) < snapThreshold && Mathf.Abs(offset.x) < snapThreshold)
            {
                float snapY = otherRect.localPosition.y + (Mathf.Sign(offset.y) * targetDistY);
                // X축은 상대방과 중앙을 맞춰줍니다.
                myRect.localPosition = new Vector2(otherRect.localPosition.x, snapY);
                break;
            }
        }

        _pressHandler.isLongPress = false;
        transform.localScale = Vector3.one;
    }
}