using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CycleSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [HideInInspector] public Transform originalParent;
    private Coroutine _holdCoroutine;
    private float holdTime = 0.2f;
    private bool _isCanceled = false;

    private GameObject _placeholder = null;

    public void SetSlot(Sprite sprite, string text)
    {
        _icon.sprite = sprite;
        _nameText.SetText(text);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _holdCoroutine = StartCoroutine(CheckHoldAfterDelay(eventData));
    }

    private IEnumerator CheckHoldAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdTime);

        if (!_isCanceled && !eventData.dragging)
        {
            transform.position = eventData.position;
            originalParent = transform.parent;
            originalParent.GetComponent<CycleHorizontalLayout>().RemoveFromSlot(this);
            transform.SetParent(UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
        }

        _holdCoroutine = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!eventData.dragging)
        {
            transform.SetParent(null);
            UIManager.Instance.GetUI<UI_Result>().cyclePanel.CheckRowEmpty();
            Destroy(gameObject);
            return;
        }

        CancelHold();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        CheckForPlaceHolder(eventData);
    }

    private void CheckForPlaceHolder(PointerEventData eventData)
    {
        // 드래그 중인 위치 아래에 LayoutGroup이 있는지 체크
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var layout = result.gameObject.GetComponent<CycleHorizontalLayout>();
            if (layout != null)
            {
                UpdatePlaceholder(layout.transform, eventData.position);
                return;
            }
        }

        Destroy(_placeholder);
        _placeholder = null;
    }

    private void UpdatePlaceholder(Transform parent, Vector2 mousePos)
    {
        if (_placeholder == null)
        {
            // 1. Placeholder 초기 생성
            _placeholder = new GameObject("Placeholder");
            _placeholder.transform.SetParent(parent);
            var le = _placeholder.AddComponent<LayoutElement>();
            var rect = GetComponent<RectTransform>();
            le.preferredWidth = rect.rect.width;
            le.preferredHeight = rect.rect.height;
        }

        if (_placeholder.transform.parent != parent)
        {
            _placeholder.transform.SetParent(parent);
        }

        // 2. 마우스 위치에 따른 순서(SiblingIndex) 결정
        int newIndex = parent.childCount;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (mousePos.x < parent.GetChild(i).position.x)
            {
                newIndex = i;
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

    private void CancelHold()
    {
        transform.SetParent(originalParent);
        _isCanceled = true;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int finalIndex = -1;
        if (_placeholder != null)
        {
            // 드롭 시 Placeholder의 위치로 슬롯 이동
            finalIndex = _placeholder.transform.GetSiblingIndex();
            Destroy(_placeholder);
            _placeholder = null;
        }
        if (!ProcessDrop(eventData, finalIndex))
        {
            CancelHold();
        }
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
                hz.AddSlot(this, targetIndex);
                return true;
            }
            else if (result.gameObject.CompareTag("CyclePanel"))
            {
                CyclePanel cyclePanel = result.gameObject.GetComponent<CyclePanel>();
                cyclePanel.AddSlotToLast(this);
                return true;
            }
        }

        return false;
    }
}