using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CycleSlot : DraggableSlot
{
    [SerializeField] private GameObject chargeBackground;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject countBackground;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image head;
    [SerializeField] private TextMeshProUGUI nameText;
    [HideInInspector] public Transform originalParent;

    private bool _isCanceled;
    private bool _isDragging;

    private bool _isScrolling;

    private GameObject _placeholder;
    private ControlType _controlType = ControlType.Normal;

    [Header("색깔 변경")]
    [SerializeField] private ColorEventChannel onFontColorChanged;

    private void Awake()
    {
        onFontColorChanged.RegisterListener(SetFontColor);
    }

    private void Start()
    {
        countBackground.SetActive(false);
        SetFontColor(UIManager.Instance.GetUI<UI_Result>().optionPanel.colorSelectPanel.CurFontColor);
    }

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

    protected override void OnSlotPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _isDragging = false;
        holdCoroutine = StartCoroutine(CheckHoldAfterDelay(eventData));
    }

    private IEnumerator CheckHoldAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdDelaySeconds);

        if (!_isCanceled && !eventData.dragging)
        {
            _isDragging = true;
            originalParent = transform.parent;
            if (originalParent.TryGetComponent(out CycleHorizontalLayout layout))
            {
                layout.RemoveFromHorizontalLayout(this);
            }

            transform.SetParent(UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            SetPositionToPointer(eventData);
        }

        holdCoroutine = null;
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            return;
        }

        CancelHold();
        TryForwardScrollOrDraggable(eventData);
    }

    protected override bool ShouldForwardScrollDrag(PointerEventData eventData) => true;

    protected override ScrollRect ResolveScrollRectForDrag(PointerEventData eventData) =>
        GetComponentInParent<ScrollRect>();

    protected override void OnSlotPointerUp(PointerEventData eventData)
    {
        ResolveClickOrDragDropPointerUp(eventData);
    }

    protected override bool IsDragDropActive()
    {
        return _isDragging;
    }

    protected override void OnSlotClick(PointerEventData eventData)
    {
        originalParent.GetComponent<CycleHorizontalLayout>().RemoveFromHorizontalLayout(this);
        CancelHold();

        transform.SetParent(null);
        UIManager.Instance.GetUI<UI_Result>().cyclePanel.CheckRowEmpty();
        Destroy(gameObject);
    }

    protected override void OnSlotDragDrop(PointerEventData eventData)
    {
        int finalIndex = _placeholder != null ? _placeholder.transform.GetSiblingIndex() : -1;
        ClearPlaceHolder();

        if (!TryDropOnCycleLayouts(this, finalIndex, eventData))
        {
            CancelHold();
        }
        else
        {
            _isCanceled = true;
            _isDragging = false;
        }
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

    protected override void OnSlotDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            Drag(eventData);
            return;
        }

        base.OnSlotDrag(eventData);
    }

    private void CheckForPlaceHolder(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        CycleHorizontalLayout foundLayout = null;
        foreach (RaycastResult result in results)
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
            LayoutElement le = _placeholder.AddComponent<LayoutElement>();
            RectTransform rect = GetComponent<RectTransform>();
            le.preferredWidth = rect.rect.width;
            le.preferredHeight = rect.rect.height;
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

    public int GetPlaceHolderIndex()
    {
        return _placeholder != null ? _placeholder.transform.GetSiblingIndex() : -1;
    }

    private void CancelHold()
    {
        transform.SetParent(originalParent);
        _isCanceled = true;
        _isDragging = false;
        StopHoldCoroutineSilently();
    }

    protected override void OnDraggableEndDrag(PointerEventData eventData)
    {
        CancelHold();
    }

    public void ClearPlaceHolder()
    {
        if (_placeholder != null)
        {
            _placeholder.transform.SetParent(UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
            Destroy(_placeholder);
            _placeholder = null;
        }
    }

    public void SetSlotCount(int count)
    {
        countBackground.SetActive(true);
        countText.SetText($"{count}");
    }

    public void ResetSlot()
    {
        countBackground.SetActive(false);
    }

    private void SetFontColor(Color color)
    {
        nameText.color = color;
    }

    public void ClearSlot()
    {
        onFontColorChanged.UnregisterListener(SetFontColor);
    }
}
