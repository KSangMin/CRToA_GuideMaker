using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CycleVerticalLayout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject rowPanelPrefab;
    [SerializeField] private IntEventChannel onRowLengthChangedEvent;
    public EventChannel onLayoutRebuiltEvent;

    private List<CycleHorizontalLayout> _rows = new();
    private Transform _verticalLayout;

    private void Awake()
    {
        _verticalLayout = GetComponent<Transform>();
        if (onRowLengthChangedEvent != null)
        {
            onRowLengthChangedEvent.RegisterListener(OnRowLengthChanged);
        }
    }

    private void OnDestroy()
    {
        if (onRowLengthChangedEvent != null)
        {
            onRowLengthChangedEvent.UnregisterListener(OnRowLengthChanged);
        }
    }

    private void OnRowLengthChanged(int value)
    {
        foreach (var row in _rows)
        {
            row.UpdateMaxSlotCount(value);
        }
        
        for (int i = 0; i < _rows.Count; i++)
        {
            _rows[i].ProcessOverflow();
        }

        RebuildLayout();
    }

    public void AddSlotToLast(CycleSlot slot)
    {
        CycleHorizontalLayout row = _rows.Count <= 0 ? CreateRow() : _rows.Last();
        row.AddSlotJust(slot);
        RebuildLayout();
    }

    public void AddSlotToNewRow(CycleSlot slot)
    {
        CycleHorizontalLayout row = CreateRow();
        row.AddSlotJust(slot);
        RebuildLayout();
    }

    public CycleHorizontalLayout GetRow(int id = -1)
    {
        if (_rows.Count <= 0 || id >= _rows.Count)
        {
            return CreateRow();
        }

        if (id == -1)
        {
            return _rows[_rows.Count - 1];
        }

        return _rows[id];
    }

    public CycleHorizontalLayout CreateRow(int id = -1)
    {
        if (id == -1)
        {
            id = _rows.Count;
        }
        CycleHorizontalLayout row = Instantiate(rowPanelPrefab, _verticalLayout).GetComponent<CycleHorizontalLayout>();
        row.name = $"Row_{id}";
        row.Init(id, this);
        InsertRow(id, row);

        return row;
    }

    public void InsertRow(int id, CycleHorizontalLayout row)
    {
        _rows.Insert(id, row);
        for (int i = 0; i < _rows.Count; i++)
        {
            _rows[i].SetId(i);
        }
    }

    public void CheckRowEmpty()
    {
        for (int i = _rows.Count - 1; i >= 0; i--)
        {
            CycleHorizontalLayout row = _rows[i];
            if (row.transform.childCount <= 0)
            {
                _rows.RemoveAt(i);
                row.transform.SetParent(null);
                Destroy(row.gameObject);
            }
        }
    }

    public void RebuildLayout()
    {
        // 새로 생성된 슬롯들의 내부 컴포넌트(TMP_Text 등)가 우선 자신의 크기를 정확히 계산하도록 강제 업데이트
        Canvas.ForceUpdateCanvases();

        // 중첩 레이아웃의 타이밍 이슈 해결을 위해 자식 레이아웃부터 바텀업으로 즉시 리빌드
        foreach (var row in _rows)
        {
            if (row != null && row.gameObject.activeInHierarchy)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(row.GetComponent<RectTransform>());
            }
        }
        
        // 이후 자기 자신 레이아웃 리빌드
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        // 부모(Content) 레이아웃 리빌드 (2연속 강제 리빌드로 수축 지연 해결)
        if (transform.parent != null)
        {
            var parentRect = transform.parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
        }

        // 최종적으로 정렬된 레이아웃의 위치(Transform)를 글로벌하게 갱신
        Canvas.ForceUpdateCanvases();

        // 이제 모든 좌표가 100% 정확하므로 영역(Area) 렌더링 이벤트 호출
        if (onLayoutRebuiltEvent != null)
        {
            onLayoutRebuiltEvent.RaiseEvent();
        }
    }

    public void ResetCycle()
    {
        foreach(Transform child in transform)
        {
            if(child.TryGetComponent(out CycleHorizontalLayout row))
            {
                row.ResetCycle();
                Destroy(row.gameObject);
            }
        }

        _rows.Clear();
    }
}
