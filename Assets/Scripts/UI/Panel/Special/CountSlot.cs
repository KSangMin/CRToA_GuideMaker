using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CountSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject ghostObject;
    [SerializeField] private TextMeshProUGUI countText;

    private GameObject _ghost;
    private RectTransform _ghostRect;
    private Coroutine _holdCoroutine;
    private float _holdTime = 0.15f;
    private bool _isCanceled = false;
    private bool _canDrag = false;

    private int _minCount = 2;
    private int _curCount = 2;
    private int _maxCount = 9;

    private void Start()
    {
        SetCountText();
    }

    private void SetCountText()
    {
        countText.SetText($"{_curCount}");
    }

    private void UpCount()
    {
        _curCount++;

        if(_curCount > _maxCount)
        {
            _curCount = _minCount;
        }

        SetCountText();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _canDrag = false;
        _holdCoroutine = StartCoroutine(CheckHoldAfterDelay(eventData));
    }

    private IEnumerator CheckHoldAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(_holdTime);

        if (!_isCanceled && !eventData.dragging)
        {
            _canDrag = true;

            CreateGhost(eventData);
            SetGhostPositionToPointer(eventData);
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
        if (!eventData.dragging)//클릭 시
        {
            CancelHold();
            UpCount();
        }

        if (_ghost != null)
        {
            _ghost.transform.SetParent(null);
            Destroy(_ghost);
            _ghost = null;
            _ghostRect = null;
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

    public void OnDrag(PointerEventData eventData)
    {
        if (!_canDrag)
        {
            _isCanceled = true;
            return;
        }

        SetGhostPositionToPointer(eventData);
    }

    private void CancelHold()
    {
        _isCanceled = true;
        _canDrag = false;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_canDrag)
        {
            CancelHold();
            return;
        }

        if (!ProcessDrop(eventData))
        {
            CancelHold();
        }
        else
        {
            _isCanceled = true;
            _canDrag = false;
        }
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
}
