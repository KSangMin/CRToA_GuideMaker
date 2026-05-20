using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectPanel : MonoBehaviour
{
    private string _id = "";
    private CookieData _data;

    [SerializeField] private GameObject _swapSlotPrefab;
    [SerializeField] private GameObject _slotPrefab;

    [Header("상단")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image classIcon;
    [SerializeField] private Image attackTypeIcon;
    [SerializeField] private Image rarityFrame;
    [SerializeField] private TextMeshProUGUI rarityText;

    [Header("스크롤뷰")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image elementalTypeIcon;

    private void Awake()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetPanel(CookieData data, ScrollRect menuScroll)
    {
        _id = data.cookieId;
        _data = data;
        scrollRect.GetComponent<PanelScrollRect>().Init(menuScroll);

        DefaultSpriteData spriteData = AddressableManager.Instance.SpriteData;
        icon.sprite = data.GetSprite(data.Icon);

        nameText.SetText(data.cookieName);
        nameText.ForceMeshUpdate();

        classIcon.sprite = spriteData.GetSprite(spriteData.classType[(int)data.Class]);
        attackTypeIcon.sprite = spriteData.GetSprite(spriteData.attackType[(int)data.AttackType]);
        rarityFrame.sprite = spriteData.GetSprite(spriteData.rarity_frame[(int)data.rarity]);

        rarityText.SetText(data.rarity.ToString());
        rarityText.ForceMeshUpdate();

        panelBackground.sprite = spriteData.GetSprite(spriteData.elementalType_frame[(int)data.Type]);
        elementalTypeIcon.sprite = spriteData.GetSprite(spriteData.elementalType[(int)data.Type]);

        SetSwapSlot();
        SetSkillSlots();

        LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rarityText.rectTransform);
    }

    private void SetSwapSlot()
    {
        SwapSlot slot = Instantiate(_swapSlotPrefab, content).GetComponent<SwapSlot>();
        slot.SetSlot(scrollRect, _id, icon.sprite);
    }

    private void SetSkillSlots()
    {
        SetBasicAttackSlots();
        SetDashSlots();
        SetSpecialSkillSlots();
        SetUltimateSlots();
    }

    private void SetBasicAttackSlots()
    {
        foreach (var basicAttack in _data.GetBasicAttackSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot(scrollRect, _id, SkillType.Basic, basicAttack.controlType, basicAttack.sprite, icon.sprite);
        }
    }

    private void SetSpecialSkillSlots()
    {
        foreach (var specialSkill in _data.GetSpecialAttackSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot(scrollRect, _id, SkillType.SpecialSkill, specialSkill.controlType, specialSkill.sprite, icon.sprite);
        }
    }

    private void SetUltimateSlots()
    {
        foreach (var ultimate in _data.GetUltimateSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot(scrollRect, _id, SkillType.Ultimate, ultimate.controlType, ultimate.sprite, icon.sprite);
        }
    }

    private void SetDashSlots()
    {
        foreach (var dash in _data.GetDashSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot(scrollRect, _id, SkillType.Dash, dash.controlType, dash.sprite, icon.sprite);
        }
    }
}
