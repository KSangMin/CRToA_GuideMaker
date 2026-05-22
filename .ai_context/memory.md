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