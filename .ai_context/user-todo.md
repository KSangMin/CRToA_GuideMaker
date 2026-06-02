### 🎮 유니티 에디터 세팅 가이드

- [ ] **프리팹/오브젝트 생성 및 계층 세팅 (Prefab/Object Creation)**:
  - 기존 사이클 렌더링 영역인 `Scroll View/Viewport/Content` 하위에, 가장 첫 번째 자식으로 빈 게임오브젝트를 생성하고 이름을 `TitleWrapper`로 지어주세요.
  - 이 `TitleWrapper` 하위에 `TMP_InputField`(타이틀용 UI)를 생성하여 배치하세요.
  - `Content` 오브젝트에는 이미 `VerticalLayoutGroup`과 `ContentSizeFitter`가 있으므로, 타이틀과 생성되는 슬롯 행들이 자연스럽게 위아래로 쌓이게 됩니다.
  
  - ⚠️ **[중요] 래퍼(Wrapper) 세팅 (무한 늘어남 방지)**:
    - `TitleWrapper` 오브젝트에 **`Layout Element`** 컴포넌트를 추가하고, `Min Width`에 최소값(예: 500), `Preferred Width`에 **0**, `Flexible Width`에 **1**을 입력하세요.
    - `TitleWrapper` 오브젝트에 **`Vertical Layout Group`** 컴포넌트를 추가하세요. (Padding 0, Spacing 0 세팅).
      - **Control Child Size**: `Width`, `Height` 둘 다 체크!
      - **Child Force Expand**: `Width`, `Height` 둘 다 체크!
    - `TitleWrapper` 오브젝트에 **`Content Size Fitter`** 컴포넌트를 추가하고 Vertical Fit을 `Preferred Size`로 설정하세요. (Horizontal은 Unconstrained)
    - 부모인 `Content`의 `Vertical Layout Group`에서 **Control Child Size - Width**와 **Child Force Expand - Width**를 둘 다 체크하세요!

  - ⚠️ **[중요] InputField 세팅 (세로 0 방지 및 자동 늘어남)**:
    - `TMP_InputField` 본체에 **`Layout Element`** 컴포넌트를 추가하고, **`Min Height`**를 체크한 뒤 기본 1줄 높이(예: **50**)를 입력하세요. (이제 스크립트가 텍스트 줄바꿈에 맞춰 이 컴포넌트의 높이를 자동으로 갱신합니다!)
    - `TMP_InputField` 본체에는 `Content Size Fitter`를 **넣지 마세요**! (스크립트가 Layout Element를 조작하므로 불필요합니다).
    - `TMP_InputField`와 하위 `Text Area`, `Text (TMP)` 3개 모두의 `RectTransform`에서 **Pivot Y 값을 1**로 설정하세요.
    - `TMP_InputField` 하위의 `Text (TMP)` 컴포넌트에서 정렬(Alignment)을 상단(Top)으로 지정하세요.
    - `TMP_InputField`의 `Line Type`을 `Multi Line Newline`으로 설정하세요.

- [ ] **컴포넌트 부착 (Component Attachment)**:
  - (해당 없음) 신규 스크립트는 없으며 기존 `CyclePanel.cs`와 `OptionPanel.cs`가 그대로 유지됩니다.

- [ ] **인스펙터 참조 할당 (Inspector Assignments)**:
  - `CyclePanel` 인스펙터의 **`titleInput`** 필드 슬롯에 추가한 `TMP_InputField` 오브젝트를 할당하세요.
  - `CyclePanel` 인스펙터의 **`onFontColorChanged`** 필드 슬롯에 기존 사용 중인 글꼴 색상 이벤트 채널(`OnFontColorChanged` 에셋)을 할당하세요.
  - 옵션 패널 쪽에 타이틀을 켜고 끌 수 있는 `Toggle` UI 오브젝트를 새로 만든 뒤, `OptionPanel` 컴포넌트의 **`titleToggle`** 필드 슬롯에 할당하세요.

- [ ] **에디터 설정 및 설정 검증 (Editor Settings & Setup Validation)**:
  - 에디터를 플레이(Play)한 뒤 씬 진입 시 `CyclePanel`과 `OptionPanel` 관련 NullReferenceException 로그가 뜨지 않는지 확인하세요.
  - `OptionPanel`의 타이틀 표시 토글을 누를 때마다 타이틀 텍스트박스 영역이 켜지고 꺼지며 세로 높이(ContentSizeFitter)가 자동 반응하는지 검증하세요.
