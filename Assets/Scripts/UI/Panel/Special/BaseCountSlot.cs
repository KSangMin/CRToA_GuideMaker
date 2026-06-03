using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseCountSlot : SelfGhostSlot
{
    #region Serialized Fields

    [SerializeField] protected TextMeshProUGUI countText;

    #endregion

    #region Protected Fields
    [SerializeField] protected int _minCount = 2;
    [SerializeField] protected int _maxCount = 9;

    protected int _curCount;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        _curCount = _minCount;

        if (countText == null)
        {
            Debug.LogError($"[{GetType().Name}] countText is not assigned on {name}.", this);
        }
    }

    protected virtual void Start()
    {
        SetCountText();
    }

    #endregion

    #region Protected Methods

    protected override void OnSelfGhostClick(PointerEventData eventData)
    {
        UpCount();
    }

    protected override bool ProcessDrop(PointerEventData eventData)
    {
        return TryRaycastCycleSlot(eventData, cycleSlot => ApplyToCycleSlot(cycleSlot, _curCount, _maxCount));
    }

    protected abstract void ApplyToCycleSlot(CycleSlot cycleSlot, int count, int maxCount);

    protected virtual void SetCountText()
    {
        if (countText != null)
        {
            countText.SetText($"{_curCount}");
        }
    }

    protected virtual void UpCount()
    {
        _curCount++;

        if (_curCount > _maxCount)
        {
            _curCount = _minCount;
        }

        SetCountText();
    }

    #endregion
}
