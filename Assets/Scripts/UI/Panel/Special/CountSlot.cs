using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CountSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject ghostObject;
    [SerializeField] private TextMeshProUGUI countText;

    private ScrollRect _panelScroll;
    private bool _isDraggingScroll = false;

    private GameObject _ghost;
    private RectTransform _ghostRect;
    private Coroutine _holdCoroutine;
    private float _holdTime = 0.15f;
    private bool _isCanceled = false;

    private int _minCount = 2;
    private int _curCount = 2;
    private int _maxCount = 9;

    private void Start()
    {
        SetCountText();
    }

    public void SetSlot(ScrollRect panelScroll)
    {
        _panelScroll = panelScroll;
    }

    private void SetCountText()
    {
        countText.SetText($"{_curCount}");
    }

    private void UpCount()
    {
        _curCount++;

        if (_curCount > _maxCount)
        {
            _curCount = _minCount;
        }

        SetCountText();
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

        if (!eventData.dragging && _ghost == null)//드래그하지 않고 클릭 시
        {
            UpCount();
        }

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
                cs.SetSlotCount(_curCount);
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
