using System.Collections.Generic;
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

public class TabSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private IconType _type;
    private int _id = -1;

    [SerializeField] private GameObject iconPrefab;

    private GameObject _ghost;

    public void SetSlot(IconType type, int id, Sprite sprite)
    {
        _type = type;
        _id = id;
        GetComponent<Image>().sprite = sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _ghost = Instantiate(iconPrefab, UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
        KeyValuePair<int, int> wh = GetWidthHeight();
        _ghost.GetComponent<Icon>().SetIcon(wh.Key, wh.Value, GetComponent<Image>().sprite);
        Debug.Log("°í½ºÆ® »ý¼ºµÊ");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost == null) { return; }

        _ghost.transform.position = eventData.position + new Vector2(-51.25f, 51.25f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_ghost == null) { return; }

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        bool success = false;
        if(results.Count > 0)
        {
            if (results[0].gameObject.TryGetComponent<Slot>(out Slot slot))
            {
                slot.CheckSlot(GetWidthHeight());
                _ghost.transform.SetParent(slot.transform);
                _ghost.transform.position = slot.transform.position;
                success = true;
                Debug.Log("slot Å½ÁöµÊ");
            }
        }

        if (!success)
        {
            Destroy(_ghost);
        }

        _ghost = null;
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
