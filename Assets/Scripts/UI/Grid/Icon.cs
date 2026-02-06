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

    private RectTransform _rect;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        _width = _rect.sizeDelta.x;
        _height = _rect.sizeDelta.y;
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
        _rect.sizeDelta = new(_width * _widthModifier, _height * _heightModifier);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 2. 레이캐스트로 마우스 아래 대상 확인
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        BackgroundSlot targetSlot = null;
        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent(out targetSlot)) break;
        }

        // 3. 상태에 따른 가이드 표시
        if (targetSlot != null)
        {
            // [CASE A] 기존 슬롯 내부로 들어간 경우
            ShowSnapGuide(eventData, targetSlot);
        }
        else
        {
            // [CASE B] 빈 공간(Content)에 있는 경우
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                UIManager.Instance.GetUI<UI_Grid>().content
                , eventData.position
                , eventData.pressEventCamera
                , out Vector2 localPos);

            ShowContentPlacementGuide(localPos);
        }
    }

    private void ShowSnapGuide(PointerEventData eventData, BackgroundSlot targetSlot)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetSlot.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 slotLocalMousePos);

        UIManager.Instance.GetUI<UI_Grid>().SetSnapGuide(
            targetSlot.GetComponent<RectTransform>()
            , targetSlot.GetSnapPosition(slotLocalMousePos)
            , _rect);
    }

    private void ShowContentPlacementGuide(Vector2 localPos)
    {
        // Spacing을 포함한 그리드 단위로 인덱스 계산
        float unit = UIManager.Instance.GetUI<UI_Grid>().GridUnit
            + UIManager.Instance.GetUI<UI_Grid>().Spacing;
        // 현재 인덱스 계산
        int rawX = Mathf.RoundToInt(localPos.x / unit);
        int rawY = Mathf.RoundToInt(localPos.y / -unit);

        // 0보다 작으면 무조건 -1을 보냄 (GridManager에서 1칸 확장의 신호로 씀)
        int sendX = rawX < 0 ? -1 : rawX;
        int sendY = rawY < 0 ? -1 : rawY;

        if (sendX < 0 || sendY < 0)
        {
            UIManager.Instance.GetUI<UI_Grid>().CheckAndExpand(new Vector2Int(sendX, sendY));

            // 확장 중일 때는 가이드 위치를 0,0에 고정 (시각적 튐 방지)
            rawX = 0;
            rawY = 0;
        }

        // 빈 공간용 가이드 표시 (Content 기준 스냅 위치)
        Vector2 snapPos = UIManager.Instance.GetUI<UI_Grid>().GetPosFromIndex(
            new Vector2Int(Mathf.Max(0, rawX), Mathf.Max(0, rawY)));
        UIManager.Instance.GetUI<UI_Grid>().SetSnapGuide(
            UIManager.Instance.GetUI<UI_Grid>().content
            , snapPos
            , _rect);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UIManager.Instance.GetUI<UI_Grid>().HideSnapGuide();

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.TryGetComponent(out BackgroundSlot targetSlot))
            {
                targetSlot.AddIcon(_rect);
                
                return;
            }
        }

        CreateNewBackgroundSlotAtCurrentPos();
    }

    private void CreateNewBackgroundSlotAtCurrentPos()
    {
        // 현재 위치의 인덱스 계산
        float unit = UIManager.Instance.GetUI<UI_Grid>().GridUnit 
            + UIManager.Instance.GetUI<UI_Grid>().Spacing;
        int tx = Mathf.Max(0, Mathf.RoundToInt(_rect.localPosition.x / unit));
        int ty = Mathf.Max(0, Mathf.RoundToInt(_rect.localPosition.y / -unit));

        // TODO: 새로운 BackgroundSlot 프리팹을 생성하고 GridManager에 등록
        BackgroundSlot slot = Util
            .InstantiatePrefabAndGetComponent<BackgroundSlot>(
            path: "UI/BackgroundSlot"
            , parent: UIManager.Instance.GetUI<UI_Grid>().content);
        slot.SetSlot(tx, ty, _widthModifier, _heightModifier);
        slot.UpdateVisualPosition();
        slot.AddIcon(_rect);
        UIManager.Instance.GetUI<UI_Grid>().RegisterSlot(slot);
    }
}
