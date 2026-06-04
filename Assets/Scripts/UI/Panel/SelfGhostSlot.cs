using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SelfGhostSlot : Slot
{
    #region Serialized Fields

    [SerializeField] private GameObject ghostObject;

    #endregion

    #region Private Fields

    private GameObject _ghost;
    private RectTransform _ghostRect;

    #endregion

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        if (ghostObject == null)
        {
            Debug.LogError($"[{nameof(SelfGhostSlot)}] ghostObject is not assigned on {name}.", this);
        }
    }

    #endregion

    #region Public Methods

    public void SetSlot(ScrollRect panelScroll)
    {
        _panelScroll = panelScroll;
    }

    #endregion

    #region Protected Methods

    protected bool HasGhost => _ghost != null;

    protected override void OnSlotPointerDown(PointerEventData eventData)
    {
        WaitHoldThen(eventData, () => CreateGhost(eventData));
    }

    protected override void OnSlotPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (_ghost == null && !eventData.dragging)
        {
            OnSelfGhostClick(eventData);
        }

        if (_ghost != null)
        {
            ProcessDrop(eventData);
            DestroyGhost();
            return;
        }
    }

    protected override void OnSlotBeginDrag(PointerEventData eventData)
    {
        TryBeginPanelScrollDrag(eventData, HasGhost);
    }

    protected override void OnSlotDrag(PointerEventData eventData)
    {
        if (_ghost != null)
        {
            SetRectTransformToPointer(_ghostRect, eventData);
            return;
        }

        ForwardPanelScrollDrag(eventData, HasGhost);
    }

    protected override void OnSlotEndDrag(PointerEventData eventData)
    {
        if (EndPanelScrollDrag(eventData))
        {
            return;
        }

        CancelHold();
    }

    protected virtual void OnSelfGhostClick(PointerEventData eventData)
    {
    }

    protected abstract bool ProcessDrop(PointerEventData eventData);

    protected bool TryRaycastCycleSlot(PointerEventData eventData, System.Action<CycleSlot> onHit)
    {
        RaycastAll(eventData);

        foreach (RaycastResult result in RaycastBuffer)
        {
            if (result.gameObject.CompareTag("CycleSlot"))
            {
                CycleSlot cycleSlot = result.gameObject.GetComponent<CycleSlot>();
                onHit(cycleSlot);
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Private Methods

    private void CreateGhost(PointerEventData eventData)
    {
        _ghost = Instantiate(ghostObject, GetGhostParent());
        _ghostRect = _ghost.GetComponent<RectTransform>();
        SetRectTransformToPointer(_ghostRect, eventData);
    }

    private void DestroyGhost()
    {
        if (_ghost != null)
        {
            _ghost.transform.SetParent(null);
            Destroy(_ghost);
            _ghost = null;
            _ghostRect = null;
        }
    }

    #endregion
}
