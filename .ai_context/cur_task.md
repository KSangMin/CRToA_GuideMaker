# 📍 Current Task: Slot 상속 구조 OOP 검증 및 코드 정돈

## 🎯 Current Goal
- 파편화된 슬롯(`CycleSlot`, `SelectSlot`, `CountSlot`, `ResetSlot`)을 **`Slot` → `DraggableSlot` / `SelfGhostSlot`** 계층으로 재정립한다.
- **역할 분리**: 타임라인 거주 슬롯 vs 패널 드래그·고스트 슬롯.
- **추상화**: 홀드·스크롤 전달·클릭/드롭 분기·고스트 좌표·레이캐스트 버퍼는 부모에만 둔다.
- **범위**: 위 4종 + 베이스 3파일. `TabSlot`/`BackgroundSlot`은 2차 태스크.

---

## 📊 현재 코드 품질 판정 (2026-05-18 실측)

| 항목 | 점수 | 요약 |
|------|------|------|
| 구조화 | ⭐⭐ (2/5) | 상속 없음. 동일 패턴 4~5회 복제. |
| OOP 준수 | ⭐⭐ (2/5) | SRP·OCP·DRY 모두 미충족. Template Method 부재. |
| 유지보수성 | ⭐⭐ (2/5) | 홀드 시간·스크롤 로직 변경 시 4파일 동시 수정 필요. |
| GC/성능 | ⭐⭐⭐ (3/5) | 레이캐스트 `List` 매 호출 할당. 나머지는 경량. |
| 도메인 분리 | ⭐⭐⭐ (3/5) | 폴더·클래스 이름으로 역할은 구분되나 코드 경계는 없음. |

**상태 (2026-05-19)**: `Slot` / `SelfGhostSlot` / `DraggableSlot` 구현 및 4종 마이그레이션 **완료**. Play Mode QA(3단계)만 남음.

### 잘 된 점
- 폴더 단위 역할(`Panel/Special`, `Panel/Select`, `Result/Cycle`)이 명확함.
- `CountSlot` vs `ResetSlot`은 이미 **드롭 한 줄**만 다름 → `SelfGhostSlot` 흡수 용이.
- `SelectSlot` ↔ `CycleSlot` 협력 API(`Drag`, `GetPlaceHolderIndex`, `ClearPlaceHolder`)는 응집되어 있음.

### 반드시 고칠 점
1. **베이스 부재** — 4클래스가 각각 `IPointerDown/Up`, `IBeginDrag`, `IDrag`, `IEndDrag` 구현.
2. **CountSlot ≈ ResetSlot** (~130줄 중 ~115줄 동일).
3. **SelectSlot.ProcessDrop ≈ CycleSlot.ProcessDrop** — `CycleHorizontalLayout` / `CyclePanel` 태그 분기 중복.
4. **홀드 코루틴 4벌** — `_holdTime`, `_isCanceled`, `CancelHold` 동일.
5. **포인터→RectTransform** — `SetGhostPositionToPointer` / `SetPositionToPointer` 동일 알고리즘.
6. **GC** — `new List<RaycastResult>()` 드롭/플레이스홀더마다 할당.
7. **CycleSlot.CancelHold** — `OnEndDrag`에서도 호출되어 드래그 종료 시 부모 복귀 버그 가능성 (QA 확인).

---

## 🗺️ 목표 클래스 다이어그램

```
Slot (abstract, IPointer*)
├── SelfGhostSlot (abstract)
│   ├── CountSlot      → OnSelfGhostClick: UpCount / ProcessDrop: SetSlotCount
│   └── ResetSlot      → ProcessDrop: ResetSlot only
└── DraggableSlot (abstract)
    ├── SelectSlot     → CycleSlot 고스트 생성·탭 시 AddSlotToLast
    └── CycleSlot      → self-reparent, placeholder, 탭 삭제
```

### 부모(`Slot`)가 가져갈 책임
- `[SerializeField] holdDelaySeconds` (기본 0.15f)
- `WaitHoldThen(eventData, Action onElapsed)`
- `CancelHold()`, `ResolveClickOrDragDropPointerUp` (클릭 vs 고스트 드롭)
- `TryBeginPanelScrollDragUnlessGhost` / `ForwardPanelScrollDrag`
- `InstantiateGhostUnderPanel`, `SetRectTransformToPointer`, `DestroyGhost`
- `protected static List<RaycastResult> RaycastBuffer` (또는 인스턴스 버퍼)

### `DraggableSlot` 추가 책임
- `TryDropOnCycleLayouts(CycleSlot slot, int targetIndex)` — `SelectSlot`/`CycleSlot` 공용
- 가상 `OnSlotBeginDrag` → 스크롤 전달 (고스트 없을 때)

### `SelfGhostSlot` 추가 책임
- `GameObject` 단순 고스트 파이프라인 전체
- `protected abstract bool ProcessDrop(PointerEventData)`
- `protected virtual void OnSelfGhostClick(PointerEventData)` — `CountSlot`만 override

