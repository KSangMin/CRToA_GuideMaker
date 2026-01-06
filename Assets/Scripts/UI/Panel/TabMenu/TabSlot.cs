using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum IconType
{
    Cookie,
    Artifact,
    Equipment,
    Seasonite,
    Potential,
}

public class TabSlot : MonoBehaviour, IBeginDragHandler
{
    private IconType type;
    private int id;

    private Image icon;

    private void Awake()
    {
        icon = GetComponent<Image>();
    }

    public void SetSlot(IconType type, int id, Sprite sprite)
    {
        this.type = type;
        this.id = id;
        icon.sprite = sprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
