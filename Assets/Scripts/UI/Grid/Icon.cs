using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Icon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    private Slot _parentSlot;

    private float halfSlotSize = 51.25f;
    private float _width;
    private float _height;
    private int _widthModifier = 1;
    private int _heightModifier = 1;

    private Vector2 _startPosition;
    private Coroutine _holdCoroutine;
    private float holdTime = 0.2f;
    private bool _isCanceled = false;

    private void Awake()
    {
        _width = GetComponent<RectTransform>().sizeDelta.x;
        _height = GetComponent<RectTransform>().sizeDelta.y;
    }

    public void SetIcon(int w, int h, Sprite sprite)
    {
        _widthModifier = w;
        _heightModifier = h;
        GetComponent<Image>().sprite = sprite;
        SetRect(w, h);
    }

    public void SetRect(int w, int h)
    {
        GetComponent<RectTransform>().sizeDelta = new(_width * _widthModifier, _height * _heightModifier);
    }

    public void SetParentSlot(Slot parent)
    {
        _parentSlot = parent;

        transform.SetParent(_parentSlot.transform);
        transform.position = _parentSlot.transform.position;

        GetComponent<Image>().raycastTarget = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _startPosition = eventData.position;
        _holdCoroutine = StartCoroutine(waitForHold(eventData));
    }

    private IEnumerator waitForHold(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdTime);

        if (!_isCanceled)
        {
            transform.SetParent(UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
            transform.position = eventData.position + new Vector2(-halfSlotSize, halfSlotSize);
        }

        _holdCoroutine = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (!eventData.dragging)
        {
            SetParentSlot(_parentSlot);
            //TODO: 크기 설정 관련 코드 추가
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        float distance = Vector2.Distance(_startPosition, eventData.position);
        if (!_isCanceled && distance > EventSystem.current.pixelDragThreshold)
        {
            CancelHold();
            SetParentSlot(_parentSlot);

            return;
        }

        transform.position = eventData.position + new Vector2(-halfSlotSize, halfSlotSize);
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

    public void OnEndDrag(PointerEventData eventData)
    {
        CancelHold();

        if (!ProcessDrop(eventData))
        {
            SetParentSlot(_parentSlot);
        }
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<UI_Panel>() != null)
            {
                return false;
            }
            else if (result.gameObject.TryGetComponent<Slot>(out Slot slot))
            {
                if (slot == _parentSlot)
                {
                    return false;
                }

                _parentSlot.ClearSlot();
                slot.SetIconToSlot(gameObject, new(_widthModifier, _heightModifier));

                return true;
            }
        }

        return false;
    }
}
