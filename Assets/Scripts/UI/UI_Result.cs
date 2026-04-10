using System;
using UnityEngine;

public class UI_Result : UI
{
    public CyclePanel cyclePanel;
    [SerializeField] private Camera captureCamera;
    [SerializeField] private Canvas canvas;

    public Texture2D GetCycleTexture()
    {
        captureCamera.gameObject.SetActive(true);
        Texture2D result = cyclePanel.GetCycleTexture(captureCamera, canvas);
        captureCamera.gameObject.SetActive(false);
        return result;
    }
}
