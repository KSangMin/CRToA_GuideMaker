using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CountSlot : SelfGhostSlot
{
    #region Serialized Fields

    [SerializeField] private TextMeshProUGUI countText;

    #endregion

    #region Private Fields

    private int _minCount = 2;
    private int _curCount = 2;
    private int _maxCount = 9;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        if (countText == null)
        {
            Debug.LogError($"[{nameof(CountSlot)}] countText is not assigned on {name}.", this);
        }
    }

    private void Start()
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
        return TryRaycastCycleSlot(eventData, cycleSlot => cycleSlot.SetSlotCount(_curCount));
    }

    #endregion

    #region Private Methods

    private void SetCountText()
    {
        countText.SetText($"{_curCount}");
    }

    private void UpCount()
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
