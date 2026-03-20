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
            ShowContentPlacementGuide(eventData);
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

    private void ShowContentPlacementGuide(PointerEventData eventData)
    {
        // 1. [1차 계산] 확장이 필요한지 판단하기 위해 현재 마우스 위치 파악
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUI<UI_Grid>().contentRect
            , eventData.position
            , eventData.pressEventCamera
            , out Vector2 tempPos);

        float unit = UIManager.Instance.GetUI<UI_Grid>().content.ItemUnit 
            + UIManager.Instance.GetUI<UI_Grid>().content.Spacing;
        int rawX = Mathf.FloorToInt(tempPos.x / unit);
        int rawY = Mathf.FloorToInt(tempPos.y / -unit);

        // 2. 확장 실행 (여기서 contentRect의 localPosition이 바뀔 수 있음)
        UIManager.Instance.GetUI<UI_Grid>().content.CheckAndExpand(new Vector2Int(rawX, rawY));

        // 3. [재계산] 확장이 완료된 후, 바뀐 좌표계(contentRect) 기준으로 다시 위치 파악
        // 이 과정이 있어야 마우스가 튀지 않고 정확한 인덱스를 다시 잡습니다.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUI<UI_Grid>().contentRect
            , eventData.position
            , eventData.pressEventCamera
            , out Vector2 refinedPos);

        int finalX = Mathf.Max(0, Mathf.FloorToInt(refinedPos.x / unit));
        int finalY = Mathf.Max(0, Mathf.FloorToInt(refinedPos.y / -unit));
        Debug.Log($"finalX: {finalX}, {finalY}");

        // 4. 최종 스냅 좌표 산출
        Vector2 snapPos = UIManager.Instance.GetUI<UI_Grid>().content.GetPosFromIndex(
            new Vector2Int(finalX, finalY));
        Debug.Log($"snapPos: {snapPos}");

        // 5. 가이드 표시
        UIManager.Instance.GetUI<UI_Grid>().SetSnapGuide(
            UIManager.Instance.GetUI<UI_Grid>().contentRect
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

        CreateNewBackgroundSlotAtCurrentPos(eventData);
    }

    private void CreateNewBackgroundSlotAtCurrentPos(PointerEventData eventData)
    {
        BackgroundSlot slot = Util
            .InstantiatePrefabAndGetComponent<BackgroundSlot>(
            path: "UI/BackgroundSlot"
            , parent: UIManager.Instance.GetUI<UI_Grid>().contentRect);

        // 현재 위치의 인덱스 계산
        float unit = UIManager.Instance.GetUI<UI_Grid>().content.ItemUnit
            + UIManager.Instance.GetUI<UI_Grid>().content.Spacing;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Instance.GetUI<UI_Grid>().contentRect
            , eventData.position
            , eventData.pressEventCamera
            , out Vector2 refinedPos);
        int tx = Mathf.Max(0, Mathf.FloorToInt(refinedPos.x / unit));
        int ty = Mathf.Max(0, Mathf.FloorToInt(refinedPos.y / -unit));

        slot.SetSlot(tx, ty, _widthModifier, _heightModifier);
        slot.UpdateVisualPosition();
        slot.AddIcon(_rect, true);
        UIManager.Instance.GetUI<UI_Grid>().content.RegisterSlot(slot);
    }

    public Vector2 GetWH()
    {
        return new(_width, _height);
    }
}
