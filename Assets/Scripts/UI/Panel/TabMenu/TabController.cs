using System.Collections.Generic;
using UnityEngine;

public class TabController : MonoBehaviour
{
    [SerializeField] private List<TabSelectButton> buttons = new();

    private void Awake()
    {
        SelectTab(0);
    }

    private void Start()
    {
        for(int i = 0; i < AddressableManager.Instance.Labels.Count; i++)
        {
            buttons[i].Init(AddressableManager.Instance.Labels[i]);
        }
    }

    public void SelectTab(int id)
    {
        foreach (TabSelectButton button in buttons)
        {
            button.SelectButton(button.id == id);
        }
    }
}
