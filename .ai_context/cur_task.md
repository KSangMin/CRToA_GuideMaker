# 📍 Current Task: Slot 상속 구조 OOP 검증 및 코드 정돈

## 🎯 Current Goal
- 슬롯 계층(`Slot` → `DraggableSlot` / 패널·타임라인 파생)이 **역할 분리·추상화·DRY** 기준을 충족하도록 중간 계층(`PanelGhostSlot`)을 도입하고, 자식 클래스에는 **도메인 특화 동작만** 남긴다.
- `TabSlot` 등 미마이그레이션 슬롯은 본 스프린트 범위 밖이며, 타임라인/패널 슬롯 5종 정돈 완료 후 2차로 편입한다.

## 📝 Todo List

### 1단계: 설계 및 데이터 구조화 (Planner)
- [x] `.ai_context/architecture.md` 슬롯 계층·OOP 검증·목표 클래스 다이어그램 반영
- [x] `CountSlot`/`ResetSlot`/`CycleSlot`/`SelectSlot` 중복 맵 및 통합 방향 문서화
- [ ] (선택) `SkillSlotDisplay` 컴포넌트 분리 여부 확정 — `SelectSlot`·`CycleSlot` 공통 SerializeField 목록 확정 후 진행

### 2단계: 핵심 C# 스크립팅 (Builder)

#### 2-A. 패널 특수 슬롯 통합 (`PanelGhostSlot`)
- [ ] `Assets/Scripts/UI/Panel/PanelGhostSlot.cs` 신규: `Slot` 상속, `CountSlot`/`ResetSlot` 공통 필드(`ghostObject`, `_panelScroll`, `_ghost`, `_ghostRect`, `_isDraggingScroll`) 및 `OnSlotPointerDown`~`OnSlotEndDrag`·`ResolveClickOrDragDropPointerUp` 파이프라인 구현
- [ ] `PanelGhostSlot`에 `protected abstract bool ProcessDrop(PointerEventData eventData)` 및 `protected virtual void OnPanelGhostClick(PointerEventData eventData)` 훅 정의
- [ ] `Assets/Scripts/UI/Panel/Special/CountSlot.cs` — `PanelGhostSlot` 상속으로 축소: `UpCount` + `ProcessDrop`(태그 `CycleSlot` → `SetSlotCount`)만 유지
- [ ] `Assets/Scripts/UI/Panel/Special/ResetSlot.cs` — `PanelGhostSlot` 상속으로 축소: `ProcessDrop`(→ `ResetSlot`)만 유지
- [ ] `PanelGhostSlot` 레이캐스트에 `DraggableSlot`과 동일한 **재사용 `List<RaycastResult>` 버퍼** 적용 (GC 스파이크 방지)

#### 2-B. 타임라인 슬롯 (`CycleSlot`) 정리
- [ ] `CycleSlot.CheckHoldAfterDelay` 제거 → `WaitHoldThen(eventData, OnHoldElapsedReparent)`로 교체 (`holdDelaySeconds`는 부모 `Slot` 필드 사용)
- [ ] `CycleSlot.OnSlotBeginDrag` — `base.OnSlotBeginDrag(eventData)` 호출로 `DraggableSlot` 가상 체인 복원 (`_isDragging`일 때 early return은 유지)
- [ ] `CycleSlot.SetPositionToPointer` public 래퍼 제거 또는 `SetRectTransformToPointer` 직접 호출로 통일 (`SelectSlot` 참조부 함께 수정)
- [ ] `CycleSlot.CheckForPlaceHolder` — 인스턴스 `List` 할당 제거, protected 버퍼 또는 static 헬퍼로 통합

#### 2-C. 패널 선택 슬롯 (`SelectSlot`) 정리
- [ ] `SelectSlot` — `OnBeforeScrollDragForwarded` 빈 override 제거 (불필요 시)
- [ ] (2-A 완료 후) `SkillSlotDisplay` 추출 시: `Assets/Scripts/UI/Panel/SkillSlotDisplay.cs` 생성, `SelectSlot`/`CycleSlot`의 charge/icon/head/nameText/`ControlType` 분기 이전

#### 2-D. 베이스 계층 (`Slot` / `DraggableSlot`) 역할 재정의
- [ ] `DraggableSlot` XML 주석·클래스 summary를 **「패널 스크롤 전달 + 사이클 레이아웃 드롭」** 으로 명확화 (타임라인 전용이 아님을 문서화)
- [ ] `Slot`에 `protected List<RaycastResult> RaycastBuffer` (또는 static thread-safe 버퍼) 추가 검토 — `CountSlot`/`ResetSlot`/`CycleSlot` 공용
- [ ] `Slot.InstantiateGhostUnderPanel`의 `UIManager.Instance` 직접 참조 유지 여부 기록 (`memory.md`에 패턴 추가는 QA 단계)

#### 2-E. 컨벤션 정렬 (변경 파일 한정)
- [ ] `CycleSlot`: `holdCoroutine` 등 부모 protected 필드와 네이밍 일관 (`_` private / SerializeField camelCase per `convention.md`)
- [ ] `CycleSlot.originalParent` — `[HideInInspector] public` → `protected` + 필요 시 프로퍼티로 노출 검토
- [ ] Allman brace·선언 순서(`convention.md` §2) 변경 파일 전체 점검

### 3단계: 예외 케이스 및 검증 (QA_Debugger)
- [ ] 홀드 중 스크롤: `SelectSlot`/`CycleSlot` 패널·타임라인 `ScrollRect` 전달이 고스트 활성 시 차단되는지 Play Mode 검증
- [ ] `CountSlot` 탭 → 카운트 증가 / 홀드 드롭 → `CycleSlot.SetSlotCount` / 드롭 실패 시 고스트 파괴
- [ ] `ResetSlot` 홀드 드롭 → `CycleSlot.ResetSlot` / 비대상 영역 드롭 시 고스트 정리
- [ ] `SelectSlot` 탭 → 마지막 행 추가 / 고스트 드롭 → 레이아웃·플레이스홀더 인덱스 정확도
- [ ] `CycleSlot` 탭 삭제·재배치·`CancelHold` 복귀 시 `originalParent` null/파괴 레이아웃 NRE 없음
- [ ] `OnDisable`/`ClearSlot` 시 `ColorEventChannel` 리스너 해제 누락 없음 (`CycleSlot`)
- [ ] 인스펙터: `ghostPrefab`/`ghostObject`/`panelScroll` SerializeField 누락 시 `Debug.LogError` 자가 진단 (변경 클래스 각 1회)

---

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-05-18: `CycleSlot`, `SelectSlot`, `CountSlot`, `ResetSlot`을 `Slot`/`DraggableSlot` 계층으로 1차 리팩토링 완료. 본 태스크는 **OOP 검증 + 2차 정돈(중복 제거·역할 분리)** 이다.
- `TabSlot`/`BackgroundSlot`은 Slot 계열 미편입 — 별도 태스크로 분리.
