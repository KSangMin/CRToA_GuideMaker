#### 1. Objective (목표)
- 사이클 에디터 최상단에 타이틀 입력 UI(`TMP_InputField`)를 추가하고 이미지 캡처 시 타이틀 영역이 함께 출력되도록 계층 구조 개편

#### 2. Implementation Design (구현 설계 / 아키텍처 변경점)
- 변경/추가될 속성: `CyclePanel` 클래스에 캡처 대상을 제어하기 위한 `captureTarget` (`RectTransform`), 사이클 타이틀을 받아올 `titleInput` (`TMP_InputField`) 등 레퍼런스 필드 추가.
- 아키텍처 변경점: `architecture.md`에 기재된 대로 `CyclePanel`이 사이클 이름과 슬롯 컨테이너를 모두 포함하는 최상위 영역 캡처를 관리.
- 데이터 흐름: 기존에는 `CyclePanel`에서 `content` (ScrollRect의 Content)를 캡처 대상으로 지정했으나, 이를 타이틀과 슬롯 컨테이너(`SlotContainer`)를 모두 감싸는 부모 컨테이너(`captureTarget`)로 변경하여 타이틀 UI까지 캡처 영역에 포함시킴.

#### 3. Tasks (작업 리스트)

##### 1단계: 설계 및 데이터 구조화 (Planner)
- [x] `.ai_context/architecture.md` 최신화 및 구조 정의
- [x] 확장성과 유연성을 위한 캡처 대상 계층 구조(Hierarchy) 설계 완료
- [x] 타이틀 영역이 캡처 영역에 포함되도록 레이아웃 컨테이너 구조 분리 결정

##### 2단계: 핵심 로직 구현 (Builder)
- [x] `Assets/Scripts/UI/Result/Cycle/CyclePanel.cs` `titleInput` 필드 추가 및 캡처 타겟 유지 (`Content` 구조 활용)
- [x] 캡처 로직 간소화: 별도의 `captureTarget` 계층 분리 없이 기존 `content` 하위에 `InputField`를 직속으로 두어 캡처 영역 안에 자연스럽게 포함시킴
- [x] `CyclePanel` 필드 연동 및 런타임 Null-Check 방어적 검증 코드 작성
- [x] `Assets/Scripts/UI/Result/Option/OptionPanel.cs` 수정: 타이틀 UI 온오프 제어를 위한 `Toggle`(또는 `Button`) 기능 연동 및 `CyclePanel.titleInput` 활성화/비활성화 처리 로직 구현
##### 3단계: 방어적 검증 및 예외 처리 (QA_Debugger)
- [x] 타이틀 영역 내 엔터(줄바꿈) 입력 및 다중 행 텍스트 붙여넣기 허용 시, `ContentSizeFitter`에 의해 세로 높이가 정상적으로 자동 확장되는지 검증
- [x] 고의적인 텍스처 한계 초과 방지를 위한 적절한 최대 글자 수(Max Length, 100~200자 내외) 제한 로직 정상 작동 검증 (줄 수 제한 없이 Max Length로만 통제)
- [x] 타이틀을 비워둘(Empty) 경우 캡처본에 불필요한 상단 여백이 남지 않도록 해당 UI 레이아웃이 비활성화(숨김)되는지 테스트
- [x] 다운로드된 이미지(PNG) 최상단에 타이틀이 깨짐 없이 렌더링되는지 확인 (PC 및 WebGL 해상도 왜곡 테스트)
- [x] 협업 및 인계 목적의 유니티 에디터 세팅 가이드라인(`user-todo.md` 등) 작성: ScrollRect Content 하위에 `Title InputField`와 `SlotContainer`를 위아래로 배치하는 VerticalLayoutGroup 구성법 안내

---

## 💬 User Feedback & Requests
- 2026-06-01 13:31: OptionPanel에 사이클 이름 UI 온오프 토글 버튼 추가 요구

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-06-01 13:19: planner.md 및 builder.md 템플릿 변경에 따른 cur_task.md 양식 전면 재작성
- 2026-06-01 13:33: (grill-me) 타이틀란 비어있을 시 자동 숨김 처리, 엔터 차단 및 가로폭 제한 기반의 자동 Wrap 적용, Wrap 시 세로 높이 유연 확장(ContentSizeFitter) 결정 반영
- 2026-06-01 13:45: (grill-me) 붙여넣기 시 엔터->공백 강제 치환 및 Max Length(100~200자) 제한 도입 결정 반영
- 2026-06-01 13:47: (grill-me 번복) 엔터(줄바꿈) 입력 허용, 다중 행 붙여넣기 허용으로 정책 변경. 세로 길이 무한 확장은 Max Length(글자 수 제한)만으로 통제하기로 확정.
- 2026-06-01 15:23: QA_Debugger 검증 완료, user-todo.md 에디터 조립 가이드 생성 및 태스크 종료.
- 2026-06-01 15:52: (유저 피드백) 복잡한 `captureTarget` 계층 분리를 취소하고, 기존 `Content` 하위에 직속으로 타이틀 UI를 추가하는 간소화 방향으로 구조 개편. 코드 및 가이드라인 롤백/수정 완료.

## 🌲 Proposed Hierarchy
```text
▼ Scroll View
  ▼ Viewport
    ▼ Content (새로운 CaptureTarget, VerticalLayoutGroup 적용)
      ► Title InputField (사이클 타이틀 입력란)
      ► SlotContainer (기존 Content, CycleVerticalLayout이 제어할 실제 슬롯들의 부모)
```
