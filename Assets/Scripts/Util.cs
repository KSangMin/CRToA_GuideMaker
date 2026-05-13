using System;
using UnityEngine;

public static class Util
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : MonoBehaviour
    {
        return go.TryGetComponent<T>(out T component) ?  component : go.AddComponent<T>();
    }

    /// <summary>
    /// Resources/Prefabs 폴더 내의 지정된 경로에서 Prefab을 로드하여 하이어라키에 추가합니다.
    /// </summary>
    public static GameObject InstantiatePrefab(string path, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
    {
        GameObject go = Resources.Load<GameObject>($"Prefabs/{path}");
        if (go == null)
        {
            throw new InvalidOperationException($"Failed to Load Prefab: {path}");
        }

        if(parent != null) return GameObject.Instantiate(go, parent, false);
        else return GameObject.Instantiate(go, position, rotation, parent);
    }

    /// <summary>
    /// Resources/Prefabs 폴더 내의 지정된 경로에서 Prefab을 로드하여 추가된 하이어라키의 컴포넌트를 가져옵니다.
    /// </summary>
    public static T InstantiatePrefabAndGetComponent<T>(string path, Vector3 position = default, Quaternion rotation = default, Transform parent = null) where T : Component
    {
        T comp = InstantiatePrefab(path, position, rotation, parent).GetComponent<T>();
        if (comp == null)
        {
            throw new InvalidOperationException($"Prefab instantiated but component of type {typeof(T)} not found in {path}");
        }
        return comp;
    }

    /// <summary>
    /// 바이트(byte)를 메가바이트(MB)로 변환하여 int로 반환합니다.
    /// </summary>
    public static int ConversionToMB(long bytes)
    {
        return Mathf.RoundToInt(bytes / (1024f * 1024f));
    }

    public static int ConversionToMB(float bytes)
    {
        return Mathf.RoundToInt(bytes / (1024f * 1024f));
    }

    public static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    public static void SetPosToNearTargetTopLeft(
    this RectTransform rect,
    RectTransform target,
    Vector2 offset = default)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        // target 좌상단 월드 좌표
        Vector3[] targetCorners = new Vector3[4];
        target.GetWorldCorners(targetCorners);

        Vector3 worldTopLeft = targetCorners[1];

        RectTransform parentRect = rect.parent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(cam, worldTopLeft),
            cam,
            out Vector2 localPoint);

        // rect 자신의 좌상단 보정
        Vector2 pivotOffset = new(
            rect.rect.width * rect.pivot.x,
            -rect.rect.height * (1 - rect.pivot.y));

        rect.anchoredPosition = localPoint + pivotOffset + offset;
    }
}
