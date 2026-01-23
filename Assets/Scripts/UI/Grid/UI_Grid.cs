using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Grid : UI, IDragHandler, IScrollHandler
{
    public RectTransform content;
    [SerializeField] private float zoomSpeed = 0.2f;
    [SerializeField] private float minZoom = 0.2f;
    [SerializeField] private float maxZoom = 5.0f;

    public Transform _forDragParent;

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
        if(content == null)
        {
            return;
        }

        content.anchoredPosition += eventData.delta / GetCanvasScale();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (content == null) return;

        Vector3 newScale = content.localScale;

        float scroll = eventData.scrollDelta.y;
        float zoomStep = scroll > 0 ? +zoomSpeed : -zoomSpeed;

        newScale.x = Mathf.Clamp(newScale.x + zoomStep, minZoom, maxZoom);
        newScale.y = Mathf.Clamp(newScale.y + zoomStep, minZoom, maxZoom);
        newScale.z = 1;

        content.localScale = newScale;
    }

    private float GetCanvasScale()
    {
        // 캔버스의 Scale Factor를 가져와 드래그 속도를 일정하게 유지
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1.0f;
    }
}
