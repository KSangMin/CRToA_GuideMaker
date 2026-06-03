### 🎮 유니티 에디터 세팅 가이드

- [ ] **프리팹/오브젝트 생성 및 계층 세팅 (Prefab/Object Creation)**:
  - 기존 `CountSlot` 프리팹을 복제하여 `ChargeCountSlot` 프리팹을 새로 생성하세요.
  - `CycleSlot` 프리팹 하위에 빈 오브젝트를 생성하고 이름을 `ChargeCountBackground`로 변경하세요.
  - `ChargeCountBackground` 자식으로 텍스트 UI를 추가하여 이름을 `ChargeCountText`로 변경하여 계층 구조를 조립하세요.

- [ ] **컴포넌트 부착 (Component Attachment)**:
  - 프로젝트 창에서 `ChargeCountSlot.cs`를 새로 생성한 `ChargeCountSlot` 프리팹에 부착하세요 (기존 `CountSlot` 스크립트는 제거).
  - (해당 없음) 유니티 기본 컴포넌트 설정 생략.

- [ ] **인스펙터 참조 할당 (Inspector Assignments)**:
  - 텍스트 UI 에셋을 드래그 앤 드롭하여 `ChargeCountSlot` 프리팹의 **`_countText`**(또는 `countText`) 슬롯에 할당하세요.
  - `CycleSlot` 프리팹 인스펙터의 **`_chargeCountBackground`**(또는 `chargeCountBackground`) 슬롯에 `ChargeCountBackground` 오브젝트를 연결하세요.
  - `CycleSlot` 프리팹 인스펙터의 **`_chargeCountText`**(또는 `chargeCountText`) 슬롯에 `ChargeCountText` 오브젝트를 연결하세요.
  - `CountSlot` 및 `ChargeCountSlot`의 **`_minCount`**, **`_maxCount`** 슬롯에 원하는 수치(예: 카운트는 2~9, 차지는 1~3 등)를 세팅하세요.

- [ ] **에디터 설정 및 설정 검증 (Editor Settings & Setup Validation)**:
  - (해당 없음) 태그 및 레이블 설정 확인 불필요.
  - 에디터를 플레이(Play)한 뒤 씬 진입 시 `CycleSlot`, `CountSlot`, `ChargeCountSlot` 관련 NullReferenceException 로그가 뜨지 않는지 확인하세요.
  - 런타임에 드래그 앤 드롭 테스트를 진행하여 CycleSlot에 각각의 UI가 갱신되는지 최종 수치들을 검증하세요.
