using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : SelectBaseSlot
{
    #region Serialized Fields

    [SerializeField] private GameObject chargeBackground;
    [SerializeField] protected Image icon;
    [SerializeField] private Image head;
    [SerializeField] private TextMeshProUGUI nameText;

    #endregion

    #region Private Fields

    private ControlType _controlType = ControlType.Normal;

    #endregion

    #region Protected Properties

    protected override Sprite GhostSkillIcon => icon.sprite;
    protected override ControlType GhostControlType => _controlType;
    protected override Sprite GhostHeadIcon => head.sprite;
    protected override string GhostName => nameText.text;

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

    #endregion
}
