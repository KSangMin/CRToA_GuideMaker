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
        if (onLayoutRebuiltEvent != null)
        {
            onLayoutRebuiltEvent.RegisterListener(UpdateOverlays);
        }
    }

    private void OnDestroy()
    {
        if (onLayoutRebuiltEvent != null)
        {
            onLayoutRebuiltEvent.UnregisterListener(UpdateOverlays);
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

        // 씬 내의 CycleVerticalLayout을 찾거나 참조를 받아서 스캔
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
                if (!isStartForNewMarker) candidates.Add(id); // End 마커를 놓았다면, Start 마커를 찾음
            }

            foreach (var id in s.EndAreaIds)
            {
                if (!idCount.ContainsKey(id)) idCount[id] = 0;
                idCount[id]++;
                if (isStartForNewMarker) candidates.Add(id); // Start 마커를 놓았다면, End 마커를 찾음
            }
        }

        foreach (var id in candidates)
        {
            if (idCount[id] == 1) return id;
        }

        return null;
    }

    private void UpdateOverlays()
    {
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
            // 모든 줄을 기본 패딩(-1)과 NameField 없음(false)으로 초기화 세팅
            foreach (Transform child in cycleVerticalLayout.transform)
            {
                if (child.TryGetComponent(out CycleHorizontalLayout row))
                {
                    rowLevels[row] = -1; // -1 means default
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

            // 이 영역이 걸쳐 있는 모든 줄을 찾아 maxLevel 업데이트, 시작 줄은 hasName 업데이트
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

        if (layoutChanged && cycleVerticalLayout != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(cycleVerticalLayout.GetComponent<RectTransform>());
        }

        Canvas.ForceUpdateCanvases();

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
        if (boxObj.TryGetComponent(out AreaHighlightBox box))
        {
            box.Init(areaId, areaName, startSlot, endSlot, _flatSlots, level);
            _activeBoxes.Add(box);
        }
    }

    #endregion
}
