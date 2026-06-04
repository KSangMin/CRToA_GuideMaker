using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class Slot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Statics

    protected static readonly List<RaycastResult> RaycastBuffer = new List<RaycastResult>();

    protected static Transform GetGhostParent()
    {
        return UIManager.Instance.GetUI<UI_Panel>().forGhostParent;
    }

    protected static void SetRectTransformToPointer(RectTransform targetRect, PointerEventData eventData)
    {
        RectTransform parentRect = targetRect.transform.parent.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        targetRect.anchoredPosition = localPoint;
    }

    protected static void RaycastAll(PointerEventData eventData)
    {
        RaycastBuffer.Clear();
        EventSystem.current.RaycastAll(eventData, RaycastBuffer);
    }

    #endregion

    #region Serialized Fields

    private float _holdDelaySeconds = 0.15f;

    #endregion

    #region Private Fields

    protected ScrollRect _panelScroll;
    private bool _isDraggingScroll;
    private Coroutine _holdCoroutine;
    private bool _isCanceled;

    #endregion

    #region Unity Lifecycle

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSlotPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnSlotPointerUp(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnSlotBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnSlotDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnSlotEndDrag(eventData);
    }

    #endregion

    #region Protected Methods

    protected virtual void OnSlotPointerDown(PointerEventData eventData)
    {
    }

    protected virtual void OnSlotPointerUp(PointerEventData eventData)
    {
    }

    protected virtual void OnSlotBeginDrag(PointerEventData eventData)
    {
    }

    protected virtual void OnSlotDrag(PointerEventData eventData)
    {
    }

    protected virtual void OnSlotEndDrag(PointerEventData eventData)
    {
    }

    protected void WaitHoldThen(PointerEventData eventData, Action onElapsed)
    {
        _isCanceled = false;
        _holdCoroutine = StartCoroutine(WaitHoldThenCoroutine(onElapsed));
    }

    protected virtual void CancelHold()
    {
        MarkHoldCanceled();
    }

    protected void MarkHoldCanceled()
    {
        _isCanceled = true;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    protected void ResolveClickOrDragDropPointerUp(
        PointerEventData eventData,
        bool hasGhost,
        Action onClickWhenNoGhost,
        Action<PointerEventData> onGhostPointerUp)
    {
        CancelHold();

        if (!hasGhost && !eventData.dragging)
        {
            onClickWhenNoGhost?.Invoke();
        }

        if (hasGhost)
        {
            onGhostPointerUp?.Invoke(eventData);
        }
    }

    protected bool TryBeginPanelScrollDrag(PointerEventData eventData, bool hasActiveGhost)
    {
        if (hasActiveGhost)
        {
            return false;
        }

        _isDraggingScroll = true;
        CancelHold();
        _panelScroll.OnBeginDrag(eventData);
        return true;
    }

    protected void ForwardPanelScrollDrag(PointerEventData eventData, bool hasActiveGhost)
    {
        if (hasActiveGhost)
        {
            return;
        }

        if (_isDraggingScroll)
        {
            _panelScroll.OnDrag(eventData);
        }
    }

    protected bool EndPanelScrollDrag(PointerEventData eventData)
    {
        if (_isDraggingScroll)
        {
            _panelScroll.OnEndDrag(eventData);
            _isDraggingScroll = false;
            return true;
        }

        return false;
    }

    #endregion

    #region Private Methods

    private IEnumerator WaitHoldThenCoroutine(Action onElapsed)
    {
        yield return new WaitForSeconds(_holdDelaySeconds);

        if (!_isCanceled)
        {
            onElapsed?.Invoke();
        }

        _holdCoroutine = null;
    }

    #endregion
}
