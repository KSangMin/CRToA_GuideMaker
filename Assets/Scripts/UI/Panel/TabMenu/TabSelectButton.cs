using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TabSelectButton : MonoBehaviour
{
    public int id = -1;
    [SerializeField] private TabScrollView panel;
    [SerializeField] private TabController tabController;
    private Button button;
    private Image buttonImage;

    private Color selectedColor = Color.white;
    private Color notSelectedColor = Color.gray;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(() => tabController.SelectTab(id));
    }

    public void Init(string label)
    {
        panel.Init(label);
    }

    public void SelectButton(bool flag)
    {
        buttonImage.color = flag ? selectedColor : notSelectedColor;
        panel.gameObject.SetActive(flag);
    }
}
