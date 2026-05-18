using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CycleSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject chargeBackground;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject countBackground;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image head;
    [SerializeField] private TextMeshProUGUI nameText;
    [HideInInspector] public Transform originalParent;
    private Coroutine _holdCoroutine;
    private float _holdTime = 0.15f;
    private bool _isCanceled = false;
    private bool _isDragging = false;

    private ScrollRect _parentScroll;
    private bool _isScrolling;

    private GameObject _placeholder = null;
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
        if(controlType == ControlType.Charge)
        {
            chargeBackground.SetActive(true);
        }
        else if(controlType == ControlType.Normal)
        {
            chargeBackground.SetActive(false);
        }
        head.sprite = headIcon;
        nameText.SetText(text);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _isDragging = false;
        _holdCoroutine = StartCoroutine(CheckHoldAfterDelay(eventData));
    }

    private IEnumerator CheckHoldAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(_holdTime);

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

        _holdCoroutine = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            return;
        }

        CancelHold();

        _parentScroll = GetComponentInParent<ScrollRect>();
        if (_parentScroll != null)
        {
            _isScrolling = true;
            _parentScroll.OnBeginDrag(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging && !eventData.dragging)//클릭 시
        {
            originalParent.GetComponent<CycleHorizontalLayout>().RemoveFromHorizontalLayout(this);
            CancelHold();

            transform.SetParent(null);
            UIManager.Instance.GetUI<UI_Result>().cyclePanel.CheckRowEmpty();
            Destroy(gameObject);
            return;
        }

        if (_isDragging)
        {
            int finalIndex = _placeholder != null ? _placeholder.transform.GetSiblingIndex() : -1;
            ClearPlaceHolder();

            if (!ProcessDrop(eventData, finalIndex))
            {
                CancelHold();
            }
            else
            {
                _isCanceled = true;
                _isDragging = false;
            }
        }
    }

    //SelectSlot에서 넘겨받는 메서드
    public void Drag(PointerEventData eventData)
    {
        SetPositionToPointer(eventData);

        CheckForPlaceHolder(eventData);
    }

    public void SetPositionToPointer(PointerEventData eventData)
    {
        RectTransform parentRect = transform.parent.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);

        GetComponent<RectTransform>().anchoredPosition = localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDragging)
        {
            Drag(eventData);
            return;
        }

        if (_isScrolling && _parentScroll != null)
        {
            _parentScroll.OnDrag(eventData);
        }
    }

    private void CheckForPlaceHolder(PointerEventData eventData)
    {
        // 드래그 중인 위치 아래에 LayoutGroup이 있는지 체크
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        CycleHorizontalLayout foundLayout = null;
        foreach (var result in results)
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
            // 1. Placeholder 초기 생성
            _placeholder = new GameObject("Placeholder");
            _placeholder.transform.SetParent(parent, false);
            var le = _placeholder.AddComponent<LayoutElement>();
            var rect = GetComponent<RectTransform>();
            le.preferredWidth = rect.rect.width;
            le.preferredHeight = rect.rect.height;
        }

        if (_placeholder.transform.parent != parent)
        {
            _placeholder.transform.SetParent(parent, false);
        }

        // 2. 마우스 위치에 따른 순서(SiblingIndex) 결정
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

        // 4. 인덱스가 변했을 때만 실행하여 불필요한 연산 방지
        if (_placeholder.transform.GetSiblingIndex() != newIndex)
        {
            _placeholder.transform.SetSiblingIndex(newIndex);

            // [중요] ContentSizeFitter와 LayoutGroup을 즉시 재계산하게 강제함
            // 이 코드가 없으면 한 박자 늦게 반응하는 현상이 발생합니다.
            var rectParent = parent.GetComponent<CycleHorizontalLayout>();
            if (rectParent != null)
            {
                rectParent.ReBuildLayout();
            }
        }
    }

    //SelectSlot에서 사용하는 메서드
    public int GetPlaceHolderIndex()
    {
        return _placeholder != null ? _placeholder.transform.GetSiblingIndex() : -1;
    }

    private void CancelHold()
    {
        transform.SetParent(originalParent);
        _isCanceled = true;
        _isDragging = false;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isScrolling && _parentScroll != null)
        {
            _parentScroll.OnEndDrag(eventData);
            _isScrolling = false;
            return;
        }

        CancelHold();
    }

    private bool ProcessDrop(PointerEventData eventData, int targetIndex)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("CycleHorizontalLayout"))
            {
                CycleHorizontalLayout hz = result.gameObject.GetComponent<CycleHorizontalLayout>();
                hz.AddSlotAndCheckExplode(this, targetIndex);
                
                return true;
            }
            else if (result.gameObject.CompareTag("CyclePanel"))
            {
                CyclePanel cyclePanel = result.gameObject.GetComponent<CyclePanel>();
                cyclePanel.AddSlotToNewRow(this);

                return true;
            }
        }

        return false;
    }

    public void ClearPlaceHolder()
    {
        if (_placeholder != null)
        {
            //Destroy가 한 프레임에서 즉각 실행되지 않기 때문에 Row에 남아 있어서 다른 부분에서 문제가 생기기 때문에 추가
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
