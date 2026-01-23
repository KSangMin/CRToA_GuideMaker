using UnityEngine;

public class BackgroundSlot : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float gridSize = 100f;   // 내부 격자 단위
    [SerializeField] private Vector2 padding = new Vector2(10f, 10f); // 안쪽 여백

    private RectTransform _rectTransform;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
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
}