using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CycleHorizontalLayout : MonoBehaviour
{
    private int _id = -1;
    private int _maxSlotCount = 8;
    private CycleVerticalLayout _cycleVerticalLayout;
    private List<CycleSlot> _slots = new();

    [Header("색깔 변경")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private ColorEventChannel onBackgroundColorChanged;

    [Header("오버레이 패딩 설정")]
    [SerializeField] private float levelOffset = 30f;

    private void Awake()
    {
        onBackgroundColorChanged.RegisterListener(SetBackgroundColor);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void Init(int id, CycleVerticalLayout vert)
    {
        SetId(id);
        _cycleVerticalLayout = vert;
        _maxSlotCount = UIManager.Instance.GetUI<UI_Result>().optionPanel.rowLengthPanel.CurValue;
        SetBackgroundColor(UIManager.Instance.GetUI<UI_Result>().optionPanel.colorSelectPanel.CurBackgroundColor);
    }

    public void SetId(int id)
    {
        _id = id;
    }

    private void AddSlot(CycleSlot slot, int index = -1)
    {
        _slots.Add(slot);
        slot.transform.SetParent(transform);

        if (index != -1)
        {
            slot.transform.SetSiblingIndex(index);
        }

        slot.originalParent = slot.transform.parent;

        _cycleVerticalLayout.CheckRowEmpty();
    }

    public void AddSlotJust(CycleSlot slot, int index = -1)
    {
        if (_slots.Count >= _maxSlotCount)
        {
            CycleHorizontalLayout row = _cycleVerticalLayout.CreateRow();
            row.AddSlotJust(slot);
            return;
        }

        AddSlot(slot, index);
    }

    public void AddSlotAndCheckExplode(CycleSlot slot, int index = -1)
    {
        AddSlot(slot, index);

        if (_slots.Count > _maxSlotCount)
        {
            MoveLastSlotToNextRow();
        }
    }

    private void MoveLastSlotToNextRow()
    {
        CycleHorizontalLayout row = _cycleVerticalLayout.GetRow(_id + 1);
        CycleSlot lastSlot = transform.GetChild(transform.childCount - 1).GetComponent<CycleSlot>();
        RemoveFromHorizontalLayout(lastSlot);
        row.AddSlotAndCheckExplode(lastSlot, 0);
    }

    public void RemoveFromHorizontalLayout(CycleSlot slot)
    {
        _slots.Remove(slot);
    }

    public void ReBuildLayout()
    {
        _cycleVerticalLayout.RebuildLayout();
    }

    public void UpdateMaxSlotCount(int value)
    {
        _maxSlotCount = value;
    }

    public void ProcessOverflow()
    {
        if(_slots.Count > _maxSlotCount)
        {
            int repeatCount = _slots.Count - _maxSlotCount;
            for(int i = 0;  i < repeatCount; i++)
            {
                MoveLastSlotToNextRow();
            }
        }
    }

    public void ResetCycle()
    {
        foreach(Transform child in transform)
        {
            if(child.TryGetComponent(out CycleSlot slot))
            {
                slot.ClearSlot();
                Destroy(slot.gameObject);
            }            
        }

        _slots.Clear();
    }

    private void SetBackgroundColor(Color color)
    {
        backgroundImage.color = color;
    }

    private void OnDestroy()
    {
        onBackgroundColorChanged.UnregisterListener(SetBackgroundColor);
    }

    public bool SetDynamicPadding(int maxOverlapLevel, bool hasNameField)
    {
        if (TryGetComponent(out HorizontalLayoutGroup hlg))
        {
            float basePadding = hasNameField ? 35f : 10f;
            float requiredSpace = maxOverlapLevel < 0 ? 0f : (maxOverlapLevel * levelOffset) + basePadding; 
            
            if (Mathf.Abs(hlg.padding.top - requiredSpace) > 1f)
            {
                hlg.padding = new RectOffset(hlg.padding.left, hlg.padding.right, Mathf.RoundToInt(requiredSpace), hlg.padding.bottom);
                return true;
            }
        }
        return false;
    }
}
