using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum IconType
{
    Cookie,
    Artifact,
    Thumbnail,
    Header,
    Card,
    Equipment,
    Potential,
    Seasonite,
}

public class TabSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    private IconType _type;
    private int _id = -1;

    private float halfSlotSize = 51.25f;

    private ScrollRect _parentScroll;

    [SerializeField] private GameObject iconPrefab;
    private GameObject _ghost;
    private Vector2 _startPosition;
    private Coroutine _holdCoroutine;
    [SerializeField] private float holdTime = 0.15f;
    private bool _isCanceled = false;

    public void SetSlot(ScrollRect scroll, IconType type, int id, Sprite sprite)
    {
        _parentScroll = scroll;
        _type = type;
        _id = id;
        GetComponent<Image>().sprite = sprite;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _startPosition = eventData.position;
        _holdCoroutine = StartCoroutine(CreateGhostAfterDelay(eventData));
    }

    private IEnumerator CreateGhostAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdTime);

        if (!_isCanceled)
        {
            _ghost = Instantiate(iconPrefab, UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
            KeyValuePair<int, int> wh = GetWidthHeight();
            _ghost.GetComponent<Icon>().SetIcon(wh.Key, wh.Value, GetComponent<Image>().sprite);

            _ghost.transform.position = eventData.position + new Vector2(-halfSlotSize, halfSlotSize);
        }
        
        _holdCoroutine = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (_ghost != null)
        {
            if (!eventData.dragging)
            {
                Destroy(_ghost);
                _ghost = null;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost == null)
        {
            float distance = Vector2.Distance(_startPosition, eventData.position);
            if (!_isCanceled && distance > EventSystem.current.pixelDragThreshold)
            {
                CancelHold();

                if (_parentScroll != null)
                {
                    _parentScroll.OnInitializePotentialDrag(eventData);
                    _parentScroll.OnBeginDrag(eventData);
                }
            }
            if (_isCanceled && _parentScroll != null)
            {
                _parentScroll.OnDrag(eventData);
            }
            return;
        }

        _ghost.transform.position = eventData.position + new Vector2(-halfSlotSize, halfSlotSize);
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

        if (_ghost == null)
        {
            if (_parentScroll != null)
            {
                _parentScroll.OnEndDrag(eventData);
            }
            return;
        }

        if (!ProcessDrop(eventData))
        {
            Destroy(_ghost);
        }
        _ghost = null;
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach(var result in results)
        {
            if (result.gameObject.GetComponent<UI_Panel>() != null)
            {
                return false;
            }
            else if (result.gameObject.TryGetComponent<Slot>(out Slot slot))
            {
                slot.SetIconToSlot(_ghost, GetWidthHeight());

                return true;
            }
        }

        return false;
    }

    private KeyValuePair<int, int> GetWidthHeight()
    {
        KeyValuePair<int, int> wh = new(1, 1);
        switch (_type)
        {
            case IconType.Header:
                wh = new(2, 1);
                break;
            case IconType.Card:
                wh = new(1, 2);
                break;
            default:
                break;
        }

        return wh;
    }
}
