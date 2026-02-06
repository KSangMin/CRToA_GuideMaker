using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Grid : UI, IPointerDownHandler, IDragHandler, IScrollHandler
{
    public RectTransform content;
    [SerializeField] private float zoomSpeed = 0.2f;
    [SerializeField] private float minZoom = 0.4f;
    [SerializeField] private float maxZoom = 5.0f;

    private float _gridUnit = 100f;
    private float _padding = 20f;
    private float _spacing = 50f;
    public float GridUnit => _gridUnit;
    public float Padding => _padding;
    public float Spacing => _spacing;

    private Dictionary<Vector2Int, BackgroundSlot> _occupancy = new();
    private List<BackgroundSlot> _slots = new();

    [SerializeField] private List<ResizeHandle> _handles = new();
    public Transform _forDragParent;
    [HideInInspector] public bool isHandleVisible;

    [SerializeField] private RectTransform _snapGuide;

    private bool _isExpanding = false;
    private float _expandDelay = 0.2f;

    protected override void Awake()
    {
        base.Awake();

        _handles[0].InitHandle(HandlePosition.TopLeft, 0, 1);
        _handles[1].InitHandle(HandlePosition.TopRight, 1, 1);
        _handles[2].InitHandle(HandlePosition.BottomLeft, 0, 0);
        _handles[3].InitHandle(HandlePosition.BottomRight, 1, 0);

        HideHandles();
    }

    public Vector2 GetPosFromIndex(Vector2Int index)
    {
        float x = index.x * (_gridUnit + _spacing);
        float y = index.y * -(_gridUnit + _spacing);
        return new Vector2(x, y);
    }

    // 마이너스 인덱스 발생 시 판 확장 및 모든 슬롯 밀기
    public void CheckAndExpand(Vector2Int targetIndex)
    {
        // 이미 확장 중이면 무시
        if (_isExpanding) return;

        int shiftX = targetIndex.x < 0 ? 1 : 0;
        int shiftY = targetIndex.y < 0 ? 1 : 0;

        if (shiftX > 0 || shiftY > 0)
        {
            StartCoroutine(ExpandCoroutine(new Vector2Int(shiftX, shiftY)));
        }
    }

    private IEnumerator ExpandCoroutine(Vector2Int shift)
    {
        _isExpanding = true;

        // 1. 데이터 및 물리 위치 보정 (기존 로직)
        foreach (var slot in _slots)
        {
            slot.gridIndex += shift;
            slot.UpdateVisualPosition();
        }

        float unit = UIManager.Instance.GetUI<UI_Grid>().GridUnit
            + UIManager.Instance.GetUI<UI_Grid>().Spacing;
        Vector2 expandAmount = new Vector2(shift.x * unit, shift.y * unit);

        content.sizeDelta += expandAmount;
        content.localPosition -= (Vector3)expandAmount;

        RebuildOccupancy();

        // 2. 지정된 시간만큼 대기 (이게 0.n초의 핵심!)
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

    public void OnPointerDown(PointerEventData eventData)
    {
        HideHandles();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (content == null)
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
        foreach (ResizeHandle h in _handles)
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
