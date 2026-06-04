# 🧠 Project Long-Term Memory (Trauma Note)

This document records critical engineering mistakes, resolved bugs, and strict optimization lessons to prevent regression. **All agents must read this before writing code.**

---

### 🚨 [2026-05-22] Unity UI: AreaHighlightBox 위치 엇나감 (Odd/Even Toggle) 및 레이아웃 지연 이슈 총정리
- **Issue**: `AreaOverlayPanel`에서 `CycleSlot`의 좌표를 읽어 영역 상자를 그릴 때, 줄(Row)이 추가되거나 삭제될 때마다 상자의 Y좌표가 짝수/홀수 번마다 번갈아가며 어긋나는 핑퐁 현상 및 정확히 52.5(슬롯 절반 높이)만큼 영구적으로 어긋나는 현상 발생.
- **Root Cause & Resolution**: 유니티 UI 렌더링 파이프라인의 3가지 복합적인 지연/동기화 버그가 겹쳐서 발생함. 향후 UI 좌표계를 월드 좌표 기준으로 동기화해야 할 때는 반드시 아래 3가지 원칙을 지킬 것.

#### 1. ContentSizeFitter 수축(Shrink) 1프레임 지연 버그
- **Root Cause**: `ContentSizeFitter`가 달린 부모 객체에서 자식 오브젝트가 삭제되어 전체 높이가 줄어들 때, `VerticalLayoutGroup`이 줄어든 전체 높이를 기준으로 내부 자식들의 Y좌표를 갱신하는 처리가 1프레임(또는 1 Layout Pass) 지연되는 유니티 고질병. 단일 `Canvas.ForceUpdateCanvases()`나 단일 `ForceRebuildLayoutImmediate`로는 해결되지 않고 과거 좌표에 머무름.
- **Resolution**: 레이아웃이 쪼그라들 때는 반드시 부모 레이아웃을 **2연속으로 강제 리빌드** 해야 완벽한 최신 Y좌표가 정렬됨.
  ```csharp
  Canvas.ForceUpdateCanvases();
  LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
  LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect); // 2번 강제 호출!
  Canvas.ForceUpdateCanvases();
  ```

#### 2. ScrollRect LateUpdate 강제 보정(Shift) 충돌 버그
- **Root Cause**: 스크롤뷰 내부의 Content 크기(레이아웃 높이)가 변하면, `ScrollRect` 컴포넌트가 화면 바깥으로 벗어난 여백을 메꾸기 위해 **`LateUpdate`** 단계에서 Content의 `anchoredPosition`을 스윽 이동(보정)시켜버림. 코루틴에서 `Update` 시점에 강제 리빌드를 마치고 완벽하다고 생각하여 좌표를 읽었으나, 직후 `LateUpdate`에서 `ScrollRect`가 슬롯들을 통째로 이동시켜버려 그 거리(52.5)만큼 좌표가 영구적으로 엇나감.
- **Resolution**: 레이아웃 높이가 변했다면 그 프레임의 `Update` 단계에서 좌표를 읽지 말고, 무조건 **`yield return null;`**로 1프레임을 더 넘겨서 `LateUpdate`의 스크롤 보정까지 완벽하게 끝난 다음 프레임 `Update` 시점에 정착된 좌표를 읽어야 함. (`yield return new WaitForEndOfFrame();`은 렌더링 직후이므로 `Canvas.ForceUpdateCanvases`가 오작동할 수 있어 사용 금지)

#### 3. Instantiate 직후 UI RectTransform 행렬 캐시 누락 버그
- **Root Cause**: `Instantiate`로 UI 프리팹(`AreaHighlightBox`)을 씬에 막 생성한 직후, 곧바로 해당 객체의 자식 RectTransform을 참조하여 `RectTransformUtility.ScreenPointToLocalPointInRectangle` 등의 로컬 좌표 변환 연산을 수행하면, 유니티가 캔버스 행렬을 채 업데이트하기 전이라 엉뚱한(초기화되지 않은) 행렬 캐시로 오차가 발생함.
- **Resolution**: UI 요소를 `Instantiate` 한 직후에 그 객체의 로컬 좌표계를 기준으로 수학적 연산을 해야 한다면, 반드시 연산 직전에 `Canvas.ForceUpdateCanvases();`를 한 번 호출하여 행렬을 강제 동기화할 것.

### 🚨 [2026-05-29] UI 마커 (Area/Arrow) 짝 매칭 시 단일 마커 강제 삭제(Auto-Delete) 논리 오류
- **Issue**: `ArrowStartSlot` 등 단일 마커를 씬에 드롭하자마자 아무런 에러 로그 없이 즉시 삭제되는 버그 발생. (과거 `AreaSlot` 구현 시에도 동일한 논리적 실수가 발생했었음)
- **Root Cause**: `UpdateOverlaysCoroutine` 등에서 마커들을 `ArrowId` 단위로 그룹화(`Dictionary<string, List<CycleSlot>>`)할 때, 아직 두 번째 짝이 드롭되지 않아 배열 크기가 1인 경우(`list.Count == 1`)를 고려하지 않고 포괄적인 `else { toResetIds.Add(id); }`로 묶어버림. 이로 인해 혼자 대기해야 할 정상 마커가 예외로 취급되어 삭제(Reset) 로직을 타게 됨.
- **Resolution**: 무결성 검증 시 `else`를 지양하고 명확히 `else if (list.Count > 2)`로 조건을 제한하여, 카운트가 1개인 마커는 렌더링만 건너뛰고 데이터 상에 온전히 대기하도록 보존해야 함.

