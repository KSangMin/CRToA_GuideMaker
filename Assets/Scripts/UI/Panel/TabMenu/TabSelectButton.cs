using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TabSelectButton : MonoBehaviour
{
    public int id = -1;
    [SerializeField] private GameObject panel;
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

    public void SelectButton(bool flag)
    {
        GetComponent<Image>().color = flag ? selectedColor : notSelectedColor;
        panel.SetActive(flag);
    }
}
