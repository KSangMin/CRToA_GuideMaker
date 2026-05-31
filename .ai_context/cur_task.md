# 📍 Current Task: 사이클 반복 및 연계 화살표 시스템 구현

## 🎯 Current Goal
- 딜 사이클 내 특정 스킬 슬롯에서 다른 스킬 슬롯으로 연결되는 화살표 컴포넌트를 동적으로 생성한다.
- 화살표의 시작 마커(`ArrowStartSlot`)와 끝 마커(`ArrowEndSlot`)를 배치하여 짝을 맞추고, `ArrowOverlayPanel`을 통해 베지어 곡선 또는 직교 형태의 연결선(Arrow)을 시각적으로 렌더링한다.

---

## 🌲 Proposed Hierarchy (씬 하이어라키 구조)

```txt
Canvas (UI Root)
└── CyclePanel (캡처 대상 RectTransform)
    └── Scroll View (또는 Content 래퍼)
        ├── CycleVerticalLayout (Vertical Layout Group - 물리 정렬)
        │   ├── CycleHorizontalLayout (Row 1)
        │   │   ├── CycleSlot (스킬 슬롯 1 - IsArrowStart)
        │   │   └── CycleSlot (스킬 슬롯 2 - IsArrowEnd)
        │   └── ...
        ├── AreaOverlayPanel
        └── ArrowOverlayPanel (화살표 오버레이 패널 - Area와 분리/병행)
            ├── ArrowRenderer (화살표 1 인스턴스 - ArrowId 바인딩)
            │   ├── LineRenderer (또는 UGUI 기반 자체 메쉬 생성, Line/Curve UI)
            │   ├── ArrowHead (화살표 머리 이미지)
            │   └── LoopCountUI (클릭 증감 UI - 클릭 시 반복 횟수 증가)
            └── ArrowRenderer (화살표 2 인스턴스)
```

---

## 📝 Todo List

### 1. 설계 및 기획 (Planner - 현재 단계 완료)
- [x] 화살표 연결선 구조 설계 (시작 슬롯, 끝 슬롯 마킹을 위한 `ArrowStartSlot`, `ArrowEndSlot`)
- [x] 화살표 반복 횟수 입력 방식 설계 (모바일 UX 및 렌더링 최적화를 위해 클릭 증감 방식 채택)
- [x] 화살표 렌더링 및 앵커 설계 (아이콘 정중앙을 관통하는 단순 직선(Straight Line) 방식 채택)
- [x] 줄바꿈 및 불규칙 레이아웃에서의 화살표 처리 방안 설계 (`ArrowOverlayPanel`)
- [x] 역전(][) 및 중첩 연결 검증 시 짝 매칭 로직 설계 (Area 매칭 로직 재사용)
- [x] 엣지 케이스 처리 설계: Self-Loop(시작=끝 동일 슬롯) 불허 및 동일 구간 중복 화살표 드롭 시 덮어쓰기/무시 로직 적용
- [x] 다중 화살표 시각적 구분을 위한 랜덤 색상 부여(Area 방식 차용) 설계
- [x] `.ai_context/architecture.md` 및 `cur_task.md` 최신화 완료

### 2. 핵심 로직 구현 (Builder)
- [x] `Assets/Scripts/UI/Panel/Special/ArrowStartSlot.cs` 및 `ArrowEndSlot.cs` 구현 (`SelfGhostSlot` 상속)
- [x] `Assets/Scripts/UI/Result/Cycle/CycleSlot.cs`에 `IsArrowStart`, `IsArrowEnd`, `ArrowId` 필드 및 화살표 상태 리셋 메서드 추가
- [x] `Assets/Scripts/UI/Result/Cycle/ArrowOverlayPanel.cs` 생성. `OnLayoutRebuilt` 이벤트 수신 후 전체 `CycleSlot` DFS 스캔하여 `ArrowId` 쌍 묶기 및 좌표 계산 로직 구현
- [x] `ArrowOverlayPanel.cs` 무결성 검증 추가: 시작과 끝이 같은 슬롯인 경우(Self-Loop) 즉시 파괴 및 동일 구간 중복 쌍 필터링 로직 구현
- [x] `Assets/Scripts/UI/Result/Cycle/ArrowRenderer.cs` 생성. 시작 슬롯과 끝 슬롯의 정중앙(Center)을 최단 거리로 잇는 단순 직선(Straight Line)과 화살표 촉을 그리는 기능 구현
- [x] `ArrowRenderer` 상단에 클릭 가능한 `LoopCountUI`를 배치하고, 터치 시 화살표 반복 횟수가 순환 증가(예: 1~9)하는 로직 구현
- [x] `ArrowOverlayPanel`에서 고유 `ArrowId` 단위로 랜덤 파스텔 색상을 생성/캐싱하고, 이를 `ArrowRenderer`의 선, 화살표 촉, 반복 횟수 텍스트(UI) 색상에 일괄 적용하는 로직 구현
- [x] `SpecialPanel.cs`에 신규 화살표 마커 슬롯 프리팹 연동
- [x] (Ad-hoc) `ArrowRenderer` UI 커스터마이징 및 시각적 보완 기능 추가
  - [x] 프리팹 인스펙터에 `headSize` 및 `countFontSize` 속성을 노출시켜 크기 조절 기능 추가
  - [x] 텍스트와 선이 겹치지 않도록 직교 벡터를 활용한 `textOffset` 띄움(Offset) 로직 추가
  - [x] 화살표 방향(좌/우)과 무관하게 반복 횟수 텍스트가 항상 선형의 상단(Upward)에 배치되도록 수직 방향 벡터 보정 추가
