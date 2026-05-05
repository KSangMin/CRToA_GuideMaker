using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResetSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject ghostObject;

    private ScrollRect _panelScroll;
    private bool _isDraggingScroll = false;

    private GameObject _ghost;
    private RectTransform _ghostRect;
    private Coroutine _holdCoroutine;
    private float _holdTime = 0.15f;
    private bool _isCanceled = false;

    public void SetSlot(ScrollRect panelScroll)
    {
        _panelScroll = panelScroll;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _holdCoroutine = StartCoroutine(CheckHoldAfterDelay(eventData));
    }

    private IEnumerator CheckHoldAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(_holdTime);

        if (!_isCanceled && !eventData.dragging)
        {
            CreateGhost(eventData);
        }

        _holdCoroutine = null;
    }

    private void CreateGhost(PointerEventData eventData)
    {
        Transform ghostParent = UIManager.Instance.GetUI<UI_Panel>().forGhostParent;
        _ghost = Instantiate(ghostObject, ghostParent);
        _ghostRect = _ghost.GetComponent<RectTransform>();

        SetGhostPositionToPointer(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();

        DestroyGhost();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_ghost == null)
        {
            _isDraggingScroll = true;
            CancelHold();
            _panelScroll.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost != null)
        {
            SetGhostPositionToPointer(eventData);
            return;
        }

        if (_isDraggingScroll)
        {
            _panelScroll.OnDrag(eventData);
        }
    }

    public void SetGhostPositionToPointer(PointerEventData eventData)
    {
        if(_ghost == null)
        {
            return;
        }

        RectTransform parentRect = _ghost.transform.parent.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);

        _ghostRect.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDraggingScroll)
        {
            _panelScroll.OnEndDrag(eventData);
            _isDraggingScroll = false;
            return;
        }

        if (_ghost != null)
        {
            ProcessDrop(eventData);
            DestroyGhost();
        }

        CancelHold();
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("CycleSlot"))
            {
                CycleSlot cs = result.gameObject.GetComponent<CycleSlot>();
                cs.ResetSlot();
                return true;
            }
        }

        return false;
    }

    private void CancelHold()
    {
        _isCanceled = true;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    private void DestroyGhost()
    {
        if (_ghost != null)
        {
            _ghost.transform.SetParent(null);
            Destroy(_ghost);
            _ghost = null;
            _ghostRect = null;
        }
    }
}
