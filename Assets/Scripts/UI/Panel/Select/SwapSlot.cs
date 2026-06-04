using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwapSlot : SelectBaseSlot
{
    [SerializeField] private Image icon;
    [SerializeField] private Image swapIcon;

    private Sprite _headIcon;
    private string _name = "쿠키 스왑";

    protected override Sprite GhostSkillIcon => _headIcon;
    protected override ControlType GhostControlType => ControlType.Normal;
    protected override Sprite GhostHeadIcon => swapIcon.sprite;
    protected override string GhostName => _name;

    protected virtual void Awake()
    {
        if (ghostPrefab == null) Debug.LogError($"[{nameof(SwapSlot)}] ghostPrefab is not assigned on {name}.", this);
        if (icon == null) Debug.LogError($"[{nameof(SwapSlot)}] icon is not assigned on {name}.", this);
    }

    public void SetSlot(ScrollRect panelScroll, string id, Sprite headIcon)
    {
        _panelScroll = panelScroll;
        _id = id;
        _headIcon = headIcon;
        icon.sprite = headIcon;
    }
}