- [x] (Ad-hoc) `ArrowRenderer`의 위치 갱신 로직을 `Init` 시점에서 `LateUpdate` 기반으로 변경하여 레이아웃 추가/변경(Area 등) 시에도 실시간으로 위치가 동기화되도록 버그 수정
- [x] (Ad-hoc) `ArrowRenderer`의 양끝 연결 기준 좌표를 슬롯 전체 영역(`CycleSlot`)에서 아이콘 고유 영역(`IconRect`)으로 변경하여, 주석(Comment) 추가로 슬롯 크기가 확장되더라도 화살표 렌더링 위치가 어긋나지 않도록 고정
- [x] (Ad-hoc) `ArrowRenderer`의 `LateUpdate` 갱신 로직에 이전 프레임 좌표 캐싱 및 비교(Dirty Check)를 추가하여, 위치가 실제로 변경되었을 때만 UI 렌더링 연산을 수행하도록 성능 최적화
- [x] (Ad-hoc) `ArrowRenderer`의 반복 횟수 버튼 위치 오프셋을 수동(`textOffset`)이 아닌, `선 두께(lineWidth)`와 `버튼 크기(countSize)`에 비례하여 자동 계산되도록 동적 로직으로 변경
- [x] (Ad-hoc) `ArrowOverlayPanel`에서 부모 컨테이너(`CycleVerticalLayout`)의 상하좌우 패딩에 안전 여백(Safe Padding)을 강제 주입하여 마스크 잘림 현상 해결
- [x] (Ad-hoc) 무조건 사방에 여백을 추가하던 기존 휴리스틱 로직을 폐기하고, 렌더링될 버튼과 화살촉의 실제 좌표(Corner Position)를 컨테이너 영역과 물리적으로 비교하여 **초과 돌출된 특정 방향에만 튀어나온 픽셀만큼 정확히** 여백을 부여하는 `ApplyExactPadding` 알고리즘으로 고도화. (Area와의 이중 패딩 겹침 문제도 로컬 좌표계 연산을 통해 자연스럽게 소거됨)
- [x] (Ad-hoc) `ApplyExactPadding` 연산 시, 항상 슬롯 내부에 안전하게 렌더링되는 선형 및 화살촉(`head`)의 연산을 제외하고, 실제로 튀어나갈 수 있는 `countText` 버튼의 모서리 영역만 검사하도록 로직 최적화
- [x] (Ad-hoc) `ApplyExactPadding`에서 화살표 좌표를 중복으로 시뮬레이션하던 연산을 모조리 소거하고, 생성된 `countText` 오브젝트의 실제 `RectTransform.position`을 직접 참조하는 방식으로 압축 리팩토링
- [x] (Ad-hoc) 다중 화살표 생성 시 부모 레이아웃 그룹(`VerticalLayoutGroup`)의 자동 정렬로 인해 발생하는 1프레임 좌표 오차 버그를 해결하기 위해, 여백 계산 직전에 모든 화살표의 내부 좌표를 재동기화하는 `ForceUpdatePositions` 로직 추가
- [x] (Ad-hoc) 마커(Area/Arrow) 드롭 시 전체 레이아웃 리빌드 이벤트(`onLayoutRebuiltEvent`)가 전파되지 않아 상대방 패널이 레이아웃 갱신을 감지하지 못하고 이전 패딩을 덮어씌워 여백이 중복되던 이벤트 누락 버그 해결

### 3. 방어적 검증 및 예외 처리 (QA_Debugger)
- [x] UI 레이아웃 갱신 3대 지연 버그 방지(1프레임 지연, ScrollRect LateUpdate, 초기 행렬 미동기화) 검증 (-> `memory.md` 참조)
- [x] 슬롯 이동, 삭제, 줄바꿈 발생 시 화살표 선형이 꼬이거나 다른 슬롯을 덮어버리지 않는지 테스트
- [x] 화살표(Curve)가 캡처 영역(`CyclePanel` 범위) 밖으로 삐져나가지 않는지 검증
- [x] 화살표 마커 삭제(`ResetSlot`) 시 연쇄 리셋 정상 동작 테스트
- [x] Self-Loop 발생 시도 및 완전히 겹치는 중복 마커 드롭 시 앱이 멈추지 않고 즉시 파괴(Auto-Delete) 처리되는지 검증

