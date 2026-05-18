using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : DraggableSlot
{
    private string _id = "";

    private ScrollRect _panelScroll;

    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject chargeBackground;
    [SerializeField] private Image icon;
    [SerializeField] private Image head;
    [SerializeField] private TextMeshProUGUI nameText;
    private ControlType _controlType = ControlType.Normal;
    private CycleSlot _ghostSlot;

    public void SetSlot(ScrollRect panelScroll, string id, SkillType skillType, ControlType controlType, Sprite skillIcon,
        Sprite headIcon)
    {
        _panelScroll = panelScroll;
        _id = id;
        icon.sprite = skillIcon;
        head.sprite = headIcon;
        _controlType = controlType;
        if (controlType == ControlType.Charge)
        {
            chargeBackground.SetActive(true);
            nameText.SetText(string.Format("{0}: 차징", GetAttackText(skillType)));
        }
        else
        {
            chargeBackground.SetActive(false);
            nameText.SetText(GetAttackText(skillType));
        }
    }

    private string GetAttackText(SkillType skillType)
    {
        string result = "";

        switch (skillType)
        {
            case SkillType.Basic:
                result = "기본 공격";
                break;
            case SkillType.SpecialSkill:
                result = "특수 스킬";
                break;
            case SkillType.Ultimate:
                result = "궁극기";
                break;
            case SkillType.Dash:
                result = "대시";
                break;
            default:
                break;
        }

        return result;
    }

    protected override void OnSlotPointerDown(PointerEventData eventData)
    {
        holdCanceled = false;
        holdCoroutine = StartCoroutine(WaitHoldThen(eventData, OnHoldElapsedCreateGhost));
    }

    private void OnHoldElapsedCreateGhost(PointerEventData eventData)
    {
        _ghostSlot = CreateSlot(eventData);
    }

    private CycleSlot CreateSlot(PointerEventData eventData)
    {
        Transform ghostParent = UIManager.Instance.GetUI<UI_Panel>().forGhostParent;
        CycleSlot slot = Instantiate(ghostPrefab, ghostParent)
            .GetComponent<CycleSlot>();
        slot.name = $"Slot_{head.sprite.name}_{nameText.text}";
        slot.SetSlot(icon.sprite, _controlType, head.sprite, nameText.text);
        slot.SetPositionToPointer(eventData);

        return slot;
    }

    protected override void OnSlotPointerUp(PointerEventData eventData)
    {
        ResolveClickOrDragDropPointerUp(eventData);
    }

    protected override void BeforeSlotPointerUp(PointerEventData eventData)
    {
        CancelHoldTracking();
    }

    protected override bool IsDragDropActive()
    {
        return _ghostSlot != null;
    }

    protected override void OnSlotClick(PointerEventData eventData)
    {
        UIManager.Instance.GetUI<UI_Result>().cyclePanel.AddSlotToLast(CreateSlot(eventData));
    }

    protected override void OnSlotDragDrop(PointerEventData eventData)
    {
        int targetIndex = _ghostSlot.GetPlaceHolderIndex();
        _ghostSlot.ClearPlaceHolder();

        if (!TryDropOnCycleLayouts(_ghostSlot, targetIndex, eventData))
        {
            Destroy(_ghostSlot.gameObject);
        }

        _ghostSlot = null;
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        if (_ghostSlot != null)
        {
            return;
        }

        CancelHoldTracking();
        TryForwardScrollOrDraggable(eventData);
    }

    protected override bool ShouldForwardScrollDrag(PointerEventData eventData) => true;

    protected override ScrollRect ResolveScrollRectForDrag(PointerEventData eventData) => _panelScroll;

    protected override void OnBeforeScrollDragForwarded(PointerEventData eventData) { }

    protected override void OnDraggableDrag(PointerEventData eventData)
    {
        if (_ghostSlot != null)
        {
            _ghostSlot.Drag(eventData);
        }
    }

    protected override void OnDraggableEndDrag(PointerEventData eventData)
    {
        CancelHoldTracking();
    }
}
