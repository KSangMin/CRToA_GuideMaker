using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private string _id = "";

    private ScrollRect _panelScroll;
    private bool _isDraggingScroll = false;

    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject chargeBackground;
    [SerializeField] private Image icon;
    [SerializeField] private Image head;
    [SerializeField] private TextMeshProUGUI nameText;
    private ControlType _controlType = ControlType.Normal;
    private CycleSlot _ghostSlot;
    private int _targetIndex = -1;
    private Coroutine _holdCoroutine;
    private float _holdTime = 0.15f;
    private bool _isCanceled = false;

    public void SetSlot(ScrollRect panelScroll, string id, SkillType skillType, ControlType controlType, Sprite skillIcon, Sprite headIcon)
    {
        _panelScroll = panelScroll;
        _id = id;
        icon.sprite = skillIcon;
        head.sprite = headIcon;
        _controlType = controlType;
        if (controlType == ControlType.Charge)
        {
            chargeBackground.SetActive(true);
            nameText.SetText(string.Format("{0}: 차징", GetAttackText(skillType)));
        }
        else
        {
            chargeBackground.SetActive(false);
            nameText.SetText(GetAttackText(skillType));
        }
    }

    private string GetAttackText(SkillType skillType)
    {
        string result = "";

        switch (skillType)
        {
            case SkillType.Basic:
                result = "기본 공격";
                break;
            case SkillType.SpecialSkill:
                result = "특수 스킬";
                break;
            case SkillType.Ultimate:
                result = "궁극기";
                break;
            case SkillType.Dash:
                result = "대시";
                break;
            default:
                break;
        }

        return result;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isCanceled = false;
        _holdCoroutine = StartCoroutine(CreateGhostAfterDelay(eventData));
    }

    private IEnumerator CreateGhostAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(_holdTime);

        if (!_isCanceled && !eventData.dragging)
        {
            _ghostSlot = CreateSlot(eventData);
        }

        _holdCoroutine = null;
    }

    private CycleSlot CreateSlot(PointerEventData eventData)
    {
        Transform ghostParent = UIManager.Instance.GetUI<UI_Panel>().forGhostParent;
        CycleSlot slot = Instantiate(ghostPrefab, ghostParent)
            .GetComponent<CycleSlot>();
        slot.name = $"Slot_{head.sprite.name}_{nameText.text}";
        slot.SetSlot(icon.sprite, _controlType, head.sprite, nameText.text);
        slot.SetPositionToPointer(eventData);

        return slot;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (_ghostSlot != null)
        {
            if (!eventData.dragging)
            {
                Destroy(_ghostSlot.gameObject);
                _ghostSlot = null;
            }
        }
        else if(!_isDraggingScroll)//클릭
        {
            UIManager.Instance.GetUI<UI_Result>().cyclePanel.AddSlotToLast(CreateSlot(eventData));
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_ghostSlot == null)
        {
            _isDraggingScroll = true;
            CancelHold();
            _panelScroll.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghostSlot != null)
        {
            _ghostSlot.Drag(eventData);
            return;
        }

        if (_isDraggingScroll)
        {
            _panelScroll.OnDrag(eventData);
        }
    }

    private void CancelHold()
    {
        _isCanceled = true;
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDraggingScroll)
        {
            _panelScroll.OnEndDrag(eventData);
            _isDraggingScroll = false;
            return;
        }

        if (_ghostSlot != null)
        {
            _targetIndex = _ghostSlot.GetPlaceHolderIndex();
            _ghostSlot.ClearPlaceHolder();

            if (!ProcessDrop(eventData))
            {
                Destroy(_ghostSlot.gameObject);
            }
            _ghostSlot = null;
        }
        CancelHold();
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("CycleHorizontalLayout"))
            {
                CycleHorizontalLayout hz = result.gameObject.GetComponent<CycleHorizontalLayout>();
                hz.AddSlotAndCheckExplode(_ghostSlot, _targetIndex);
                return true;
            }
            else if (result.gameObject.CompareTag("CyclePanel"))
            {
                CyclePanel cyclePanel = result.gameObject.GetComponent<CyclePanel>();
                cyclePanel.AddSlotToNewRow(_ghostSlot);
                return true;
            }
        }

        return false;
    }
}

