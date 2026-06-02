using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CyclePanel : MonoBehaviour
{
    [Header("References")]
    public CycleVerticalLayout vert;
    public AreaOverlayPanel area;
    public ArrowOverlayPanel arrow;
    [SerializeField] private Transform verticalLayout;
    [SerializeField] private Transform content;
    public GameObject cycleTitle;
    [SerializeField] private TMP_InputField titleInput;
    [SerializeField] private ColorEventChannel onFontColorChanged;
    
    private float _captureScale = 1.8286f;

    private void Awake()
    {
        if (titleInput == null) Debug.LogError("[CyclePanel] titleInput is missing!");
        if (content == null) Debug.LogError("[CyclePanel] content is missing!");

        if (onFontColorChanged != null)
        {
            onFontColorChanged.RegisterListener(SetTitleColor);
        }

        // onValueChanged 대신 LateUpdate에서 높이 변화를 완벽하게 감지합니다.
    }

    private void OnDestroy()
    {
        if (onFontColorChanged != null)
        {
            onFontColorChanged.UnregisterListener(SetTitleColor);
        }
    }

    private float _lastTitleHeight = -1f;
    private string _lastText = null;
    private float _lastWidth = -1f;

    private void LateUpdate()
    {
        if (titleInput != null && titleInput.textComponent != null)
        {
            string currentText = titleInput.text;
            float currentWidth = titleInput.textComponent.rectTransform.rect.width;

            // 텍스트 내용 또는 가로폭이 변했을 때만 갱신 검사 수행
            if (currentText != _lastText || Mathf.Abs(currentWidth - _lastWidth) > 0.1f)
            {
                _lastText = currentText;
                _lastWidth = currentWidth;

                // 레이아웃 계산을 위해 TMP_Text 메시 강제 갱신
                titleInput.textComponent.ForceMeshUpdate();
                float currentHeight = titleInput.textComponent.preferredHeight;

                // 텍스트가 완전히 비었을 때는 강제로 0으로 지정하여 최소 크기(50f)로 수축되도록 함
                if (string.IsNullOrEmpty(currentText))
                {
                    currentHeight = 0f;
                }

                if (Mathf.Abs(_lastTitleHeight - currentHeight) > 1f)
                {
                    _lastTitleHeight = currentHeight;
                    ApplyTitleHeight(currentHeight);
                    RebuildLayout();
                }
            }
        }
    }

    private void ApplyTitleHeight(float targetHeight)
    {
        var layoutElement = titleInput.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            float padding = 0f;
            if (titleInput.textViewport != null)
            {
                padding = Mathf.Abs(titleInput.textViewport.offsetMax.y) + Mathf.Abs(titleInput.textViewport.offsetMin.y);
            }
            layoutElement.preferredHeight = Mathf.Max(50f, targetHeight + padding + 10f);
        }

        // TMP 내부 스크롤 로직 무력화
        titleInput.textComponent.rectTransform.anchoredPosition = Vector2.zero;
    }

    private void SetTitleColor(Color color)
    {
        if (titleInput != null && titleInput.textComponent != null)
        {
            titleInput.textComponent.color = color;
        }
    }

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

    public void AddSlotToLast(CycleSlot slot)
    {
        vert.AddSlotToLast(slot);
    }

    public void AddSlotToNewRow(CycleSlot slot)
    {
        vert.AddSlotToNewRow(slot);
    }

    public void CheckRowEmpty()
    {
        vert.CheckRowEmpty();
    }

    public void RebuildLayout()
    {
        // 1. 자식(TitleWrapper) 리빌드
        if (titleInput != null && titleInput.transform.parent != null)
        {
            var titleWrapperRect = titleInput.transform.parent.GetComponent<RectTransform>();
            if (titleWrapperRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(titleWrapperRect);
            }
        }

        // 2. 최상위 content의 레이아웃 리빌드 (2연속 리빌드로 ContentSizeFitter 지연 해결)
        if (content != null)
        {
            var contentRect = content.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
                Canvas.ForceUpdateCanvases();
            }
        }

        // 3. 레이아웃 리빌드 완료 이벤트 전파
        if (vert.onLayoutRebuiltEvent != null)
        {
            vert.onLayoutRebuiltEvent.RaiseEvent();
        }
    }

    public Texture2D GetCycleTexture(Camera captureCamera, Canvas captureCanvas, Canvas canvas)
    {
        bool wasTitleActive = cycleTitle != null && cycleTitle.activeSelf;
        bool isTitleEmpty = titleInput != null && string.IsNullOrWhiteSpace(titleInput.text);

        // 제목이 비어있다면 캡처본에서 아예 영역 자체를 날려버림 (OptionPanel 토글 끄는 것과 동일한 효과)
        if (isTitleEmpty && wasTitleActive && cycleTitle != null)
        {
            cycleTitle.SetActive(false);
            RebuildLayout();
        }

        RectTransform targetRect = content.GetComponent<RectTransform>();

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

            // 캡처가 끝나면 유저가 OptionPanel에서 설정했던 상태(wasTitleActive)로 완벽하게 원상 복구
            if (isTitleEmpty && wasTitleActive && cycleTitle != null)
            {
                cycleTitle.SetActive(true);
                RebuildLayout();
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
        vert.ResetCycle();
    }
}