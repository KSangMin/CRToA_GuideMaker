using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CycleSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    private Transform _originalParent;
    private Coroutine _holdCoroutine;
    private float holdTime = 0.2f;
    private bool _isCanceled = false;

    public void SetSlot(Sprite sprite, string text)
    {
        _icon.sprite = sprite;
        _nameText.SetText(text);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _holdCoroutine = StartCoroutine(CheckHoldAfterDelay(eventData));
    }

    private IEnumerator CheckHoldAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdTime);

        if (!_isCanceled && !eventData.dragging)
        {
            transform.position = eventData.position;
            _originalParent = transform.parent;
            _originalParent.GetComponent<CycleHorizontalLayout>().RemoveFromSlot(this);
            transform.SetParent(UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
        }

        _holdCoroutine = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    private void CancelHold()
    {
        transform.SetParent(_originalParent);
        _isCanceled = true;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!ProcessDrop(eventData))
        {
            CancelHold();
        }
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("CycleHorizontalLayout"))
            {
                CycleHorizontalLayout hz = result.gameObject.GetComponent<CycleHorizontalLayout>();
                hz.AddSlot(this);
                return true;
            }
            else if (result.gameObject.CompareTag("CyclePanel"))
            {
                CyclePanel cyclePanel = result.gameObject.GetComponent<CyclePanel>();
                cyclePanel.AddSlotToLast(this);
                return true;
            }
        }

        return false;
    }
}