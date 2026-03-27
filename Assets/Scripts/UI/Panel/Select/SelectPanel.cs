using TMPro;
using UnityEngine;
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
    [SerializeField] private Transform content;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Image elementalTypeIcon;

    private void Awake()
    {
        foreach(Transform child in transform)
        {
            Destroy(child);
        }
    }

    public void SetPanel(
        CookieData data
        , Sprite headIconSprite
        , Sprite classIconSprite
        , Sprite attackTypeIconSprite
        , Sprite rarityFrameSprite
        , Sprite panelBackgroundSprite
        , Sprite elementalTypeIconSprite)
    {
        _id = data.cookieId;
        _data = data;
        
        icon.sprite = headIconSprite;
        nameText.SetText(data.cookieName);
        classIcon.sprite = classIconSprite;
        attackTypeIcon.sprite = attackTypeIconSprite;
        rarityFrame.sprite = rarityFrameSprite;
        rarityText.SetText(data.rarity.ToString());

        panelBackground.sprite = panelBackgroundSprite;
        elementalTypeIcon.sprite = elementalTypeIconSprite;

        SetSkillSlots();
    }

    private void SetSkillSlots()
    {
        SetBasicAttackSlots();
        SetSpecialSkillSlots();
        SetUltimateSlots();
        SetDashSlots();
    }

    private void SetBasicAttackSlots()
    {
        if(_data.BasicAttack.Count > 0)
        {

        }
        else
        {

        }
    }

    private void SetSpecialSkillSlots()
    {
        if (_data.SpecialAttack.Count > 0)
        {

        }
        else
        {

        }
    }

    private void SetUltimateSlots()
    {
        if (_data.Ultimate.Count > 0)
        {

        }
        else
        {

        }
    }

    private void SetDashSlots()
    {
        if (_data.Dash.Count > 0)
        {

        }
        else
        {

        }
    }
}
