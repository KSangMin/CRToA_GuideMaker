# 🏗️ Technical Architecture Design

## 1. System Architecture Diagram

- **Pattern**: MonoBehaviour 상속 + Template Method (`OnSlot*`, `ResolveClickOrDragDropPointerUp`) + UGUI EventSystems
- **Data Flow**:
  - **패널(소스)** → 홀드/드래그 → 고스트(`CycleSlot` 또는 `GameObject`) → 레이캐스트 → **타임라인(싱크)** `CycleHorizontalLayout` / `CyclePanel`
  - **타임라인(거주)** `CycleSlot` → 홀드 시 자기 `RectTransform`을 `forGhostParent`로 이동 → 플레이스홀더로 인덱스 계산 → 드롭 시 레이아웃 재배치

```mermaid
classDiagram
    direction TB
  class Slot {
    +holdDelaySeconds
    #ResolveClickOrDragDropPointerUp()
    #WaitHoldThen()
    #InstantiateGhostUnderPanel()
    #TryBeginPanelScrollDragUnlessGhost()
  }
  class DraggableSlot {
    #TryForwardScrollOrDraggable()
    #TryDropOnCycleLayouts()
  }
  class PanelGhostSlot {
    <<planned>>
    #ProcessDrop()
    #OnPanelClick()
  }
  class CycleSlot {
    +SetSlot()
    placeholder drag
  }
  class SelectSlot {
    spawns CycleSlot ghost
  }
  class CountSlot {
    count ghost
  }
  class ResetSlot {
    reset ghost
  }
  Slot <|-- DraggableSlot
  Slot <|-- PanelGhostSlot
  DraggableSlot <|-- CycleSlot
  DraggableSlot <|-- SelectSlot
  Slot <|-- CountSlot
  Slot <|-- ResetSlot
  CountSlot ..|> PanelGhostSlot : 목표: 흡수
  ResetSlot ..|> PanelGhostSlot : 목표: 흡수
```

### 역할 분리 (도메인)

| 구분 | 클래스 | 위치 | 책임 |
|------|--------|------|------|
| **타임라인 거주 슬롯** | `CycleSlot` | `UI/Result/Cycle/` | 배치된 스킬 표시, 재정렬, 탭 삭제, 반복 카운트 UI |
| **패널 → 타임라인 공급** | `SelectSlot` | `UI/Panel/Select/` | 스킬 선택 후 `CycleSlot` 고스트 생성·드롭 |
| **패널 특수 고스트** | `CountSlot`, `ResetSlot` | `UI/Panel/Special/` | 단순 프리팹 고스트 + `CycleSlot` 태그 대상 조작 |
| **미마이그레이션** | `TabSlot`, `BackgroundSlot` | `UI/Grid/`, `UI/Panel/TabMenu/` | 별도 홀드/스크롤 구현 (향후 `Slot` 계열 편입 후보) |

## 2. Key Components & Class Responsibilities

### 현재 구현

- **`Slot.cs`**: 포인터 이벤트 파사드, 홀드 코루틴(`WaitHoldThen`), 클릭 vs 드롭 분기(`ResolveClickOrDragDropPointerUp`), 패널 `ScrollRect` 위임, 단순 고스트 생성/파괴 헬퍼.
- **`DraggableSlot.cs`**: `ScrollRect` 전달(`TryForwardScrollOrDraggable`) + `CyclePanel`/`CycleHorizontalLayout` 드롭 레이캐스트(`TryDropOnCycleLayouts`).
- **`CycleSlot.cs`**: 타임라인 내 **자기 자신**을 홀드 후 reparent하여 드래그; 플레이스홀더 인덱싱; `DraggableSlot` 스크롤 전달 재사용.
- **`SelectSlot.cs`**: 패널에서 **`CycleSlot` 프리팹** 고스트; 드롭은 `DraggableSlot.TryDropOnCycleLayouts`.
- **`CountSlot.cs` / `ResetSlot.cs`**: `Slot`만 상속; **단순 GameObject 고스트** + 패널 스크롤; 드롭 시 `CycleSlot` 태그 레이캐스트.

### OOP 검증 요약 (2026-05-18)