### 🚨 [2026-05-31] Unity UI: 오브젝트 좌표 직접 참조 시 발생하는 1프레임 스텔스 오차 (1-Frame Layout Sync Bug)
- **Issue**: 화살표가 여러 개 렌더링되거나 새로운 Area 영역이 추가되었을 때, 이중 패딩이나 과도한 패딩이 발생하는 문제. 수학적 계산 알고리즘 자체는 완벽했으나 오브젝트의 실제 글로벌 좌표를 직접 읽어들이는 방식(`CountButtonRT.position`)에서 오차가 발생함.
- **Root Cause**: `ArrowRenderer`가 여러 개 연달아 생성될 때, 부모인 `VerticalLayoutGroup`은 새 오브젝트를 자동 정렬하며 먼저 생성된 기존 오브젝트들을 아래로 밀어냄. 이때 기존 오브젝트의 글로벌 부모 트랜스폼(`_parentRT`)은 레이아웃에 의해 밀려났지만, 내부에 배치된 자식 버튼(`_loopCountBtnRT`)은 아직 밀려나기 전의 과거 로컬 오프셋 위치를 그대로 유지한 채 `ApplyExactPadding` 연산식으로 넘겨짐. 이 '1프레임 스텔스 엇갈림' 때문에 돌출 검사 수치가 비정상적으로 뻥튀기됨.
- **Resolution**: 수학적 시뮬레이션 대신 로컬 자식 노드의 좌표 오프셋을 글로벌로 직접 꺼내쓰는 아키텍처에서는, 여백 계산식에 위치 변수를 읽어들이기 **직전**에 반드시 `ForceUpdatePositions()`와 같은 함수를 호출하여 현재 밀려난 부모 좌표계를 기준으로 로컬 오프셋 연산을 강제 재동기화(Sync)해야 함.

### 🚨 [2026-05-31] 유니티 마커 이벤트 통신 아키텍처 결함 (이벤트 누락으로 인한 여백 중복 버그)
- **Issue**: Arrow 마커로 인해 최상단 컨테이너 여백이 늘어나 있는 상태에서, Area 마커를 드롭해 내부 행(Row) 패딩이 크게 늘어나면, 기존 화살표 여백이 흡수되거나 0으로 재조정되지 않고 방치되어 패딩이 무식하게 중복 합산되는 버그.
- **Root Cause**: 퍼포먼스 오버헤드를 막는답시고 `HandleMarkerDropped` 내부에서 슬롯에 마커를 추가할 때 `AddAreaStart(id, false)`처럼 `false` 플래그를 넘겨 레이아웃 전체 리빌드 이벤트(`onLayoutRebuiltEvent.RaiseEvent()`) 전파를 의도적으로 단절시키고 있었음. 이 때문에 상대방 패널이 레이아웃을 변형시켰음에도 다른 패널들은 레이아웃이 변했다는 사실 자체를 통보받지 못해 여백 재계산 코루틴 자체가 실행조차 되지 못하고 완전히 스킵되었음.
- **Resolution**: 뷰나 UI 구조에 물리적인 변화(패딩 조절 등)를 야기하는 컴포넌트 간 상호작용에서는 절대 이벤트 전파를 인위적으로 막아서는 안 됨. 타겟 플래그를 `true`로 설정하여 글로벌 이벤트 채널을 통해 모든 리스너 오버레이들이 한날한시에 같이 깨어나 레이아웃 변화를 공동으로 감지하고 각자의 패딩을 재동기화하도록 아키텍처를 바로잡아야 함.

### 🚨 [2026-06-02] Unity UI: TMP_InputField (CycleTitle) 줄바꿈 레이아웃 어긋남 및 텍스트 삭제 시 축소 불가 버그
- **Issue**: InputField에 텍스트를 입력하여 줄바꿈이 발생할 때 홀수/짝수 번째마다 레이아웃이 지연되어 어긋나고, 텍스트를 한 번에 다 지우면 Title 영역 높이가 줄어들지 않고 그대로 방치되는 현상 발생.
- **Root Cause & Resolution**:
  1. **캐시된 낡은 높이 참조**: 텍스트 입력이나 가로폭 변화 시점의 Canvas 물리 갱신 전에 `preferredHeight`를 가져오면서 엇박자가 났음. `LateUpdate`에서 `text` 문자열 및 `rectTransform.rect.width`의 변화를 캐싱하여 실제 변화가 발생했을 때만 `textComponent.ForceMeshUpdate()`를 강제 호출해 정확한 높이를 가져오도록 해결.
  2. **공백 입력 시의 예외**: 텍스트가 완전히 지워졌을 때 Preferred Height 갱신이 보장되지 않는 문제를 막기 위해, `string.IsNullOrEmpty` 상태일 때는 타겟 높이를 0f로 강제하여 최소 높이(50f)로 수축되게 유도함.
  3. **ContentSizeFitter 수축 지연 및 비효율적인 전체 리빌드**: 변경 사항이 최상위 부모인 `Content`로 즉각 전파되지 못해 발생한 문제. `CycleTitle`이 변할 때는 무의미한 슬롯 행들(`_rows`)의 리빌드 순회를 생략하고, 자식(`TitleWrapper`)과 최상위 부모(`Content`)에 대해서만 **2연속 강제 리빌드**(`LayoutRebuilder.ForceRebuildLayoutImmediate` 2회 호출)를 돌려 즉각 동기화되도록 수정.
