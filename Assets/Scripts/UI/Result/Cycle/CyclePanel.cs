using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CyclePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform verticalLayout;
    private CycleVerticalLayout _vert;

    private void Awake()
    {
        _vert = verticalLayout.GetComponent<CycleVerticalLayout>();
    }

    public void AddSlotToLast(CycleSlot slot)
    {
        _vert.AddSlotToLast(slot);
    }

    public void AddSlotToNewRow(CycleSlot slot)
    {
        _vert.AddSlotToNewRow(slot);
    }

    public void CheckRowEmpty()
    {
        _vert.CheckRowEmpty();
    }

    public void RebuildLayout()
    {
        _vert.RebuildLayout();
    }

    public Texture2D GetCycleTexture(Camera captureCamera, Canvas canvas)
    {
        RectTransform targetRect = _vert.GetComponent<RectTransform>();

        Camera previousCanvasCamera = canvas.worldCamera;
        canvas.worldCamera = captureCamera;

        // Canvas 전체 크기로 RenderTexture 생성
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        RenderTexture previousRT = captureCamera.targetTexture;
        captureCamera.targetTexture = rt;
        captureCamera.Render();

        // targetRect의 월드 코너 → 스크린 좌표 변환
        Vector3[] corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);

        Vector3 screenCornerMin = captureCamera.WorldToScreenPoint(corners[0]); // 좌하
        Vector3 screenCornerMax = captureCamera.WorldToScreenPoint(corners[2]); // 우상

        // 시작점(좌하단)은 올림, 끝점(우상단)은 내림 → 안쪽 경계로 맞춤
        float minX = Mathf.CeilToInt(screenCornerMin.x);
        float minY = Mathf.CeilToInt(screenCornerMin.y);
        float maxX = Mathf.FloorToInt(screenCornerMax.x);
        float maxY = Mathf.FloorToInt(screenCornerMax.y);

        int screenWidth = (int)(maxX - minX);
        int screenHeight = (int)(maxY - minY);

        Texture2D result = new Texture2D(screenWidth, screenHeight, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;

        result.ReadPixels(
            new Rect(minX, minY, screenWidth, screenHeight),
            0, 0);
        result.Apply();

        captureCamera.targetTexture = previousRT;
        RenderTexture.active = null;
        rt.Release();
        Destroy(rt);

        canvas.worldCamera = previousCanvasCamera;

        return result;
    }
}
