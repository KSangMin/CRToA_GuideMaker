using UnityEngine;

public class Content : MonoBehaviour
{
    public RectTransform _content;

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

    private void Awake()
    {
        _content = GetComponent<RectTransform>();
    }

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
}
