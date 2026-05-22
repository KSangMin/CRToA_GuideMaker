using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CycleSlot : DraggableSlot
{
    #region Serialized Fields

    [SerializeField] private GameObject chargeBackground;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject countBackground;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image head;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("색깔 변경")]
    [SerializeField] private ColorEventChannel onFontColorChanged;

    [Header("주석")]
    [SerializeField] private TMP_InputField commentInput;

    #endregion

    #region Public Properties

    public List<string> StartAreaIds { get; } = new();
    public List<string> EndAreaIds { get; } = new();

    #endregion

    #region Private Fields

    [HideInInspector]
    public Transform originalParent;

    private bool _isDragging;
    private GameObject _placeholder;
    private ControlType _controlType = ControlType.Normal;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (chargeBackground == null) Debug.LogError($"[{nameof(CycleSlot)}] chargeBackground is not assigned on {name}.", this);
        if (icon == null) Debug.LogError($"[{nameof(CycleSlot)}] icon is not assigned on {name}.", this);
        if (countBackground == null) Debug.LogError($"[{nameof(CycleSlot)}] countBackground is not assigned on {name}.", this);
        if (countText == null) Debug.LogError($"[{nameof(CycleSlot)}] countText is not assigned on {name}.", this);
        if (head == null) Debug.LogError($"[{nameof(CycleSlot)}] head is not assigned on {name}.", this);
        if (nameText == null) Debug.LogError($"[{nameof(CycleSlot)}] nameText is not assigned on {name}.", this);
        if (onFontColorChanged == null) Debug.LogError($"[{nameof(CycleSlot)}] onFontColorChanged is not assigned on {name}.", this);

        if (onFontColorChanged != null)
        {
            onFontColorChanged.RegisterListener(SetFontColor);
        }

        if (commentInput != null)
        {
            commentInput.onValueChanged.AddListener(OnCommentValueChanged);
        }
    }

    private void Start()
    {
        countBackground.SetActive(false);
        commentInput.gameObject.SetActive(false);
        SetFontColor(UIManager.Instance.GetUI<UI_Result>().optionPanel.colorSelectPanel.CurFontColor);
    }

    #endregion

    #region Public Methods

    public void SetSlot(Sprite skillIcon, ControlType controlType, Sprite headIcon, string text)
    {
        icon.sprite = skillIcon;
        _controlType = controlType;
        if (controlType == ControlType.Charge)
        {
            chargeBackground.SetActive(true);
        }
        else if (controlType == ControlType.Normal)
        {
            chargeBackground.SetActive(false);
        }

        head.sprite = headIcon;
        nameText.SetText(text);
    }

    public void Drag(PointerEventData eventData)
    {
        SetPositionToPointer(eventData);
        CheckForPlaceHolder(eventData);
    }

    public void SetPositionToPointer(PointerEventData eventData)
    {
        SetRectTransformToPointer(GetComponent<RectTransform>(), eventData);
    }

    public int GetPlaceHolderIndex()
    {
        return _placeholder != null ? _placeholder.transform.GetSiblingIndex() : -1;
    }

    public void ClearPlaceHolder()
    {
        if (_placeholder != null)
        {
            _placeholder.transform.SetParent(GetGhostParent());
            Destroy(_placeholder);
            _placeholder = null;
        }
    }

    public void SetSlotCount(int count)
    {
        countBackground.SetActive(true);
        countText.SetText($"{count}");
    }

    public void EnableComment()
    {
        if (commentInput != null)
        {
            commentInput.text = "주석 입력";
            commentInput.gameObject.SetActive(true);
            if (originalParent != null && originalParent.TryGetComponent(out CycleHorizontalLayout layout))
            {
                layout.ReBuildLayout();
            }
        }
    }

    public void ResetSlot()
    {
        countBackground.SetActive(false);
        if (commentInput != null)
        {
            commentInput.text = "";
            commentInput.gameObject.SetActive(false);
            if (originalParent != null && originalParent.TryGetComponent(out CycleHorizontalLayout layout))
            {
                layout.ReBuildLayout();
            }
        }
        ResetAreaStateCascade();
    }

    public void ClearSlot()
    {
        if (onFontColorChanged != null)
        {
            onFontColorChanged.UnregisterListener(SetFontColor);
        }
        if (commentInput != null)
        {
            commentInput.onValueChanged.RemoveListener(OnCommentValueChanged);
        }
        ResetAreaStateCascade();
    }

    private void OnDestroy()
    {
        ClearSlot();
    }

    #endregion

    #region Protected Methods

    protected override void OnSlotPointerDown(PointerEventData eventData)
    {
        _isDragging = false;
        WaitHoldThen(eventData, () =>
        {
            if (eventData.dragging)
            {
                return;
            }

            _isDragging = true;
            originalParent = transform.parent;
            if (originalParent.TryGetComponent(out CycleHorizontalLayout layout))
            {
                layout.RemoveFromHorizontalLayout(this);
            }

            transform.SetParent(GetGhostParent());
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            SetPositionToPointer(eventData);
        });
    }

    protected override void OnSlotPointerUp(PointerEventData eventData)
    {
        if (!_isDragging && !eventData.dragging)
        {
            originalParent = transform.parent; // Set for cleanup context
            originalParent.GetComponent<CycleHorizontalLayout>().RemoveFromHorizontalLayout(this);
            CancelHold();

            ClearSlot(); // Explicitly clear areas and trigger rebuild before destroying

            transform.SetParent(null);
            UIManager.Instance.GetUI<UI_Result>().cyclePanel.CheckRowEmpty();
            Destroy(gameObject);
            return;
        }

        if (_isDragging)
        {
            int finalIndex = _placeholder != null ? _placeholder.transform.GetSiblingIndex() : -1;
            ClearPlaceHolder();

            if (!TryDropOnCycleLayouts(this, finalIndex, eventData))
            {
                CancelHold();
            }
            else
            {
                MarkHoldCanceled();
                _isDragging = false;
                TriggerLayoutRebuild();
            }
        }
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        if (_panelScroll == null)
        {
            _panelScroll = GetComponentInParent<ScrollRect>();
        }
        
        TryBeginPanelScrollDrag(eventData, _isDragging);
    }

    protected override void OnSlotDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            Drag(eventData);
            return;
        }

        ForwardPanelScrollDrag(eventData, _isDragging);
    }

    protected override void OnSlotEndDrag(PointerEventData eventData)
    {
        if (EndPanelScrollDrag(eventData))
        {
            return;
        }

        CancelHold();
    }

    protected override void CancelHold()
    {
        transform.SetParent(originalParent);
        if (_isDragging && originalParent != null && originalParent.TryGetComponent(out CycleHorizontalLayout layout))
        {
            layout.AddSlotJust(this);
            TriggerLayoutRebuild();
        }
        _isDragging = false;
        base.CancelHold();
    }

    #endregion

    #region Private Methods

    private void CheckForPlaceHolder(PointerEventData eventData)
    {
        RaycastAll(eventData);

        CycleHorizontalLayout foundLayout = null;
        foreach (RaycastResult result in RaycastBuffer)
        {
            if (result.gameObject.TryGetComponent(out CycleHorizontalLayout layout))
            {
                foundLayout = layout;
                break;
            }
        }

        if (foundLayout != null)
        {
            UpdatePlaceholder(foundLayout.transform, eventData);
        }
        else
        {
            ClearPlaceHolder();
        }
    }

    private void UpdatePlaceholder(Transform parent, PointerEventData eventData)
    {
        if (_placeholder == null)
        {
            _placeholder = new GameObject("Placeholder");
            _placeholder.transform.SetParent(parent, false);
            LayoutElement layoutElement = _placeholder.AddComponent<LayoutElement>();
            RectTransform rect = GetComponent<RectTransform>();
            layoutElement.preferredWidth = rect.rect.width;
            layoutElement.preferredHeight = rect.rect.height;
        }

        if (_placeholder.transform.parent != parent)
        {
            _placeholder.transform.SetParent(parent, false);
        }

        RectTransform parentRect = parent.GetComponent<RectTransform>();
        if (parentRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos);

        int newIndex = parent.childCount - 1;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == _placeholder.transform)
            {
                continue;
            }

            if (!child.TryGetComponent(out RectTransform childRect))
            {
                continue;
            }

            Vector2 childCenter = parentRect.InverseTransformPoint(childRect.TransformPoint(childRect.rect.center));
            if (localMousePos.x < childCenter.x)
            {
                newIndex = child.GetSiblingIndex();
                if (_placeholder.transform.GetSiblingIndex() < newIndex)
                {
                    newIndex--;
                }

                break;
            }
        }

        if (_placeholder.transform.GetSiblingIndex() != newIndex)
        {
            _placeholder.transform.SetSiblingIndex(newIndex);

            CycleHorizontalLayout rectParent = parent.GetComponent<CycleHorizontalLayout>();
            if (rectParent != null)
            {
                rectParent.ReBuildLayout();
            }
        }
    }

    private void SetFontColor(Color color)
    {
        nameText.color = color;
    }

    private void OnCommentValueChanged(string text)
    {
        if (originalParent != null && originalParent.TryGetComponent(out CycleHorizontalLayout layout))
        {
            layout.ReBuildLayout();
        }
    }

    #endregion

    #region Area Overlay Support

    public void AddAreaStart(string areaId, bool triggerRebuild = true)
    {
        if (!StartAreaIds.Contains(areaId)) StartAreaIds.Add(areaId);
        if (triggerRebuild) TriggerLayoutRebuild();
    }

    public void AddAreaEnd(string areaId, bool triggerRebuild = true)
    {
        if (!EndAreaIds.Contains(areaId)) EndAreaIds.Add(areaId);
        if (triggerRebuild) TriggerLayoutRebuild();
    }

    public void RemoveArea(string areaId, bool triggerRebuild = true)
    {
        StartAreaIds.Remove(areaId);
        EndAreaIds.Remove(areaId);
        if (triggerRebuild) TriggerLayoutRebuild();
    }

    public void ResetAreaStateCascade()
    {
        var idsToReset = new List<string>(StartAreaIds);
        idsToReset.AddRange(EndAreaIds);
        
        StartAreaIds.Clear();
        EndAreaIds.Clear();

        if (originalParent != null && originalParent.parent != null && originalParent.parent.TryGetComponent(out CycleVerticalLayout layout))
        {
            var allSlots = layout.GetComponentsInChildren<CycleSlot>();
            foreach (var slot in allSlots)
            {
                if (slot != this)
                {
                    foreach (var id in idsToReset)
                    {
                        slot.RemoveArea(id);
                    }
                }
            }
        }
        TriggerLayoutRebuild();
    }

    private void TriggerLayoutRebuild()
    {
        if (originalParent != null && originalParent.TryGetComponent(out CycleHorizontalLayout layout))
        {
            layout.ReBuildLayout();
        }
    }

    #endregion
}