---

## 💬 User Feedback & Requests
- 2026-05-29 18:19: 화살표 반복 횟수 입력 방식을 `InputField` 대신 성능과 모바일 UX에 유리한 클릭(터치) 증감 방식으로 결정 및 반영 완료.
- 2026-05-29 18:29: 다중 화살표 구분을 위해 Area 컴포넌트와 동일하게 무작위 색상을 선과 반복 횟수 텍스트에 적용하는 기능 요구 반영.
- 2026-05-29 19:36: 인터뷰를 통해 렌더링 스타일(중앙 관통 직선), Self-Loop 불허, 중복 마커 방지 등 엣지 케이스 처리 방침 확정 및 반영 완료.
- 2026-05-29 21:51: ArrowRenderer 인스펙터에서 headSize, countFontSize 크기를 직접 조절할 수 있도록 변수 노출 요구 반영.
- 2026-05-29 22:00: 화살표 각도에 따라 텍스트가 선을 가리는 문제 제보. textOffset 속성 및 노멀 벡터 로직 적용하여 해결.
- 2026-05-29 22:15: 화살표가 좌측을 향할 때 텍스트가 하단에 위치하는 문제 제보. 노멀 벡터 Y축 양수 보정(항상 상단 고정) 로직 적용 완료.
- 2026-05-29 22:23: 영역(Area) 추가/변경 등 레이아웃 갱신 시 화살표 위치가 어긋나는 버그 제보. ArrowRenderer가 LateUpdate에서 매 프레임 위치를 갱신하도록 수정하여 해결.
- 2026-05-29 22:28: CommentSlot(주석) 추가 시 슬롯 자체의 크기가 세로로 확장되며 화살표 렌더링 위치가 아래로 처지는 버그 제보. 연결 기준점을 아이콘(IconRect)의 중앙으로 변경하여 고정 완료.
- 2026-05-29 22:34: LateUpdate 매 프레임 연산에 대한 성능 저하 우려 피드백. Dirty Check(이전 프레임 좌표 캐싱 비교) 로직을 추가하여 실질적 위치 변동 시에만 UI 컴포넌트 갱신을 수행하도록 최적화.
- 2026-05-29 22:52: 반복 횟수 버튼(loopCountButton)이 화살표 선에서 너무 멀리 렌더링된다는 시각적 피드백 수렴. 오프셋 거리를 제어하는 textOffset(기본값 15f) 속성을 인스펙터에 분리 노출하여 해결.
- 2026-05-29 22:54: countSize 변경 시 수동으로 textOffset을 맞춰야 하는 번거로움 제보. 수동 변수 대신 선 두께와 버튼 크기를 기반으로 오프셋을 자동 연산하는 로직 적용 요구.
- 2026-05-29 23:02: 화살표의 반복 횟수 텍스트가 스크롤 뷰 범위를 벗어나 마스크(스크린샷 포함)에 잘리는 현상 제보. 부모(CycleVerticalLayout)의 여백(Padding)을 동적 확보하도록 적용.
- 2026-05-29 23:42: 화살표 유무에 따라 사방에 무조건 여백이 생기는 어색함 제보. 버튼(countText)과 컨테이너 모서리(Corner Position)의 물리적 로컬 좌표를 직접 비교 계산하여, 정확히 밖으로 튀어나간 특정 방향에만 최소한의 여백을 더하는 Exact Bounds 알고리즘으로 전면 리팩토링 및 고도화.
- 2026-05-29 23:46: 화살표 본체와 화살촉은 슬롯 중앙에 렌더링되므로 오버플로우가 발생할 일이 없다는 논리적 통찰 제보. 돌출 검사에서 화살촉을 제외하고 밖으로 튀어나가는 countText 버튼만 검사하도록 연산 최적화 적용.
- 2026-05-31 21:46: 화살표 생성 후 새로운 Area를 만들면 여백이 비정상적으로 커지는 버그 제보. 다중 오브젝트 생성 과정에서 레이아웃 그룹 갱신 시 발생하는 1프레임 좌표 밀림 현상을 발견. 여백 산출 직전에 ForceUpdatePositions를 호출하여 좌표 동기화 보장 적용 완료.
- 2026-05-31 21:59: 마커 생성 후 행(Row) 패딩이 변경되었음에도 부모 컨테이너(Vert) 패딩이 즉각 재조정되지 않아 여백이 중복되는 현상 제보. 이벤트 추적 결과, 마커 드롭 시 각 패널이 이벤트 전파(`RaiseEvent`)를 강제로 막고(`false`) 자신만 독립 갱신하고 있던 구조적 결함을 발견하여 정상 전파되도록 핫픽스 적용.

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-05-29 22:23: 레이아웃 갱신 대응을 위해 화살표 위치 갱신 로직을 `LateUpdate`로 이전 완료.
