using System.Windows.Forms.VisualStyles;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;

public class SwapSlot : SelectSlot
{
    public void SetSlot(ScrollRect panelScroll, string id, Sprite headIcon)
    {
        _panelScroll = panelScroll;
        _id = id;
        head.sprite = headIcon;
    }
}
