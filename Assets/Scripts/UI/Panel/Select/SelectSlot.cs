using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : DraggableSlot
{
    #region Serialized Fields

    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject chargeBackground;
    [SerializeField] private Image icon;
    [SerializeField] private Image head;
    [SerializeField] private TextMeshProUGUI nameText;

    #endregion

    #region Private Fields

    private string _id = "";
    private ControlType _controlType = ControlType.Normal;
    private CycleSlot _ghostSlot;
    private int _targetIndex = -1;

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        if (ghostPrefab == null) Debug.LogError($"[{nameof(SelectSlot)}] ghostPrefab is not assigned on {name}.", this);
        if (chargeBackground == null) Debug.LogError($"[{nameof(SelectSlot)}] chargeBackground is not assigned on {name}.", this);
        if (icon == null) Debug.LogError($"[{nameof(SelectSlot)}] icon is not assigned on {name}.", this);
        if (head == null) Debug.LogError($"[{nameof(SelectSlot)}] head is not assigned on {name}.", this);
        if (nameText == null) Debug.LogError($"[{nameof(SelectSlot)}] nameText is not assigned on {name}.", this);
    }

    #endregion

    #region Public Methods

    public void SetSlot(ScrollRect panelScroll, string id, SkillType skillType, ControlType controlType, Sprite skillIcon, Sprite headIcon)
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

    #endregion

    #region Protected Methods

    protected override void OnSlotPointerDown(PointerEventData eventData)
    {
        WaitHoldThen(eventData, () => _ghostSlot = CreateSlot(eventData));
    }

    protected override void OnSlotPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (_ghostSlot == null && !eventData.dragging)
        {
            UIManager.Instance.GetUI<UI_Result>().cyclePanel.AddSlotToLast(CreateSlot(eventData));
        }

        if (_ghostSlot != null)
        {
            _targetIndex = _ghostSlot.GetPlaceHolderIndex();
            _ghostSlot.ClearPlaceHolder();

            if (!TryDropOnCycleLayouts(_ghostSlot, _targetIndex, eventData))
            {
                Destroy(_ghostSlot.gameObject);
            }

            _ghostSlot = null;
        }
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        TryBeginPanelScrollDrag(eventData, _ghostSlot != null);
    }

    protected override void OnSlotDrag(PointerEventData eventData)
    {
        if (_ghostSlot != null)
        {
            _ghostSlot.Drag(eventData);
            return;
        }

        ForwardPanelScrollDrag(eventData, _ghostSlot != null);
    }

    protected override void OnSlotEndDrag(PointerEventData eventData)
    {
        if (EndPanelScrollDrag(eventData))
        {
            return;
        }

        CancelHold();
    }

    #endregion

    #region Private Methods

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

    private CycleSlot CreateSlot(PointerEventData eventData)
    {
        CycleSlot slot = Instantiate(ghostPrefab, GetGhostParent())
            .GetComponent<CycleSlot>();
        slot.name = $"Slot_{head.sprite.name}_{nameText.text}";
        slot.SetSlot(icon.sprite, _controlType, head.sprite, nameText.text);
        slot.SetPositionToPointer(eventData);

        return slot;
    }

    #endregion
}
