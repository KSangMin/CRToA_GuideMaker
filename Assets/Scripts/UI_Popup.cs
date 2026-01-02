using System;
using UnityEngine;

public class UI_Popup : UI
{
    Canvas canvas;

    public Action destroyAction;

    protected override void Awake()
    {
        base.Awake();

        canvas = GetComponent<Canvas>();
    }

    public void SetCanvasOrder(int order)
    {
        canvas.sortingOrder = order;
    }

    public override void Close()
    {
        destroyAction?.Invoke();

        UIManager.Instance.RemovePopupUI();
    }
}
