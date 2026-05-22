using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AreaOverlayPanel : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private EventChannel onLayoutRebuiltEvent;
    [SerializeField] private GameObject highlightBoxPrefab;

    #endregion

    #region Private Fields

    private readonly List<AreaHighlightBox> _activeBoxes = new();
    private readonly List<CycleSlot> _flatSlots = new();
    private readonly Dictionary<string, string> _savedAreaNames = new();
    private Coroutine _updateCoroutine;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (onLayoutRebuiltEvent == null)
        {
            Debug.LogError($"[{nameof(AreaOverlayPanel)}] onLayoutRebuiltEvent is missing.", this);
        }
        if (highlightBoxPrefab == null)
        {
            Debug.LogError($"[{nameof(AreaOverlayPanel)}] highlightBoxPrefab is missing.", this);
        }
    }

    private void Start()
    {
        var cycleVerticalLayout = UIManager.Instance.GetUI<UI_Result>().cyclePanel.GetComponentInChildren<CycleVerticalLayout>();
        if (cycleVerticalLayout != null && cycleVerticalLayout.onLayoutRebuiltEvent != null)
        {
            cycleVerticalLayout.onLayoutRebuiltEvent.RegisterListener(OnLayoutRebuilt);
        }
    }

    private void OnDestroy()
    {
        var cycleVerticalLayout = UIManager.Instance.GetUI<UI_Result>().cyclePanel.GetComponentInChildren<CycleVerticalLayout>();
        if (cycleVerticalLayout != null && cycleVerticalLayout.onLayoutRebuiltEvent != null)
        {
            cycleVerticalLayout.onLayoutRebuiltEvent.UnregisterListener(OnLayoutRebuilt);
        }
    }

    #endregion

    #region Public Methods

    public void HandleMarkerDropped(CycleSlot slot, bool isStart)
    {
        BuildFlatSlotList();
        string pairId = FindUnpairedAreaId(isStart);

        string targetId = pairId ?? System.Guid.NewGuid().ToString();

        if (isStart) slot.AddAreaStart(targetId, false);
        else slot.AddAreaEnd(targetId, false);

        UpdateOverlays();
    }

    #endregion

    #region Private Methods

    private void BuildFlatSlotList()
    {
        _flatSlots.Clear();

        var cycleVerticalLayout = UIManager.Instance.GetUI<UI_Result>().cyclePanel.GetComponentInChildren<CycleVerticalLayout>();
        if (cycleVerticalLayout == null) return;

        foreach (Transform rowTransform in cycleVerticalLayout.transform)
        {
            if (rowTransform.TryGetComponent(out CycleHorizontalLayout row))
            {
                foreach (Transform slotTransform in row.transform)
                {
                    if (slotTransform.TryGetComponent(out CycleSlot cycleSlot))
                    {
                        _flatSlots.Add(cycleSlot);
                    }
                }
            }
        }
    }

    private string FindUnpairedAreaId(bool isStartForNewMarker)
    {
        var idCount = new Dictionary<string, int>();
        var candidates = new List<string>();

        foreach (var s in _flatSlots)
        {
            foreach (var id in s.StartAreaIds)
            {
                if (!idCount.ContainsKey(id)) idCount[id] = 0;
                idCount[id]++;
                if (!isStartForNewMarker) candidates.Add(id);
            }

            foreach (var id in s.EndAreaIds)
            {
                if (!idCount.ContainsKey(id)) idCount[id] = 0;
                idCount[id]++;
                if (isStartForNewMarker) candidates.Add(id);
            }
        }

        foreach (var id in candidates)
        {
            if (idCount[id] == 1) return id;
        }

        return null;
    }

    private void OnLayoutRebuilt()
    {
        if (!gameObject.activeInHierarchy) return;
        
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        _updateCoroutine = StartCoroutine(UpdateOverlaysCoroutine());
    }

    public void UpdateOverlays()
    {
        OnLayoutRebuilt();
    }

    private IEnumerator UpdateOverlaysCoroutine()
    {
        // 첫 번째 대기: 슬롯 추가/삭제 등 외부 레이아웃 변경사항이 계층 구조에 반영되도록 1프레임 대기
        yield return null;

        BuildFlatSlotList();

        var areaDict = new Dictionary<string, List<CycleSlot>>();
        foreach (var s in _flatSlots)
        {
            var uniqueIds = new HashSet<string>(s.StartAreaIds);
            uniqueIds.UnionWith(s.EndAreaIds);

            foreach (var id in uniqueIds)
            {
                if (!areaDict.ContainsKey(id)) areaDict[id] = new List<CycleSlot>();
                areaDict[id].Add(s);
            }
        }

        var toResetIds = new List<string>();
        var activeAreas = new List<(string areaId, int start, int end, CycleSlot sSlot, CycleSlot eSlot)>();

        foreach (var kvp in areaDict)
        {
            string areaId = kvp.Key;
            var list = kvp.Value;

            if (list.Count == 2)
            {
                CycleSlot startSlot = list.Find(x => x.StartAreaIds.Contains(areaId));
                CycleSlot endSlot = list.Find(x => x.EndAreaIds.Contains(areaId));

                if (startSlot == null || endSlot == null)
                {
                    toResetIds.Add(areaId);
                }
                else
                {
                    int startIdx = _flatSlots.IndexOf(startSlot);
                    int endIdx = _flatSlots.IndexOf(endSlot);

                    if (startIdx <= endIdx)
                    {
                        activeAreas.Add((areaId, startIdx, endIdx, startSlot, endSlot));
                    }
                    else
                    {
                        toResetIds.Add(areaId);
                    }
                }
            }
            else if (list.Count > 2)
            {
                toResetIds.Add(areaId);
            }
        }

        activeAreas.Sort((a, b) => (a.end - a.start).CompareTo(b.end - b.start));

        var levels = new Dictionary<string, int>();
        var rowLevels = new Dictionary<CycleHorizontalLayout, int>();
        var rowHasName = new Dictionary<CycleHorizontalLayout, bool>();

        var cycleVerticalLayout = UIManager.Instance.GetUI<UI_Result>().cyclePanel.GetComponentInChildren<CycleVerticalLayout>();
        if (cycleVerticalLayout != null)
        {
            foreach (Transform child in cycleVerticalLayout.transform)
            {
                if (child.TryGetComponent(out CycleHorizontalLayout row))
                {
                    rowLevels[row] = -1;
                    rowHasName[row] = false;
                }
            }
        }

        foreach (var area in activeAreas)
        {
            int maxLevel = 0;
            foreach (var other in levels)
            {
                var otherArea = activeAreas.Find(x => x.areaId == other.Key);
                if (area.start <= otherArea.end && area.end >= otherArea.start)
                {
                    if (other.Value >= maxLevel)
                    {
                        maxLevel = other.Value + 1;
                    }
                }
            }
            levels[area.areaId] = maxLevel;

            var startRow = area.sSlot.transform.parent.GetComponent<CycleHorizontalLayout>();
            
            for (int i = area.start; i <= area.end; i++)
            {
                var row = _flatSlots[i].transform.parent.GetComponent<CycleHorizontalLayout>();
                if (row != null)
                {
                    if (!rowLevels.ContainsKey(row) || maxLevel > rowLevels[row])
                    {
                        rowLevels[row] = maxLevel;
                    }
                    if (row == startRow)
                    {
                        rowHasName[row] = true;
                    }
                }
            }
        }

        bool layoutChanged = false;
        foreach (var kvp in rowLevels)
        {
            bool hasName = rowHasName.ContainsKey(kvp.Key) && rowHasName[kvp.Key];
            if (kvp.Key.SetDynamicPadding(kvp.Value, hasName))
            {
                layoutChanged = true;
                LayoutRebuilder.ForceRebuildLayoutImmediate(kvp.Key.GetComponent<RectTransform>());
            }
        }

        if (cycleVerticalLayout != null)
        {
            // 유니티 ContentSizeFitter 버그(자식 삭제 시 높이가 줄어들 때 Y좌표 정렬이 1프레임/1리빌드 지연되는 현상)를
            // 방지하기 위해, 패딩 변경 여부와 상관없이 무조건 부모 레이아웃을 더블 리빌드합니다.
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(cycleVerticalLayout.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(cycleVerticalLayout.GetComponent<RectTransform>()); // 핵심: 두 번 강제 호출
            Canvas.ForceUpdateCanvases();
            
            // 패딩 변경, 또는 삭제로 인한 높이 수축(Shrink) 시 부모 ScrollRect가 LateUpdate에서
            // Content Y좌표를 보정(Shift)할 수 있도록 무조건 1프레임을 대기합니다.
            // 이 대기가 없으면 ScrollRect가 보정하기 전의 옛날 좌표를 읽게 되어 영역이 어긋납니다.
            yield return null;
        }

        foreach (var box in _activeBoxes)
        {
            if (box != null)
            {
                _savedAreaNames[box.AreaId] = box.AreaName;
                Destroy(box.gameObject);
            }
        }
        _activeBoxes.Clear();

        foreach (var area in activeAreas)
        {
            string oldName = _savedAreaNames.ContainsKey(area.areaId) ? _savedAreaNames[area.areaId] : "";
            CreateHighlightBox(area.areaId, oldName, area.sSlot, area.eSlot, levels[area.areaId]);
        }

        if (toResetIds.Count > 0)
        {
            foreach (var slot in _flatSlots)
            {
                foreach (var id in toResetIds)
                {
                    slot.RemoveArea(id, false);
                }
            }
        }
    }

    private void CreateHighlightBox(string areaId, string areaName, CycleSlot startSlot, CycleSlot endSlot, int level)
    {
        if (highlightBoxPrefab == null) return;

        var boxObj = Instantiate(highlightBoxPrefab, transform);
        
        // 새로 생성된 박스의 RectTransform 행렬이 완벽히 초기화되도록 강제 갱신
        // (초기화되지 않은 상태에서 Init 내부의 GetLocalPos를 호출하면 행렬 오차로 인해 좌표가 어긋날 수 있음)
        Canvas.ForceUpdateCanvases();
        
        if (boxObj.TryGetComponent(out AreaHighlightBox box))
        {
            box.Init(areaId, areaName, startSlot, endSlot, _flatSlots, level);
            _activeBoxes.Add(box);
        }
    }

    #endregion
}
