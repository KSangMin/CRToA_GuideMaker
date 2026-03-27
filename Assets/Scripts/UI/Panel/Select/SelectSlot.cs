using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    private string _id = "";

    private ScrollRect _parentScroll;

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

    public void SetSlot(ScrollRect scroll, string id, SkillType skillType, ControlType controlType, Sprite sprite)
    {
        _parentScroll = scroll;
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

        if (!_isCanceled)
        {
            _ghost = Instantiate(gameObject, UIManager.Instance.GetUI<UI_Panel>().forGhostParent);

            _ghost.transform.position = eventData.position + new Vector2(-_halfSlotSize, _halfSlotSize);
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

    public void OnDrag(PointerEventData eventData)
    {
        if (_ghost == null)
        {
            float distance = Vector2.Distance(_startPosition, eventData.position);
            if (!_isCanceled && distance > EventSystem.current.pixelDragThreshold)
            {
                CancelHold();

                if (_parentScroll != null)
                {
                    _parentScroll.OnInitializePotentialDrag(eventData);
                    _parentScroll.OnBeginDrag(eventData);
                }
            }
            if (_isCanceled && _parentScroll != null)
            {
                _parentScroll.OnDrag(eventData);
            }
            return;
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
        CancelHold();

        if (_ghost == null)
        {
            if (_parentScroll != null)
            {
                _parentScroll.OnEndDrag(eventData);
            }
            return;
        }

        if (!ProcessDrop(eventData))
        {
            Destroy(_ghost);
        }
        _ghost = null;
    }

    private bool ProcessDrop(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("CyclePanel"))
            {


                return true;
            }
        }

        return false;
    }
}

