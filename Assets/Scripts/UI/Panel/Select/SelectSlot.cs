using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private string _id = "";

    private ScrollRect _menuScroll;
    private ScrollRect _panelScroll;
    private ScrollRect _targetScroll;
    private bool _isDraggingScroll = false;

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    private GameObject _ghost;
    private float _halfSlotSize;
    private Vector2 _startPosition;
    private Coroutine _holdCoroutine;
    private float holdTime = 0.2f;
    private bool _isCanceled = false;

    private void Awake()
    {
        _halfSlotSize = GetComponent<RectTransform>().sizeDelta.x / 2f;
    }

    public void SetSlot(ScrollRect menuScroll, ScrollRect panelScroll, string id, SkillType skillType, ControlType controlType, Sprite sprite)
    {
        _menuScroll = menuScroll;
        _panelScroll = panelScroll;
        _id = id;
        _icon.sprite = sprite;

        if (controlType == ControlType.Charge)
        {
            _nameText.SetText(string.Format("{0}: 차징", GetAttackText(skillType)));
        }
        else
        {
            _nameText.SetText(GetAttackText(skillType));
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
        _startPosition = eventData.position;
        _holdCoroutine = StartCoroutine(CreateGhostAfterDelay(eventData));
    }

    private IEnumerator CreateGhostAfterDelay(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdTime);

        if (!_isCanceled && !eventData.dragging)
        {
            _ghost = Instantiate(gameObject, UIManager.Instance.GetUI<UI_Panel>().forGhostParent);
            _ghost.transform.position = eventData.position;
        }

        _holdCoroutine = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (_ghost != null)
        {
            if (!eventData.dragging)
            {
                Destroy(_ghost);
                _ghost = null;
            }
        }
        else
        {
            //클릭
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_ghost == null)
        {
            _isDraggingScroll = true;
            CancelHold();
            _panelScroll.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost != null)
        {
            _ghost.transform.position = eventData.position;
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

        if (_ghost != null)
        {
            if (!ProcessDrop(eventData))
            {
                Destroy(_ghost);
            }
            _ghost = null;
        }
        CancelHold();
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("CyclePanel"))
            {
                //패널에 부착하는 코드 필요
                return true;
            }
        }

        return false;
    }
}

