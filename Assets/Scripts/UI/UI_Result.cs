using System;
using UnityEngine;

public class UI_Result : UI
{
    public CyclePanel cyclePanel;
    [SerializeField] private Camera captureCamera;
    [SerializeField] private Canvas captureCanvas;
    [SerializeField] private Canvas canvas;

    public void ResetCycle()
    {
        cyclePanel.ResetCycle();
    }

    public Texture2D GetCycleTexture()
    {
        captureCamera.gameObject.SetActive(true);
        captureCanvas.gameObject.SetActive(true);
        Texture2D result = cyclePanel.GetCycleTexture(captureCamera, captureCanvas, canvas);
        captureCanvas.gameObject.SetActive(false);
        captureCamera.gameObject.SetActive(false);
        return result;
    }
}
