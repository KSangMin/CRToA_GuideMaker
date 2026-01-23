using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rect;
    [SerializeField] private float _holdTime = 0.25f; // n초 설정
    [SerializeField] private bool _isLongPress = false;
    private Coroutine timerCoroutine;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    // [클릭 판정용]
    public void OnPointerDown(PointerEventData eventData)
    {
        _isLongPress = false;
        timerCoroutine = StartCoroutine(CheckLongPress(eventData));
    }

    IEnumerator CheckLongPress(PointerEventData eventData)
    {
        yield return new WaitForSeconds(_holdTime);

        // n초가 지났다면 드래그 모드 활성화
        _isLongPress = true;
        Debug.Log("드래그 준비 완료!");

        // 시각적 피드백
        transform.localScale = Vector3.one * 0.9f;
        transform.SetParent(UIManager.Instance.GetUI<UI_Grid>()._forDragParent);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        transform.localScale = Vector3.one;

        if (!_isLongPress && !eventData.dragging)
        {
            // 드래그가 되지 않았고 롱 프레스도 아니면 '클릭'으로 판정
            Debug.Log("단순 클릭: 크기 조절 UI 오픈");
            // 크기 조절 UI 호출해주면 됨
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 롱 프레스 상태가 아닐 때 드래그가 시작되려 하면 이벤트를 취소시킴
        if (!_isLongPress)
        {
            eventData.pointerDrag = null; // 이 줄이 핵심입니다. 드래그 권한을 뺏음
            return;
        }

        // 여기서부터는 기존 드래그 로직 (부모 변경 등)
        Debug.Log("슬롯 드래그 시작");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isLongPress) return;

        // 드래그 로직 (마우스 따라 이동)
        transform.position = eventData.position + new Vector2(-_rect.sizeDelta.x / 2f, _rect.sizeDelta.y / 2f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isLongPress = false;
        transform.localScale = Vector3.one;
        transform.SetParent(UIManager.Instance.GetUI<UI_Grid>().content);
        // 여기서 자석 스냅(3~4단계) 로직 호출
    }
}