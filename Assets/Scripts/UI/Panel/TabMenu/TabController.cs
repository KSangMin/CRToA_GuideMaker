using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
struct labelPanelPair
{
    public AssetLabelReference label;
    public TabScrollView panel;
    public labelPanelPair(AssetLabelReference label, TabScrollView panel)
    {
        this.label = label;
        this.panel = panel;
    }
}

public class TabController : MonoBehaviour
{
    [SerializeField] private List<TabSelectButton> buttons = new();
    [SerializeField] private List<labelPanelPair> panels = new();

    private void Start()
    {
        for(int i = 0; i < panels.Count; i++)
        {
            panels[i].panel.Init(panels[i].label.labelString);
        }

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
