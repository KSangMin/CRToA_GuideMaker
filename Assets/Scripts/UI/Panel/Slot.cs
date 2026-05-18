using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 공통 포인터/홀드 유틸과 고스트+패널 스크롤 위임에 쓰이는 헬퍼를 제공하는 베이스.
/// </summary>
public class Slot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] protected float holdDelaySeconds = 0.15f;

    protected Coroutine holdCoroutine;
    protected bool holdCanceled;

    public virtual void OnPointerDown(PointerEventData eventData) => OnSlotPointerDown(eventData);

    public virtual void OnPointerUp(PointerEventData eventData) => OnSlotPointerUp(eventData);

    public virtual void OnBeginDrag(PointerEventData eventData) => OnSlotBeginDrag(eventData);

    public virtual void OnDrag(PointerEventData eventData) => OnSlotDrag(eventData);

    public virtual void OnEndDrag(PointerEventData eventData) => OnSlotEndDrag(eventData);

    protected virtual void OnSlotPointerDown(PointerEventData eventData) { }

    protected virtual void OnSlotPointerUp(PointerEventData eventData) { }

    /// <summary>
    /// 포인터 업 직전에 호출됩니다. 예: 홀드 코루틴 취소.
    /// </summary>
    protected virtual void BeforeSlotPointerUp(PointerEventData eventData) { }

    /// <summary>
    /// 현재 포인터 업 시점에 “드래그 앤 드롭 세션”(고스트 등)이 활성인지 여부입니다.
    /// </summary>
    protected virtual bool IsDragDropActive() => false;

    /// <summary>
    /// 탭/클릭으로 간주될 때의 동작입니다. (<see cref="IsDragDropActive"/>가 false이고 <c>!eventData.dragging</c>일 때)
    /// </summary>
    protected virtual void OnSlotClick(PointerEventData eventData) { }

    /// <summary>
    /// 드래그 앤 드롭 세션이 활성인 상태에서 포인터를 뗄 때의 동작입니다.
    /// </summary>
    protected virtual void OnSlotDragDrop(PointerEventData eventData) { }

    /// <summary>
    /// 홀드 고스트형 슬롯 공통: 취소 후 클릭 vs 드롭 분기.
    /// </summary>
    protected void ResolveClickOrDragDropPointerUp(PointerEventData eventData)
    {
        BeforeSlotPointerUp(eventData);
        bool dnd = IsDragDropActive();
        if (!dnd && !eventData.dragging)
        {
            OnSlotClick(eventData);
        }

        if (dnd)
        {
            OnSlotDragDrop(eventData);
        }
    }

    /// <summary>
    /// 고스트가 없을 때만 패널 ScrollRect로 BeginDrag를 넘깁니다.
    /// </summary>
    protected bool TryBeginPanelScrollDragUnlessGhost(PointerEventData eventData, ScrollRect panelScroll,
        ref bool isDraggingScrollFlag, bool ghostActive)
    {
        if (ghostActive)
        {
            return false;
        }

        return TryBeginPanelScrollDrag(eventData, panelScroll, ref isDraggingScrollFlag);
    }

    /// <summary>
    /// 고스트가 있으면 해당 Rect를 포인터에 맞추고, 없으면 패널 스크롤 OnDrag를 호출합니다.
    /// </summary>
    protected void ResolveDragGhostOrPanelScroll(PointerEventData eventData, RectTransform ghostRect,
        ScrollRect panelScroll, ref bool isDraggingScrollFlag, bool ghostActive)
    {
        if (ghostActive && ghostRect != null)
        {
            SetRectTransformToPointer(ghostRect, eventData);
            return;
        }

        PanelScrollOnDrag(eventData, panelScroll, isDraggingScrollFlag);
    }

    /// <summary>
    /// 패널 스크롤 EndDrag 처리 후, 아니면 홀드 추적 취소.
    /// </summary>
    protected void ResolveEndDragPanelScrollOrCancelHold(PointerEventData eventData, ScrollRect panelScroll,
        ref bool isDraggingScrollFlag)
    {
        if (TryEndPanelScrollDrag(eventData, panelScroll, ref isDraggingScrollFlag))
        {
            return;
        }

        CancelHoldTracking();
    }

    protected virtual void OnSlotBeginDrag(PointerEventData eventData) { }

    protected virtual void OnSlotDrag(PointerEventData eventData) { }

    protected virtual void OnSlotEndDrag(PointerEventData eventData) { }

    protected void CancelHoldTracking()
    {
        holdCanceled = true;
        StopHoldCoroutineSilently();
    }

    protected void StopHoldCoroutineSilently()
    {
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    protected IEnumerator WaitHoldThen(PointerEventData eventData, System.Action<PointerEventData> onElapsed)
    {
        yield return new WaitForSeconds(holdDelaySeconds);

        if (!holdCanceled)
        {
            onElapsed?.Invoke(eventData);
        }

        holdCoroutine = null;
    }

    protected static void SetRectTransformToPointer(RectTransform rectTransform, PointerEventData eventData)
    {
        var parentRect = rectTransform.transform.parent as RectTransform;
        if (parentRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        rectTransform.anchoredPosition = localPoint;
    }

    protected void InstantiateGhostUnderPanel(GameObject prefab, PointerEventData eventData, out GameObject instance,
        out RectTransform rectTransform)
    {
        Transform ghostParent = UIManager.Instance.GetUI<UI_Panel>().forGhostParent;
        instance = Instantiate(prefab, ghostParent);
        rectTransform = instance.GetComponent<RectTransform>();
        SetRectTransformToPointer(rectTransform, eventData);
    }

    protected static void DestroyGhostRoot(ref GameObject ghost, ref RectTransform rectTransform)
    {
        if (ghost == null)
        {
            return;
        }

        ghost.transform.SetParent(null);
        Destroy(ghost);
        ghost = null;
        rectTransform = null;
    }

    protected bool TryBeginPanelScrollDrag(PointerEventData eventData, ScrollRect panelScroll, ref bool isDraggingScrollFlag)
    {
        isDraggingScrollFlag = false;
        if (panelScroll == null)
        {
            return false;
        }

        isDraggingScrollFlag = true;
        CancelHoldTracking();
        panelScroll.OnBeginDrag(eventData);
        return true;
    }

    protected void PanelScrollOnDrag(PointerEventData eventData, ScrollRect panelScroll, bool isDraggingScrollFlag)
    {
        if (isDraggingScrollFlag && panelScroll != null)
        {
            panelScroll.OnDrag(eventData);
        }
    }

    protected bool TryEndPanelScrollDrag(PointerEventData eventData, ScrollRect panelScroll, ref bool isDraggingScrollFlag)
    {
        if (!isDraggingScrollFlag)
        {
            return false;
        }

        panelScroll?.OnEndDrag(eventData);
        isDraggingScrollFlag = false;
        return true;
    }
}
