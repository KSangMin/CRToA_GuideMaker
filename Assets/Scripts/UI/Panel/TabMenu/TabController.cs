using System.Collections.Generic;
using UnityEngine;

public class TabController : MonoBehaviour
{
    [SerializeField] private List<TabSelectButton> buttons = new();

    private void Awake()
    {
        SelectTab(0);
    }

    public void SelectTab(int id)
    {
        foreach (TabSelectButton button in buttons)
        {
            button.SelectButton(button.id == id);
        }
    }
}
