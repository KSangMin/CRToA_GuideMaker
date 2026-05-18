# 📐 Unity C# Code Convention

이 문서는 프로젝트의 모든 C# 스크립트에 적용되는 코드 스타일 및 명명 규칙을 정의합니다. 모든 코드는 이 가이드라인과 100% 일치해야 합니다.

---

## 1. 명명 규칙 (Naming Conventions)
- **클래스, 구조체, 인터페이스, 메서드**: 
  - PascalCase를 사용한다. (예: `InventoryController`, `SpawnStage()`)
  - 인터페이스는 접두사 `I`를 붙인다. (예: `IDamageable`)
- **변수 및 상수**:
  - **프라이빗 멤버 변수 (Private Fields)**: camelCase에 언더바(`_`) 접두사를 사용한다. (예: `_moveSpeed`, `_isInitialized`)
  - **인스펙터 노출 변수 (`[SerializeField] private`)**: 인스펙터에 노출하는 private 변수는 언더바(`_`)를 붙이지 않고 **순수 카멜케이스(camelCase)**만 사용한다. (예: `slotPrefab`, `submitButton`)
  - **상수 (Constants)**: PascalCase를 사용한다. (예: `MaxItems`)
  - **정적 변수 (Statics)**: PascalCase를 사용한다. (예: `Instance`)
- **퍼블릭 프로퍼티 (Public Properties)**: 
  - PascalCase를 사용한다. (예: `public float MoveSpeed => _moveSpeed;`)
- **지역 변수 및 매개변수 (Local Variables / Parameters)**: 
  - 일반 camelCase를 사용한다. (예: `targetPosition`, `deltaTime`)

## 2. 변수 및 속성 선언 순서 (Declaration Order) & 구조화
클래스 내부 구성 요소는 반드시 파일 상단부터 아래 순서대로 배정하며, `#region`을 활용해 그룹화하라:

1. **Constants & Statics** (`#region Constants` / `#region Statics`)
2. **`[SerializeField]` 인스펙터 노출 변수들** (`#region Serialized Fields`)
3. **일반 `private` / `protected` 멤버 변수들** (`#region Private Fields`)
4. **`public` 프로퍼티 및 이벤트/델리게이트/액션** (`#region Public Properties`)
5. **유니티 생명주기 내장 메서드** (`#region Unity Lifecycle` - `Awake()`, `Start()` 등)
6. **`public` 메서드** (`#region Public Methods`)
7. **`private` / `protected` 내부 구현 메서드** (`#region Private Methods`)

## 3. 스타일 가이드 (Style Guide)
- **접근 제한자 명시**: `private`, `public`, `protected` 등 모든 접근 제한자는 생략하지 말고 명시적으로 선언부 처음에 적어라.
- **중괄호 및 줄 바꿈 (Brace Styles & Layout)**: 클래스, 메서드 선언문뿐만 아니라 `if`, `for`, `foreach`, `while`, `switch` 등의 제어문 구동 시 **중괄호(`{}`)는 항상 새로운 줄(Next Line)에서 시작(Allman Style)**해야 한다. 중괄호 직전의 강제 개행을 엄격히 준수하라.
  ```csharp
  // 올바른 예시
  if (isReady)
  {
      ExecuteTask();
  }
  ```
- **축약형 표현**: 게터(Getter)만 존재하는 단선 프로퍼티는 화살표 표현식(=>)을 적극 활용하라.
- **UI 컴포넌트 명명**: 직관성을 위해 UGUI/UI Toolkit 컴포넌트 변수명 뒤에는 항상 컴포넌트 타입을 명시하라. 또한 반드시 TextMeshPro(TMP_Text 등)를 사용하고 레거시 Text 컴포넌트는 금지한다.
  - **인스펙터 노출 변수 예시**: submitButton, titleText, playerRigidbody
  - **내부 private 변수 예시**: _submitButton, _titleText, _playerRigidbody
- **에디터 전용 코드**: 빌드 시 에러를 방지하기 위해 에디터 전용 로직은 반드시 #if UNITY_EDITOR 전처리기로 감싸야 한다.