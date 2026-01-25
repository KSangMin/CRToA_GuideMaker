using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Grid : UI, IPointerDownHandler, IDragHandler, IScrollHandler
{
    public RectTransform content;
    [SerializeField] private float zoomSpeed = 0.2f;
    [SerializeField] private float minZoom = 0.2f;
    [SerializeField] private float maxZoom = 5.0f;

    [SerializeField] private List<ResizeHandle> _handles = new();
    public Transform _forDragParent;
    [HideInInspector] public bool isHandleVisible;

    [SerializeField] private RectTransform _snapGuide;

    protected override void Awake()
    {
        base.Awake();

        _handles[0].InitHandle(HandlePosition.TopLeft, 0, 1);
        _handles[1].InitHandle(HandlePosition.TopRight, 1, 1);
        _handles[2].InitHandle(HandlePosition.BottomLeft, 0, 0);
        _handles[3].InitHandle(HandlePosition.BottomRight, 1, 0);

        HideHandles();
    }

    protected override void Start()
    {
        base.Start();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HideHandles();
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

        RefreshHandleScale();
    }

    private float GetCanvasScale()
    {
        // 캔버스의 Scale Factor를 가져와 드래그 속도를 일정하게 유지
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1.0f;
    }

    #region 핸들
    public void OpenResizeUI(RectTransform target)
    {
        ShowHandle();

        // 네 모서리에 핸들 생성
        foreach(ResizeHandle h in _handles)
        {
            h.SetHandle(target);
        }
    }

    private void ShowHandle()
    {
        isHandleVisible = true;

        foreach (ResizeHandle h in _handles)
        {
            h.gameObject.SetActive(true);
        }
    }

    public void HideHandles()
    {
        isHandleVisible = false;

        foreach (ResizeHandle h in _handles)
        {
            h.gameObject.SetActive(false);
        }
    }

    public void RefreshHandleScale()
    {
        // Content의 현재 scale 값을 가져옵니다. (줌 배율)
        float currentZoom = content.localScale.x;

        foreach (var handle in _handles)
        {
            // 줌 배율의 역수를 scale로 지정 (예: 줌이 2배면 스케일은 0.5)
            // 이렇게 하면 화면상에서의 물리적 크기는 항상 일정하게 유지됩니다.
            handle.transform.localScale = new Vector3(1f / currentZoom, 1f / currentZoom, 1f);
        }
    }
    #endregion 핸들

    #region 가이드
    public void SetSnapGuide(RectTransform parentRect, Vector2 pos, RectTransform targetRect)
    {
        ShowSnapGuide();
        _snapGuide.SetParent(parentRect);

        _snapGuide.localPosition = pos;
        _snapGuide.sizeDelta = targetRect.sizeDelta;
    }

    private void ShowSnapGuide()
    {
        _snapGuide.gameObject.SetActive(true);
    }

    public void HideSnapGuide()
    {
        _snapGuide.gameObject.SetActive(false);
        _snapGuide.SetParent(_forDragParent);
    }
    #endregion 가이드
}
