using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Content : MonoBehaviour
{
    [SerializeField] private UI_Grid _grid;
    private RectTransform _content;

    [Header("Gizmo Settings")]
    public bool showGizmos = true;
    public Color currentBoundsColor = Color.blue;
    public Color contentBoundsColor = Color.green;

    private struct ContentBounds
    {
        public float minX, maxX, minY, maxY;
        public float width, height;
        public Vector3[] worldCorners;
        public bool isValid;
    }

    private float curMaxX = 0f;
    private float curMaxY = 0f;
    private float _gridUnit = 100f;
    private float _padding = 10f;
    private float _spacing = 10f;
    public float GridUnit => _gridUnit;
    public float Padding => _padding;
    public float Spacing => _spacing;
    private float unit;

    private Dictionary<Vector2Int, BackgroundSlot> _occupancy = new();
    private List<BackgroundSlot> _slots = new();

    private bool _isExpanding = false;
    private float _expandDelay = 0.2f;

    private void Awake()
    {
        _content = GetComponent<RectTransform>();

        unit = _gridUnit + _spacing;
    }

    #region 크기 계산

    private void Start()
    {
        ResizeContent();
    }

    public void ResizeContent()
    {
        ContentBounds bounds = CalculateContentBounds();

        if (!bounds.isValid)
        {
            return;
        }

        Vector2 diff = new(bounds.minX, bounds.maxY);
        foreach (RectTransform child in _content)
        {
            child.anchoredPosition -= diff;
        }

        _content.sizeDelta = new(bounds.width, bounds.height);
        _content.anchoredPosition = new(-bounds.width / 2f, bounds.height / 2f);

        curMaxX = Mathf.FloorToInt(_content.sizeDelta.x / unit);
        curMaxY = Mathf.CeilToInt(_content.sizeDelta.y / unit);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || _content == null)
        {
            return;
        }

        DrawCurrentContentBounds();
        //DrawCalculatedContentBounds();
    }

    private void DrawCurrentContentBounds()
    {
        Vector3[] corners = new Vector3[4];
        _content.GetWorldCorners(corners);

        Gizmos.color = currentBoundsColor;

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }

    void DrawCalculatedContentBounds()
    {
        ContentBounds bounds = CalculateContentBounds();

        if (!bounds.isValid)
        {
            return;
        }

        // 로컬 좌표의 코너들을 월드 좌표로 변환
        Vector3[] calculatedCorners = new Vector3[4];
        calculatedCorners[0] = _content.TransformPoint(new Vector3(bounds.minX - bounds.minX, bounds.minY - bounds.maxY, 0));
        calculatedCorners[1] = _content.TransformPoint(new Vector3(bounds.minX - bounds.minX, bounds.maxY - bounds.maxY, 0));
        calculatedCorners[2] = _content.TransformPoint(new Vector3(bounds.maxX - bounds.minX, bounds.maxY - bounds.maxY, 0));
        calculatedCorners[3] = _content.TransformPoint(new Vector3(bounds.maxX - bounds.minX, bounds.minY - bounds.maxY, 0));

        Gizmos.color = contentBoundsColor;

        // 계산된 경계 그리기
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(calculatedCorners[i], calculatedCorners[(i + 1) % 4]);
        }
    }

    private ContentBounds CalculateContentBounds()
    {
        ContentBounds bounds = new ContentBounds();
        bounds.isValid = false;

        if (_content == null || _content.childCount == 0)
        {
            return bounds;
        }

        // 자식들의 경계를 계산하기 위한 초기값 설정
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        // 모든 자식 오브젝트를 순회하며 경계 계산
        foreach (RectTransform child in _content)
        {
            if (!child.gameObject.activeSelf)
            {
                continue;
            }

            // 자식의 월드 좌표 코너 위치들을 가져옴
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);

            // Content의 로컬 좌표로 변환
            for (int i = 0; i < 4; i++)
            {
                Vector3 localCorner = _content.InverseTransformPoint(corners[i]);

                minX = Mathf.Min(minX, localCorner.x);
                maxX = Mathf.Max(maxX, localCorner.x);
                minY = Mathf.Min(minY, localCorner.y);
                maxY = Mathf.Max(maxY, localCorner.y);
            }
        }

        // 유효한 경계가 계산되었는지 확인
        if (minX == float.MaxValue)
        {
            return bounds;
        }

        // 결과 저장
        bounds.minX = minX;
        bounds.maxX = maxX;
        bounds.minY = minY;
        bounds.maxY = maxY;
        bounds.width = maxX - minX;
        bounds.height = maxY - minY;
        bounds.isValid = true;

        return bounds;
    }
    #endregion 크기 계산

    #region 슬롯 관리
    public Vector2 GetPosFromIndex(Vector2Int index)
    {
        float x = index.x * unit;
        float y = index.y * -unit;
        return new Vector2(x, y);
    }

    // 마이너스 인덱스 발생 시 판 확장 및 모든 슬롯 밀기
    public void CheckAndExpand(Vector2Int targetIndex)
    {
        if (_isExpanding) return;

        // 1. 좌측/상단 확장 (기존 로직: 0보다 작을 때)
        int shiftX = targetIndex.x < 0 ? 1 : 0;
        int shiftY = targetIndex.y < 0 ? 1 : 0;

        // 2. 우측/하단 확장 (추가 로직: 현재 Content 크기보다 클 때)
        // 현재 Content가 수용 가능한 칸 수를 계산합니다.

        bool isExpandRight = targetIndex.x >= curMaxX;
        bool isExpandBottom = targetIndex.y >= curMaxY;

        if (shiftX > 0 || shiftY > 0 || isExpandRight || isExpandBottom)
        {
            // 우측/하단 확장은 위치(localPosition)를 옮길 필요가 없으므로 
            // 방향 데이터를 넘겨줍니다.
            StartCoroutine(ExpandCoroutine(new Vector2Int(shiftX, shiftY), isExpandRight, isExpandBottom));
        }
    }

    private IEnumerator ExpandCoroutine(Vector2Int shift, bool expandRight, bool expandBottom)
    {
        _isExpanding = true;

        // A. 좌측/상단 확장 처리 (기존 보정 로직)
        if (shift.x > 0 || shift.y > 0)
        {
            foreach (var slot in _slots)
            {
                slot.gridIndex += shift;
                slot.UpdateVisualPosition();
            }

            Vector2 expandAmount = new Vector2(shift.x * unit, shift.y * unit);
            _content.sizeDelta += expandAmount;
            // 좌측/상단으로 늘어난 만큼 전체 판을 밀어서 기존 슬롯 위치 고정
            _content.localPosition -= new Vector3(expandAmount.x, -expandAmount.y, 0);
        }

        // B. 우측/하단 확장 처리 (단순 크기 증가)
        if (expandRight) _content.sizeDelta += new Vector2(unit, 0);
        if (expandBottom) _content.sizeDelta += new Vector2(0, unit);

        RebuildOccupancy();

        curMaxX = Mathf.FloorToInt(_content.sizeDelta.x / unit);
        curMaxY = Mathf.CeilToInt(_content.sizeDelta.y / unit);

        yield return new WaitForSeconds(_expandDelay);
        _isExpanding = false;
    }

    private void RebuildOccupancy()
    {
        _occupancy.Clear();
        foreach (var slot in _slots)
        {
            for (int x = 0; x < slot.GridWH.x; x++)
            {
                for (int y = 0; y < slot.GridWH.y; y++)
                {
                    _occupancy[slot.gridIndex + new Vector2Int(x, y)] = slot;
                }
            }
        }
    }

    public void RegisterSlot(BackgroundSlot slot)
    {
        _slots.Add(slot);
    }

    #endregion 슬롯 관리
}
