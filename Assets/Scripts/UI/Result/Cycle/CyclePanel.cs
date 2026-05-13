using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CyclePanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform verticalLayout;
    private CycleVerticalLayout _vert;
    private float _captureScale = 1.8286f;

    private readonly struct CameraState
    {
        readonly Camera _cam;
        readonly RenderTexture _rt, _activeRT;
        readonly bool _ortho;
        readonly float _orthoSize;
        readonly Vector3 _pos;
        readonly Quaternion _rot;
        readonly int _mask;
        readonly CameraClearFlags _flags;
        readonly Color _bg;

        internal CameraState(Camera cam)
        {
            _cam = cam;
            _rt = cam.targetTexture;
            _activeRT = RenderTexture.active;
            _ortho = cam.orthographic;
            _orthoSize = cam.orthographicSize;
            _pos = cam.transform.position;
            _rot = cam.transform.rotation;
            _mask = cam.cullingMask;
            _flags = cam.clearFlags;
            _bg = cam.backgroundColor;
        }

        internal void Restore()
        {
            _cam.targetTexture = _rt;
            _cam.orthographic = _ortho;
            _cam.orthographicSize = _orthoSize;
            _cam.transform.SetPositionAndRotation(_pos, _rot);
            _cam.cullingMask = _mask;
            _cam.clearFlags = _flags;
            _cam.backgroundColor = _bg;
            RenderTexture.active = _activeRT;
        }
    }

    private readonly struct RectTransformState
    {
        readonly RectTransform _rect;
        readonly Transform _parent;
        readonly int _siblingIndex;
        readonly Vector2 _anchorMin, _anchorMax, _sizeDelta, _pivot;
        readonly Vector3 _anchoredPos3D;
        readonly Quaternion _localRot;
        readonly Vector3 _localScale;

        internal RectTransformState(RectTransform rect)
        {
            _rect = rect;
            _parent = rect.parent;
            _siblingIndex = rect.GetSiblingIndex();
            _anchorMin = rect.anchorMin;
            _anchorMax = rect.anchorMax;
            _pivot = rect.pivot;
            _sizeDelta = rect.sizeDelta;
            _anchoredPos3D = rect.anchoredPosition3D;
            _localRot = rect.localRotation;
            _localScale = rect.localScale;
        }

        internal void Restore()
        {
            _rect.SetParent(_parent, false);
            _rect.SetSiblingIndex(_siblingIndex);
            _rect.anchorMin = _anchorMin;
            _rect.anchorMax = _anchorMax;
            _rect.pivot = _pivot;
            _rect.sizeDelta = _sizeDelta;
            _rect.anchoredPosition3D = _anchoredPos3D;
            _rect.localRotation = _localRot;
            _rect.localScale = _localScale;
        }
    }

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

    public Texture2D GetCycleTexture(Camera captureCamera, Canvas captureCanvas, Canvas canvas)
    {
        RectTransform targetRect = _vert.GetComponent<RectTransform>();

        var camState = new CameraState(captureCamera);
        var rectState = new RectTransformState(targetRect);

        RenderTexture rt = null;
        Texture2D result = null;

        try
        {
            Canvas.ForceUpdateCanvases();

            // 원본 Screen Space Canvas 기준으로 픽셀 크기 계산
            (int w, int h) = GetScreenSize(targetRect, canvas);
            int captureLayer = targetRect.gameObject.layer;

            rt = new RenderTexture(w, h, 24);
            result = new Texture2D(w, h, TextureFormat.ARGB32, false);

            SetCaptureCanvas(captureCanvas, targetRect, captureLayer);
            Canvas.ForceUpdateCanvases();

            SetupCaptureCamera(captureCamera, rt, targetRect.rect, w, h, captureLayer);
            captureCamera.Render();

            RenderTexture.active = rt;
            result.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            result.Apply();
        }
        finally
        {
            camState.Restore();
            rectState.Restore();

            if (rt != null)
            {
                rt.Release();
                Destroy(rt);
            }

            Canvas.ForceUpdateCanvases();
        }

        return result;
    }

    private (int w, int h) GetScreenSize(RectTransform targetRect, Canvas canvas)
    {
        // rect.rect.size는 브라우저 크기와 무관하게 고정값
        // scaleFactor를 곱하면 오히려 브라우저 크기에 따라 픽셀이 변하므로 사용하지 않음
        int w = Mathf.Max(1, Mathf.RoundToInt(targetRect.rect.width * _captureScale));
        int h = Mathf.Max(1, Mathf.RoundToInt(targetRect.rect.height * _captureScale));
        return (w, h);
    }

    private void SetCaptureCanvas(Canvas captureCanvas, RectTransform targetRect, int captureLayer)
    {
        float targetWidth = Mathf.Max(1f, targetRect.rect.width);
        float targetHeight = Mathf.Max(1f, targetRect.rect.height);

        var canvasRect = captureCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(targetWidth, targetHeight);
        canvasRect.position = Vector3.zero;

        targetRect.SetParent(canvasRect, false);
        targetRect.anchorMin = new Vector2(0.5f, 0.5f);
        targetRect.anchorMax = new Vector2(0.5f, 0.5f);
        targetRect.pivot = new Vector2(0.5f, 0.5f);
        targetRect.sizeDelta = new Vector2(targetWidth, targetHeight);
        targetRect.anchoredPosition = Vector2.zero;
    }

    private void SetupCaptureCamera(Camera cam, RenderTexture rt, Rect bounds, int w, int h, int captureLayer)
    {
        float aspect = w / (float)h;

        cam.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(bounds.height * 0.5f, bounds.width / aspect * 0.5f);
        cam.cullingMask = 1 << captureLayer;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        cam.targetTexture = rt;
    }

    public void ResetCycle()
    {
        _vert.ResetCycle();
    }
}