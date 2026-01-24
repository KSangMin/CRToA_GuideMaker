using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PressHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rect;
    [SerializeField] private float _holdTime = 0.25f; // n초 설정
    [HideInInspector] public bool isLongPress = false;
    private Coroutine timerCoroutine;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    // [클릭 판정용]
    public void OnPointerDown(PointerEventData eventData)
    {
        isLongPress = false;
        timerCoroutine = StartCoroutine(CheckLongPress(eventData));
    }

    IEnumerator CheckLongPress(PointerEventData eventData)
    {
        yield return new WaitForSeconds(_holdTime);

        // n초가 지났다면 드래그 모드 활성화
        isLongPress = true;
        Debug.Log("드래그 준비 완료!");

        // 시각적 피드백
        transform.localScale = Vector3.one * 0.9f;
        transform.SetParent(UIManager.Instance.GetUI<UI_Grid>()._forDragParent);
        MoveToMousePosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        if (!eventData.dragging)
        {
            transform.localScale = Vector3.one;
            transform.SetParent(UIManager.Instance.GetUI<UI_Grid>().content);
            if (!isLongPress)
            {
                // 드래그가 되지 않았고 롱 프레스도 아니면 '클릭'으로 판정
                Debug.Log("단순 클릭: 크기 조절 UI 오픈");
                // 크기 조절 UI 호출해주면 됨
                if (UIManager.Instance.GetUI<UI_Grid>().isHandleVisible)
                {
                    UIManager.Instance.GetUI<UI_Grid>().HideHandles();
                }
                else
                {
                    UIManager.Instance.GetUI<UI_Grid>().OpenResizeUI(GetComponent<RectTransform>());
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 롱 프레스 상태가 아닐 때 드래그가 시작되려 하면 이벤트를 취소시킴
        if (!isLongPress)
        {
            eventData.pointerDrag = null; // 이 줄이 핵심입니다. 드래그 권한을 뺏음
            return;
        }

        // 여기서부터는 기존 드래그 로직 (부모 변경 등)
        Debug.Log("슬롯 드래그 시작");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isLongPress) return;

        // 드래그 로직 (마우스 따라 이동)
        MoveToMousePosition(eventData);
    }

    private void MoveToMousePosition(PointerEventData eventData)
    {
        Vector2 offset = new Vector2(-_rect.sizeDelta.x * 0.5f, _rect.sizeDelta.y * 0.5f);
        offset *= UIManager.Instance.GetUI<UI_Grid>().content.localScale.x;
        transform.position = eventData.position + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isLongPress = false;
        transform.SetParent(UIManager.Instance.GetUI<UI_Grid>().content);
        transform.localScale = Vector3.one;
        UIManager.Instance.GetUI<UI_Grid>().HideHandles();
        // 여기서 자석 스냅(3~4단계) 로직 호출
    }
}