### 자식에 남길 것만
| 클래스 | 유지 코드 |
|--------|-----------|
| `CountSlot` | `_curCount`, `UpCount`, `SetCountText`, `ProcessDrop` 1곳 |
| `ResetSlot` | `ProcessDrop` 1곳 |
| `SelectSlot` | `SetSlot`(스킬 데이터), `CreateSlot`, 탭 시 `AddSlotToLast` |
| `CycleSlot` | placeholder, reparent, `SetSlotCount`/`ResetSlot`, 폰트 색 이벤트 |

---

## 📝 Todo List

### 0단계: 베이스 계층 신규 (Builder — 선행 필수)
- [x] `Assets/Scripts/UI/Panel/Slot.cs` — `MonoBehaviour` + 5개 포인터 인터페이스, Template Method (`OnSlotPointerDown` 등 protected virtual)
- [x] `Slot`: `WaitHoldThen`, `ResolveClickOrDragDropPointerUp`, 스크롤/고스트 헬퍼, `RaycastBuffer`
- [x] `Assets/Scripts/UI/Panel/DraggableSlot.cs` — `TryDropOnCycleLayouts`, 스크롤 전달 가상 메서드
- [x] `Assets/Scripts/UI/Panel/SelfGhostSlot.cs` — Count/Reset 공통 파이프라인 + `ProcessDrop` / `OnSelfGhostClick` 추상·가상

### 1단계: 설계 문서 (Planner) — 본 응답에서 반영
- [x] 실측 OOP 검증 및 현재/목표 다이어그램 (`architecture.md` 동기화)
- [x] 중복 맵 및 통합 방향 (`architecture.md` §5)
- [ ] (선택) `SkillSlotDisplay` 분리 — `SelectSlot`/`CycleSlot` SerializeField 목록 확정 후 2차

### 2단계: 파생 슬롯 마이그레이션 (Builder)

#### 2-A. 패널 특수 (`SelfGhostSlot`)
- [x] `CountSlot` → `SelfGhostSlot` 상속, 도메인 로직만 잔류
- [x] `ResetSlot` → `SelfGhostSlot` 상속

#### 2-B. 패널 선택 (`SelectSlot`)
- [x] `DraggableSlot` 상속, 홀드/스크롤/드롭 제거
- [x] `CreateSlot` / 탭 클릭 / `CycleSlot` 고스트 위임만 유지
- [x] `SetPositionToPointer` → `SetRectTransformToPointer` 통일

#### 2-C. 타임라인 (`CycleSlot`)
- [x] `DraggableSlot` 상속
- [x] `CheckHoldAfterDelay` → `WaitHoldThen`
- [x] `OnSlotBeginDrag`에서 `base` 호출로 가상 체인 복원
- [x] `CheckForPlaceHolder` — `RaycastBuffer` 사용
- [x] `originalParent` — `[HideInInspector] public` 유지 (`CycleHorizontalLayout` 외부 할당)

#### 2-D. 컨벤션 (`convention.md`)
- [x] 변경 파일: `#region`, Allman brace, SerializeField camelCase, `_` private
- [x] `holdDelaySeconds` SerializeField로 통일 (매직 넘버 `_holdTime` 제거)

### 3단계: QA (QA_Debugger)
- [x] 홀드 중 패널 `ScrollRect` 전달 (고스트 없을 때만) - CycleSlot 헬퍼 메서드 위임 완료
- [ ] `CountSlot` 탭 증가 / 홀드 드롭 `SetSlotCount` / 실패 시 고스트 파괴
- [ ] `ResetSlot` 홀드 드롭 `ResetSlot`
- [ ] `SelectSlot` 탭·드롭·플레이스홀더 인덱스
- [ ] `CycleSlot` 탭 삭제·재배치·`CancelHold` NRE
- [ ] `OnDisable`/`ClearSlot` — `ColorEventChannel` 해제
- [ ] 인스펙터 누락 시 `Debug.LogError` 자가 진단

---

## 💬 User Feedback & Requests (Cursor 지시용)
**[줄 단위 피드백 작성법]**
특정 항목에 대한 피드백은 해당 줄 바로 아래에 마크다운 인용구(`>`) 기호만 사용하여 달아주세요.
(예시)
- [ ] `CountSlot` → `PanelGhostSlot` 상속, 도메인 로직만 잔류
> 이렇게 꺾쇠 기호만 사용해도 Cursor가 유저의 추가 코멘트로 완벽하게 파악합니다.

**[전체 피드백]**
전체적인 구조 변경이나 공통 요구사항이 있다면 아래에 적어주세요.
- 

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-05-18: **실측** — `Slot`/`DraggableSlot` 미구현 상태에서 설계.
- 2026-05-19: **구현 완료** — `SelfGhostSlot` 명칭으로 베이스 3종 + 4종 마이그레이션. QA(3단계) 대기.
- 2026-05-19: **QA 진행** — `CycleSlot` 스크롤 전달 오류(빈 베이스 메서드 호출) 발견 및 수정. `Slot`의 패널 스크롤 헬퍼(`TryBeginPanelScrollDrag` 등)로 완벽히 위임 완수.
- `TabSlot`/`BackgroundSlot`: Slot 계열 미편입 — 별도 태스크.
