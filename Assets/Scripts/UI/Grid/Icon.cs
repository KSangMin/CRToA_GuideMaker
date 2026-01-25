using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Icon : MonoBehaviour, IDragHandler,IEndDragHandler
{
    private float _width;
    private float _height;
    private int _widthModifier = 1;
    private int _heightModifier = 1;

    private void Awake()
    {
        _width = GetComponent<RectTransform>().sizeDelta.x;
        _height = GetComponent<RectTransform>().sizeDelta.y;
    }

    public void SetIcon(int w, int h, Sprite sprite)
    {
        _widthModifier = w;
        _heightModifier = h;
        GetComponent<Image>().sprite = sprite;
        SetRect(w, h);
    }

    public void SetRect(int w, int h)
    {
        GetComponent<RectTransform>().sizeDelta = new(_width * _widthModifier, _height * _heightModifier);
    }

    public void OnDrag(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        BackgroundSlot targetSlot = null;
        foreach (var result in results)
        {
            // 마우스 아래에 있는 오브젝트 중 BackgroundSlot 컴포넌트 찾기
            targetSlot = result.gameObject.GetComponentInParent<BackgroundSlot>();
            if (targetSlot != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetSlot.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 slotLocalMousePos);

                UIManager.Instance.GetUI<UI_Grid>().SetSnapGuide(
                    targetSlot.GetComponent<RectTransform>()
                    , targetSlot.GetSnapPosition(slotLocalMousePos)
                    , GetComponent<RectTransform>());
                return;
            }
        }

        UIManager.Instance.GetUI<UI_Grid>().HideSnapGuide();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent(out BackgroundSlot targetSlot))
            {
                targetSlot.AddIcon(GetComponent<RectTransform>());
                return;
            }
        }

        // 배경 슬롯 밖으로 드롭했다면? 
        // 기존 부모(Content)로 돌아가거나, 새로운 배경 슬롯을 생성하는 로직을 여기에 넣습니다.
        transform.SetParent(UIManager.Instance.GetUI<UI_Grid>().content);
        transform.localScale = Vector3.one;

        UIManager.Instance.GetUI<UI_Grid>().HideSnapGuide();
    }
}
