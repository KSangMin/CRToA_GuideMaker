using System.Collections.Generic;
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

        RenderTexture previousRT = captureCamera.targetTexture;
        RenderTexture previousActiveRT = RenderTexture.active;
        bool previousOrthographic = captureCamera.orthographic;
        float previousOrthographicSize = captureCamera.orthographicSize;
        Vector3 previousCameraPosition = captureCamera.transform.position;
        Quaternion previousCameraRotation = captureCamera.transform.rotation;
        int previousCullingMask = captureCamera.cullingMask;
        CameraClearFlags previousClearFlags = captureCamera.clearFlags;
        Color previousBackgroundColor = captureCamera.backgroundColor;

        Transform previousParent = targetRect.parent;
        int previousSiblingIndex = targetRect.GetSiblingIndex();
        Vector2 previousAnchorMin = targetRect.anchorMin;
        Vector2 previousAnchorMax = targetRect.anchorMax;
        Vector3 previousAnchoredPosition = targetRect.anchoredPosition3D;
        Vector2 previousSizeDelta = targetRect.sizeDelta;
        Vector2 previousPivot = targetRect.pivot;
        Quaternion previousLocalRotation = targetRect.localRotation;
        Vector3 previousLocalScale = targetRect.localScale;

        RenderTexture rt = null;
        Texture2D result = null;
        GameObject tempCanvasObject = null;
        List<Transform> layerTargets = new();
        List<int> previousLayers = new();

        try
        {
            Canvas.ForceUpdateCanvases();

            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);

            Camera sizeCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector3 screenCornerMin = RectTransformUtility.WorldToScreenPoint(sizeCamera, corners[0]);
            Vector3 screenCornerMax = RectTransformUtility.WorldToScreenPoint(sizeCamera, corners[2]);
            int textureWidth = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(screenCornerMax.x - screenCornerMin.x)));
            int textureHeight = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(screenCornerMax.y - screenCornerMin.y)));
            int captureLayer = targetRect.gameObject.layer;

            rt = new RenderTexture(textureWidth, textureHeight, 24);
            result = new Texture2D(textureWidth, textureHeight, TextureFormat.ARGB32, false);

            float targetWidth = Mathf.Max(1f, targetRect.rect.width);
            float targetHeight = Mathf.Max(1f, targetRect.rect.height);
            float renderTextureAspect = textureWidth / (float)textureHeight;

            tempCanvasObject = new GameObject("Cycle Capture Canvas", typeof(RectTransform), typeof(Canvas));
            tempCanvasObject.layer = captureLayer;

            Canvas tempCanvas = tempCanvasObject.GetComponent<Canvas>();
            tempCanvas.renderMode = RenderMode.WorldSpace;
            tempCanvas.worldCamera = captureCamera;
            tempCanvas.sortingOrder = short.MaxValue;

            RectTransform tempCanvasRect = tempCanvasObject.GetComponent<RectTransform>();
            tempCanvasRect.sizeDelta = new Vector2(targetWidth, targetHeight);
            tempCanvasRect.position = Vector3.zero;
            tempCanvasRect.rotation = Quaternion.identity;
            tempCanvasRect.localScale = Vector3.one;

            targetRect.SetParent(tempCanvasRect, false);
            targetRect.anchorMin = new Vector2(0.5f, 0.5f);
            targetRect.anchorMax = new Vector2(0.5f, 0.5f);
            targetRect.pivot = new Vector2(0.5f, 0.5f);
            targetRect.sizeDelta = new Vector2(targetWidth, targetHeight);
            targetRect.anchoredPosition = Vector2.zero;
            targetRect.localRotation = Quaternion.identity;
            targetRect.localScale = Vector3.one;

            SetLayerRecursively(targetRect, captureLayer, layerTargets, previousLayers);
            Canvas.ForceUpdateCanvases();

            captureCamera.transform.rotation = Quaternion.identity;
            captureCamera.transform.position = new Vector3(0f, 0f, -10f);
            captureCamera.orthographic = true;
            captureCamera.orthographicSize = Mathf.Max(targetHeight * 0.5f, targetWidth / renderTextureAspect * 0.5f);
            captureCamera.cullingMask = 1 << captureLayer;
            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            captureCamera.targetTexture = rt;

            captureCamera.Render();

            RenderTexture.active = rt;

            result.ReadPixels(
                new Rect(0, 0, textureWidth, textureHeight),
                0, 0);
            result.Apply();
        }
        finally
        {
            captureCamera.targetTexture = previousRT;
            captureCamera.orthographic = previousOrthographic;
            captureCamera.orthographicSize = previousOrthographicSize;
            captureCamera.transform.position = previousCameraPosition;
            captureCamera.transform.rotation = previousCameraRotation;
            captureCamera.cullingMask = previousCullingMask;
            captureCamera.clearFlags = previousClearFlags;
            captureCamera.backgroundColor = previousBackgroundColor;
            RenderTexture.active = previousActiveRT;
            if (rt != null)
            {
                rt.Release();
                Destroy(rt);
            }

            targetRect.SetParent(previousParent, false);
            targetRect.SetSiblingIndex(previousSiblingIndex);
            targetRect.anchorMin = previousAnchorMin;
            targetRect.anchorMax = previousAnchorMax;
            targetRect.pivot = previousPivot;
            targetRect.sizeDelta = previousSizeDelta;
            targetRect.anchoredPosition3D = previousAnchoredPosition;
            targetRect.localRotation = previousLocalRotation;
            targetRect.localScale = previousLocalScale;

            if (tempCanvasObject != null)
            {
                Destroy(tempCanvasObject);
            }

            RestoreLayers(layerTargets, previousLayers);
            Canvas.ForceUpdateCanvases();
        }

        return result;
    }

    private static void SetLayerRecursively(Transform target, int layer, List<Transform> targets, List<int> previousLayers)
    {
        targets.Add(target);
        previousLayers.Add(target.gameObject.layer);
        target.gameObject.layer = layer;

        for (int i = 0; i < target.childCount; i++)
        {
            SetLayerRecursively(target.GetChild(i), layer, targets, previousLayers);
        }
    }

    private static void RestoreLayers(List<Transform> targets, List<int> previousLayers)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                targets[i].gameObject.layer = previousLayers[i];
            }
        }
    }
}
