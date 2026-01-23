using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Grid : UI, IDragHandler, IScrollHandler
{
    [SerializeField] private RectTransform _content;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.2f;
    [SerializeField] private float maxZoom = 5.0f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(_content == null)
        {
            return;
        }

        _content.anchoredPosition += eventData.delta / GetCanvasScale();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (_content == null) return;

        float scroll = eventData.scrollDelta.y;
        float zoomStep = 1 + (scroll * zoomSpeed);

        Vector3 newScale = _content.localScale * zoomStep;

        newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
        newScale.z = 1;

        _content.localScale = newScale;
    }

    private float GetCanvasScale()
    {
        // 캔버스의 Scale Factor를 가져와 드래그 속도를 일정하게 유지
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1.0f;
    }
}