| 원칙 | 평가 | 근거 |
|------|------|------|
| **SRP** | ⚠️ 부분 위반 | `Slot`이 홀드·스크롤·고스트·클릭분기를 모두 보유. `DraggableSlot`이 Result 영역(`CyclePanel`) 드롭까지 담당. `CycleSlot`이 표시·드래그·플레이스홀더·폰트 색까지 혼재. |
| **OCP** | ⚠️ 부분 위반 | `CountSlot`/`ResetSlot` 추가 시 동일 보일러플레이트 복붙 필요. 새 패널 특수 슬롯도 `ProcessDrop`만 다르게 확장하기 어려움. |
| **LSP** | ⚠️ 경미 | `CycleSlot.OnSlotBeginDrag`가 `base.OnSlotBeginDrag` 대신 `TryForwardScrollOrDraggable` 직접 호출 — `DraggableSlot` 가상 체인 우회. |
| **ISP** | ⚠️ 경미 | `ResetSlot`은 탭 클릭 없음에도 전체 드래그 인터페이스 구현. |
| **DRY** | ❌ 위반 다수 | `CountSlot`≈`ResetSlot` (~90% 동일), `CycleSlot.CheckHoldAfterDelay` ≈ `Slot.WaitHoldThen`, 차징 UI 분기 `SelectSlot`/`CycleSlot` 중복, 레이캐스트 `List` 매 호출 할당. |

### 목표 상속 구조 (정돈 후)

```
Slot                          // 공통 입력·홀드·클릭/드롭 분기·스크롤/고스트 유틸만
├── PanelGhostSlot (신규)      // Count/Reset 공통 템플릿 (홀드→고스트→스크롤→드롭)
│   ├── CountSlot
│   └── ResetSlot
└── DraggableSlot             // 스크롤 전달 + (선택) 사이클 드롭 헬퍼
    ├── SelectSlot            // 패널: CycleSlot 고스트 공급
    └── CycleSlot             // 타임라인: self-reparent + placeholder
```

**추가 추출 후보 (컴포넌트, 2차):**

- `SkillSlotDisplay` — `icon`, `head`, `chargeBackground`, `ControlType` 분기 (`SelectSlot`/`CycleSlot` 공유)
- `ICycleDropTarget` / static `CycleDropRaycast` — `DraggableSlot`, `CountSlot`, `ResetSlot`의 태그 레이캐스트 통합

## 3. Dependencies & Third-Party Libraries

- Unity UGUI (`ScrollRect`, `LayoutElement`), EventSystems, TextMeshPro
- `UIManager`, `UI_Panel`, `UI_Result`, `CyclePanel`, `CycleHorizontalLayout`
- `ColorEventChannel` (ScriptableObject 이벤트)

## 4. Folder Structure (Directory Layout)

```txt
Assets/Scripts/UI/
├── Panel/
│   ├── Slot.cs                 # 베이스
│   ├── DraggableSlot.cs        # 스크롤 전달 + 사이클 드롭
│   ├── PanelGhostSlot.cs       # [신규 예정] 패널 특수 고스트 베이스
│   ├── Select/SelectSlot.cs
│   └── Special/
│       ├── CountSlot.cs
│       └── ResetSlot.cs
├── Result/Cycle/
│   ├── CycleSlot.cs
│   ├── CyclePanel.cs
│   └── CycleHorizontalLayout.cs
├── Grid/                       # TabSlot 등 — Slot 미편입
└── Panel/TabMenu/TabSlot.cs
```

## 5. 중복 코드 맵 (제거 대상)

| 중복 블록 | 위치 A | 위치 B | 통합 방향 |
|-----------|--------|--------|-----------|
| 홀드→고스트→PointerUp→Begin/Drag/End | `CountSlot` | `ResetSlot` | `PanelGhostSlot` 추상 클래스 |
| `WaitForSeconds(holdDelaySeconds)` 홀드 | `CycleSlot.CheckHoldAfterDelay` | `Slot.WaitHoldThen` | `CycleSlot` → `WaitHoldThen` 사용 |
| `SetPositionToPointer` | `CycleSlot` | `Slot.SetRectTransformToPointer` | 래퍼 제거, static 호출만 |
| Charge 배경 + 텍스트 | `SelectSlot.SetSlot` | `CycleSlot.SetSlot` | `SkillSlotDisplay` 또는 protected static helper |
| `new List<RaycastResult>()` | `CountSlot`, `ResetSlot`, `CycleSlot.CheckForPlaceHolder` | `DraggableSlot._raycastBuffer` | 공유 버퍼 또는 `Slot` protected buffer |
