# 📝 User-Todo Guide

### 🎮 유니티 에디터 세팅 가이드 (Arrow System)

- [ ] **프리팹/오브젝트 생성 및 계층 세팅 (Prefab/Object Creation)**:
  - **ArrowRenderer 프리팹 만들기**: 씬에 빈 UI 오브젝트를 생성하고 이름을 `ArrowRenderer`로 변경하세요.
  - `ArrowRenderer`의 자식으로 `Image`를 생성하여 이름을 `Line`으로 변경하세요. (화살표 선 역할)
  - `ArrowRenderer`의 자식으로 `Image`를 하나 더 생성하여 이름을 `Head`로 변경하세요. (화살표 촉 역할)
  - `ArrowRenderer`의 자식으로 `Button (TextMeshPro)`를 생성하고 이름을 `LoopCountUI`로 변경하세요. (반복 횟수 클릭 버튼)

- [ ] **컴포넌트 부착 (Component Attachment)**:
  - `UI_Result` 캔버스 내 `CyclePanel` 오브젝트 하위에 빈 오브젝트를 생성하고, `ArrowOverlayPanel.cs`를 부착하세요.
  - 위에서 만든 `ArrowRenderer` 최상단 오브젝트에 `ArrowRenderer.cs`를 부착하세요.
  - `SpecialPanel` (마커 목록 패널) 내부에 새로운 마커 슬롯 UI 2개를 추가하고, 각각 `ArrowStartSlot.cs`와 `ArrowEndSlot.cs`를 부착하세요.

- [ ] **인스펙터 참조 할당 (Inspector Assignments)**:
  - **ArrowOverlayPanel**:
    - `onLayoutRebuiltEvent` 슬롯에 기존 레이아웃 리빌드 이벤트 채널 에셋을 할당하세요.
    - `arrowRendererPrefab` 슬롯에 방금 만든 `ArrowRenderer`를 연결하세요. (연결 후 `ArrowRenderer`는 프로젝트 폴더로 드래그하여 프리팹화하고 씬에서 지우세요)
  - **ArrowRenderer (프리팹)**:
    - `lineRect`와 `lineImage` 슬롯에 자식 `Line` 오브젝트 및 Image 컴포넌트를 연결하세요.
    - `headRect`와 `headImage` 슬롯에 자식 `Head` 오브젝트 및 Image 컴포넌트를 연결하세요.
    - `loopCountButton`과 `loopCountText` 슬롯에 자식 `LoopCountUI` 버튼과 그 하위 텍스트 컴포넌트를 연결하세요.
  - **SpecialPanel**:
    - 인스펙터의 `arrowStartSlot`과 `arrowEndSlot` 필드에 방금 추가한 두 마커 슬롯 게임오브젝트를 할당하세요.

- [ ] **에디터 설정 및 설정 검증 (Editor Settings & Setup Validation)**:
  - `ArrowStartSlot` 및 `ArrowEndSlot`의 아이콘이 기존 Area 마커와 시각적으로 뚜렷하게 구별되도록 스프라이트를 설정하세요.
  - 플레이 모드 진입 후 화살표 마커 쌍을 드롭했을 때, 슬롯의 아이콘 정중앙을 잇는 선과 터치 가능한 횟수 UI가 생성되는지 검증하세요.