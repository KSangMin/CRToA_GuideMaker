using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum IconType
{
    Cookie,
    Artifact,
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
        Debug.Log("°í½ºÆ® »ý¼ºµÊ");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost == null) { return; }

        _ghost.transform.position = eventData.position;
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
                _ghost.transform.position = slot.transform.position;
                _ghost.transform.SetParent(slot.transform);
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
}
