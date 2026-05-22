using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AreaHighlightBox : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private RectTransform connectingLinesParent;
    [SerializeField] private RectTransform startBracket;
    [SerializeField] private RectTransform endBracket;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private GameObject linePiecePrefab;
    [SerializeField] private float yOffset = 5f;
    [SerializeField] private float lineThickness = 3f;
    [SerializeField] private float bracketHeight = 10f;
    [SerializeField] private float levelOffset = 30f;

    #endregion

    #region Private Fields

    private string _areaId;
    private string _initialName;
    private readonly List<GameObject> _spawnedLines = new();

    public string AreaId => _areaId;
    public string AreaName => nameInputField.text;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (!string.IsNullOrEmpty(_initialName))
        {
            nameInputField.text = _initialName;
        }
    }

    #endregion

    #region Public Methods

    public void Init(string areaId, string areaName, CycleSlot startSlot, CycleSlot endSlot, List<CycleSlot> flatSlots, int level)
    {
        _areaId = areaId;
        _initialName = string.IsNullOrEmpty(areaName) ? "영역 이름" : areaName;
        nameInputField.text = _initialName;

        float levelYOffset = yOffset + (level * levelOffset);
        float currentBracketHeight = bracketHeight + (level * levelOffset);

        int startIndex = flatSlots.IndexOf(startSlot);
        int endIndex = flatSlots.IndexOf(endSlot);

        if (startIndex == -1 || endIndex == -1 || startIndex > endIndex)
        {
            Debug.LogWarning($"[{nameof(AreaHighlightBox)}] Invalid slot order or missing slots.");
            return;
        }

        // 1. 포함된 모든 슬롯들을 행 단위로 그룹화
        var rowGroups = new Dictionary<Transform, List<CycleSlot>>();
        for (int i = startIndex; i <= endIndex; i++)
        {
            var slot = flatSlots[i];
            var row = slot.transform.parent; // CycleHorizontalLayout
            if (!rowGroups.ContainsKey(row))
            {
                rowGroups[row] = new List<CycleSlot>();
            }
            rowGroups[row].Add(slot);
        }

        // 2. 가이드 라인 그리기 (각 행별 분할 렌더링)
        ClearSpawnedLines();

        RectTransform panelRect = GetComponent<RectTransform>();
        // 내부 UI 요소들의 앵커를 실제 부모 피벗과 일치시켜 anchoredPosition 좌표계를 통일
        RectTransform startBracketParent = (RectTransform)startBracket.parent;
        startBracket.anchorMin = startBracketParent.pivot;
        startBracket.anchorMax = startBracketParent.pivot;
        startBracket.pivot = new Vector2(0, 1); // Left Top

        RectTransform endBracketParent = (RectTransform)endBracket.parent;
        endBracket.anchorMin = endBracketParent.pivot;
        endBracket.anchorMax = endBracketParent.pivot;
        endBracket.pivot = new Vector2(1, 1); // Right Top

        RectTransform nameInputRect = nameInputField.GetComponent<RectTransform>();
        RectTransform nameInputParent = (RectTransform)nameInputRect.parent;
        nameInputRect.anchorMin = nameInputParent.pivot;
        nameInputRect.anchorMax = nameInputParent.pivot;
        nameInputRect.pivot = new Vector2(0.5f, 0); // Bottom Center

        foreach (var kvp in rowGroups)
        {
            Transform rowTransform = kvp.Key;
            List<CycleSlot> slotsInRow = kvp.Value;

            if (slotsInRow.Count == 0) continue;

            RectTransform firstSlotRect = slotsInRow[0].GetComponent<RectTransform>();
            RectTransform lastSlotRect = slotsInRow[^1].GetComponent<RectTransform>();

            // 영역이 이전 행에서 이어져 내려온 경우, 무조건 현재 행의 맨 앞 슬롯부터 선을 그림
            if (!slotsInRow.Contains(startSlot) && rowTransform.childCount > 0)
            {
                firstSlotRect = rowTransform.GetChild(0).GetComponent<RectTransform>();
            }

            // 영역이 다음 행으로 이어져 넘어가는 경우, 무조건 현재 행의 맨 끝 슬롯까지 선을 그림
            if (!slotsInRow.Contains(endSlot) && rowTransform.childCount > 0)
            {
                lastSlotRect = rowTransform.GetChild(rowTransform.childCount - 1).GetComponent<RectTransform>();
            }

            // 월드 좌표를 connectingLinesParent 로컬 좌표로 변환
            Vector2 firstLocalPos = GetLocalPos(connectingLinesParent, firstSlotRect, new Vector2(0f, 1f)); // Left Top
            Vector2 lastLocalPos = GetLocalPos(connectingLinesParent, lastSlotRect, new Vector2(1f, 1f));   // Right Top

            float leftX = firstLocalPos.x;
            float rightX = lastLocalPos.x;
            float topY = firstLocalPos.y + levelYOffset;

            // 라인 인스턴스 생성
            var lineObj = Instantiate(linePiecePrefab, connectingLinesParent);
            var lineRect = lineObj.GetComponent<RectTransform>();
            RectTransform lineParent = connectingLinesParent;
            lineRect.anchorMin = lineParent.pivot;
            lineRect.anchorMax = lineParent.pivot;
            lineRect.pivot = new Vector2(0, 0.5f);
            
            lineRect.anchoredPosition = new Vector2(leftX, topY);
            lineRect.sizeDelta = new Vector2(rightX - leftX, lineThickness); 

            _spawnedLines.Add(lineObj);

            // 브래킷 위치 갱신 (시작 행이면 StartBracket, 끝 행이면 EndBracket)
            if (slotsInRow.Contains(startSlot))
            {
                Vector2 startBracketPos = GetLocalPos((RectTransform)startBracket.parent, firstSlotRect, new Vector2(0f, 1f));
                startBracket.anchoredPosition = new Vector2(startBracketPos.x, startBracketPos.y + levelYOffset);
                startBracket.sizeDelta = new Vector2(lineThickness, currentBracketHeight);
            }

            if (slotsInRow.Contains(endSlot))
            {
                Vector2 endBracketPos = GetLocalPos((RectTransform)endBracket.parent, lastSlotRect, new Vector2(1f, 1f));
                endBracket.anchoredPosition = new Vector2(endBracketPos.x, endBracketPos.y + levelYOffset);
                endBracket.sizeDelta = new Vector2(lineThickness, currentBracketHeight);
            }
        }

        // 3. NameInputField 위치 갱신 (StartSlot 상단, 가이드라인 바로 위)
        RectTransform startSlotRect = startSlot.GetComponent<RectTransform>();
        Vector2 startLocalTop = GetLocalPos(nameInputParent, startSlotRect, new Vector2(0.5f, 1f));
        nameInputField.GetComponent<RectTransform>().anchoredPosition = new Vector2(startLocalTop.x, startLocalTop.y + levelYOffset);
    }

    #endregion

    #region Private Methods

    private void ClearSpawnedLines()
    {
        foreach (var line in _spawnedLines)
        {
            if (line != null) Destroy(line);
        }
        _spawnedLines.Clear();
    }

    private Vector2 GetLocalPos(RectTransform parentRect, RectTransform targetRect, Vector2 pivot)
    {
        // 대상 슬롯의 특정 pivot 지점의 로컬 좌표 계산
        Vector2 targetCenter = targetRect.rect.center;
        Vector2 targetSize = targetRect.rect.size;
        Vector2 pivotPos = targetCenter - targetSize * 0.5f + targetSize * pivot;

        Vector3 worldPos = targetRect.TransformPoint(pivotPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, RectTransformUtility.WorldToScreenPoint(null, worldPos), null, out Vector2 localPos);
        return localPos;
    }

    #endregion
}
