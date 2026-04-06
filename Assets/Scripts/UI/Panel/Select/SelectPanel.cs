using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectPanel : MonoBehaviour
{
    private string _id = "";
    private CookieData _data;

    [SerializeField] private GameObject _slotPrefab;

    [Header("상단")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image classIcon;
    [SerializeField] private Image attackTypeIcon;
    [SerializeField] private Image rarityFrame;
    [SerializeField] private TextMeshProUGUI rarityText;

    [Header("스크롤뷰")]
    private ScrollRect _menuScroll;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image elementalTypeIcon;

    private void Awake()
    {
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetPanel(CookieData data, ScrollRect menuScroll)
    {
        _id = data.cookieId;
        _data = data;
        _menuScroll = menuScroll;
        scrollRect.GetComponent<SelectPanelScrollRect>().Init(menuScroll);

        DefaultSpriteData spriteData = AddressableManager.Instance.SpriteData;
        icon.sprite = data.GetSprite(data.Icon);
        nameText.SetText(data.cookieName);
        classIcon.sprite = spriteData.GetSprite(spriteData.classType[(int)data.Class]);
        attackTypeIcon.sprite = spriteData.GetSprite(spriteData.attackType[(int)data.AttackType]);
        rarityFrame.sprite = spriteData.GetSprite(spriteData.rarity_frame[(int)data.rarity]);
        rarityText.SetText(data.rarity.ToString());

        panelBackground.sprite = spriteData.GetSprite(spriteData.elementalType_frame[(int)data.Type]);
        elementalTypeIcon.sprite = spriteData.GetSprite(spriteData.elementalType[(int)data.Type]);

        SetSkillSlots();
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
        foreach(var sprites in _data.GetBasicAttackSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot(scrollRect, _id, SkillType.Basic, sprites.controlType, sprites.sprite);
        }
    }

    private void SetSpecialSkillSlots()
    {
        foreach (var sprites in _data.GetSpecialAttackSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot( scrollRect, _id, SkillType.SpecialSkill, sprites.controlType, sprites.sprite);
        }
    }

    private void SetUltimateSlots()
    {
        foreach (var sprites in _data.GetUltimateSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot(scrollRect, _id, SkillType.Ultimate, sprites.controlType, sprites.sprite);
        }
    }

    private void SetDashSlots()
    {
        foreach (var sprites in _data.GetDashSprites())
        {
            SelectSlot slot = Instantiate(_slotPrefab, content).GetComponent<SelectSlot>();
            slot.SetSlot(scrollRect, _id, SkillType.Dash, sprites.controlType, sprites.sprite);
        }
    }
}
