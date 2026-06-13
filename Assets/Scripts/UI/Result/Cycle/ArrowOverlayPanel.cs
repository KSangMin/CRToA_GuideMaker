using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowOverlayPanel : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private CycleVerticalLayout cycleVert;
    [SerializeField] private EventChannel onLayoutRebuiltEvent;
    [SerializeField] private GameObject arrowRendererPrefab;

    #endregion

    #region Private Fields

    private readonly List<ArrowRenderer> _activeArrows = new();
    private readonly List<CycleSlot> _flatSlots = new();
    private readonly Dictionary<string, Color> _savedArrowColors = new();
    private readonly Dictionary<string, int> _savedArrowCounts = new();
    private Coroutine _updateCoroutine;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (onLayoutRebuiltEvent == null)
        {
            Debug.LogError($"[{nameof(ArrowOverlayPanel)}] onLayoutRebuiltEvent is missing.", this);
        }
        
        if (arrowRendererPrefab == null)
        {
            Debug.LogError($"[{nameof(ArrowOverlayPanel)}] arrowRendererPrefab is missing.", this);
        }
    }

    private void Start()
    {
        if (cycleVert != null && cycleVert.onLayoutRebuiltEvent != null)
        {
            cycleVert.onLayoutRebuiltEvent.RegisterListener(OnLayoutRebuilt);
        }
    }

    private void OnDestroy()
    {
        if (cycleVert != null && cycleVert.onLayoutRebuiltEvent != null)
        {
            cycleVert.onLayoutRebuiltEvent.UnregisterListener(OnLayoutRebuilt);
        }
    }

    #endregion

    #region Public Methods

    public void HandleMarkerDropped(CycleSlot slot, bool isStart)
    {
        BuildFlatSlotList();
        string pairId = FindUnpairedArrowId(isStart);
        string targetId = pairId ?? System.Guid.NewGuid().ToString();

        if (isStart)
        {
            slot.AddArrowStart(targetId, true);
        }
        else
        {
            slot.AddArrowEnd(targetId, true);
        }
    }

    public void UpdateOverlays()
    {
        OnLayoutRebuilt();
    }

    #endregion

    #region Private Methods

    private void BuildFlatSlotList()
    {
        _flatSlots.Clear();
        
        if (cycleVert == null)
        {
            return;
        }

        foreach (Transform rowTransform in cycleVert.transform)
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

    private string FindUnpairedArrowId(bool isStartForNewMarker)
    {
        var idCount = new Dictionary<string, int>();
        var candidates = new List<string>();

        foreach (var s in _flatSlots)
        {
            foreach (var id in s.StartArrowIds)
            {
                if (!idCount.ContainsKey(id))
                {
                    idCount[id] = 0;
                }
                
                idCount[id]++;
                
                if (!isStartForNewMarker)
                {
                    candidates.Add(id);
                }
            }
            
            foreach (var id in s.EndArrowIds)
            {
                if (!idCount.ContainsKey(id))
                {
                    idCount[id] = 0;
                }
                
                idCount[id]++;
                
                if (isStartForNewMarker)
                {
                    candidates.Add(id);
                }
            }
        }

        foreach (var id in candidates)
        {
            if (idCount[id] == 1)
            {
                return id;
            }
        }
        
        return null;
    }

    private void OnLayoutRebuilt()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        
        _updateCoroutine = StartCoroutine(UpdateOverlaysCoroutine());
    }

    private IEnumerator UpdateOverlaysCoroutine()
    {
        yield return null;
        BuildFlatSlotList();

        var arrowDict = new Dictionary<string, List<CycleSlot>>();
        foreach (var s in _flatSlots)
        {
            var uniqueIds = new HashSet<string>(s.StartArrowIds);
            uniqueIds.UnionWith(s.EndArrowIds);

            foreach (var id in uniqueIds)
            {
                if (!arrowDict.ContainsKey(id))
                {
                    arrowDict[id] = new List<CycleSlot>();
                }
                
                arrowDict[id].Add(s);
            }
        }

        var toResetIds = new List<string>();
        var activeArrows = new List<(string arrowId, CycleSlot sSlot, CycleSlot eSlot)>();
        var seenPairs = new HashSet<(CycleSlot, CycleSlot)>();

        foreach (var kvp in arrowDict)
        {
            string arrowId = kvp.Key;
            var list = kvp.Value;

            if (list.Count == 2)
            {
                CycleSlot startSlot = list.Find(x => x.StartArrowIds.Contains(arrowId));
                CycleSlot endSlot = list.Find(x => x.EndArrowIds.Contains(arrowId));

                if (startSlot == null || endSlot == null)
                {
                    toResetIds.Add(arrowId);
                }
                else if (startSlot == endSlot)
                {
                    toResetIds.Add(arrowId);
                }
                else
                {
                    var pair = (startSlot, endSlot);
                    if (seenPairs.Contains(pair))
                    {
                        toResetIds.Add(arrowId);
                    }
                    else
                    {
                        seenPairs.Add(pair);
                        activeArrows.Add((arrowId, startSlot, endSlot));
                    }
                }
            }
            else if (list.Count > 2)
            {
                toResetIds.Add(arrowId);
            }
        }

        if (cycleVert != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(cycleVert.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(cycleVert.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
            
            yield return null;
        }

        foreach (var arrow in _activeArrows)
        {
            if (arrow != null)
            {
                _savedArrowCounts[arrow.ArrowId] = arrow.LoopCount;
                Destroy(arrow.gameObject);
            }
        }
        
        _activeArrows.Clear();

        foreach (var arrow in activeArrows)
        {
            Color arrowColor;
            
            if (_savedArrowColors.ContainsKey(arrow.arrowId))
            {
                arrowColor = _savedArrowColors[arrow.arrowId];
            }
            else
            {
                arrowColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
                _savedArrowColors[arrow.arrowId] = arrowColor;
            }

            int prevCount = _savedArrowCounts.ContainsKey(arrow.arrowId) ? _savedArrowCounts[arrow.arrowId] : 1;
            CreateArrowRenderer(arrow.arrowId, arrow.sSlot, arrow.eSlot, arrowColor, prevCount);
        }

        // 화살표 렌더링 오브젝트가 생성되고 위치가 정렬된 후 텍스트 버튼 영역만 계산하여 여백 확보
        ApplyExactPadding();

        if (toResetIds.Count > 0)
        {
            foreach (var slot in _flatSlots)
            {
                foreach (var id in toResetIds)
                {
                    slot.RemoveArrow(id, false);
                }
            }
        }
    }

    private void CreateArrowRenderer(string arrowId, CycleSlot startSlot, CycleSlot endSlot, Color color, int loopCount)
    {
        if (arrowRendererPrefab == null)
        {
            return;
        }

        var obj = Instantiate(arrowRendererPrefab, transform);
        Canvas.ForceUpdateCanvases();
        
        if (obj.TryGetComponent(out ArrowRenderer renderer))
        {
            renderer.Init(arrowId, startSlot, endSlot, color, loopCount);
            _activeArrows.Add(renderer);
        }
    }
    
    private void ApplyExactPadding()
    {
        if (cycleVert == null) return;
        if (!cycleVert.TryGetComponent(out VerticalLayoutGroup vlg)) return;

        RectTransform cvRT = cycleVert.GetComponent<RectTransform>();
        
        // 1. 현재 패딩이 제거된 '순수 콘텐츠'의 로컬 경계선 도출 (수학적 불변량)
        float contentTop = cvRT.rect.yMax - vlg.padding.top;
        float contentBottom = cvRT.rect.yMin + vlg.padding.bottom;
        float contentLeft = cvRT.rect.xMin + vlg.padding.left;
        float contentRight = cvRT.rect.xMax - vlg.padding.right;

        // 화살표 렌더링 요소들의 최대 도달 좌표 초기화 (최소한 콘텐츠 영역은 덮도록)
        float arrowMaxY = contentTop;
        float arrowMinY = contentBottom;
        float arrowMaxX = contentRight;
        float arrowMinX = contentLeft;

        foreach (var arrow in _activeArrows)
        {
            if (arrow == null || arrow.CountButtonRT == null) continue;
            
            RectTransform btnRT = arrow.CountButtonRT;
            Vector3 btnCenter = cvRT.InverseTransformPoint(btnRT.position);
            
            float halfWidth = btnRT.rect.width * 0.5f;
            float halfHeight = btnRT.rect.height * 0.5f;

            float btnTop = btnCenter.y + halfHeight;
            float btnBottom = btnCenter.y - halfHeight;
            float btnRight = btnCenter.x + halfWidth;
            float btnLeft = btnCenter.x - halfWidth;

            arrowMaxY = Mathf.Max(arrowMaxY, btnTop);
            arrowMinY = Mathf.Min(arrowMinY, btnBottom);
            arrowMaxX = Mathf.Max(arrowMaxX, btnRight);
            arrowMinX = Mathf.Min(arrowMinX, btnLeft);
        }

        // 2. 순수 콘텐츠 영역을 초과(Overflow)한 수치만 패딩으로 산출
        // (Area 등 다른 패널이 행(Row)을 아래로 밀어냈다면, p1/p2 좌표도 이미 내려가 있으므로 자연스럽게 상쇄됨)
        int padTop = Mathf.CeilToInt(Mathf.Max(0, arrowMaxY - contentTop));
        int padBottom = Mathf.CeilToInt(Mathf.Max(0, contentBottom - arrowMinY));
        int padLeft = Mathf.CeilToInt(Mathf.Max(0, contentLeft - arrowMinX));
        int padRight = Mathf.CeilToInt(Mathf.Max(0, arrowMaxX - contentRight));

        if (vlg.padding.top != padTop || vlg.padding.bottom != padBottom || 
            vlg.padding.left != padLeft || vlg.padding.right != padRight)
        {
            vlg.padding = new RectOffset(padLeft, padRight, padTop, padBottom);
            LayoutRebuilder.ForceRebuildLayoutImmediate(cvRT);
        }
    }

    #endregion
}
