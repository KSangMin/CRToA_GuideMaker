#### 1. Objective (목표)
- OptionPanel에서 배경 색깔을 변경할 때, `CycleHorizontalLayout`(row)뿐만 아니라 `CycleVerticalLayout`(vert)과 `CyclePanel`(content)의 배경 색상도 함께 변경되도록 수정.
- 설정 초기화(ResetOption) 시 vert와 content의 배경 색상도 초기값으로 롤백되도록 처리.

#### 2. Implementation Design (구현 설계 / 아키텍처 변경점)
- 변경/추가될 클래스:
  - `Assets/Scripts/UI/Result/Cycle/CycleVerticalLayout.cs`
  - `Assets/Scripts/UI/Result/Cycle/CyclePanel.cs`
- 설계 개요:
  - `CycleVerticalLayout`과 `CyclePanel` 내부에 배경 이미지(`Image backgroundImage`) 참조 추가.
  - `onBackgroundColorChanged` 이벤트 리스너를 `Awake`에 등록하고 `OnDestroy`에서 해제.
  - 리스너에 연결된 `SetBackgroundColor(Color color)` 메서드를 구현하여 `backgroundImage.color` 값을 갱신.
  - 초기화 로직은 기존 `ColorSelectPanel`의 `ResetColor()`가 `onBackgroundColorChanged` 이벤트를 발생시키므로 자동 해결됨.

#### 3. Tasks (작업 리스트)

##### 1단계: 설계 및 데이터 구조화 (Planner)
- [x] `.ai_context/cur_task.md` 갱신 (배경색 적용 버그 수정)

##### 2단계: 핵심 로직 구현 (Builder)
- [ ] `Assets/Scripts/UI/Result/Cycle/CycleVerticalLayout.cs` 수정 (`backgroundImage`, `onBackgroundColorChanged` 직렬화 변수 추가, `Awake`/`OnDestroy`에 이벤트 리스닝 로직 추가, `SetBackgroundColor` 구현)
- [ ] `Assets/Scripts/UI/Result/Cycle/CyclePanel.cs` 수정 (`backgroundImage`, `onBackgroundColorChanged` 직렬화 변수 추가, 이벤트 리스닝 및 갱신 로직 추가)

##### 3단계: 방어적 검증 및 예외 처리 (QA_Debugger)
- [ ] `CycleVerticalLayout`과 `CyclePanel`이 부착된 유니티 프리팹(인스펙터)에서 `backgroundImage`와 `onBackgroundColorChanged` EventChannel 할당 상태 검증 계획 수립
- [ ] 색상 변경 후 `CyclePanel` 및 `CycleVerticalLayout`의 배경색 적용 테스트
- [ ] 초기화 버튼 동작 후 롤백 정상 작동 여부 검증

---

## 💬 User Feedback & Requests
- 2026-06-04 12:46: OptionPanel에서 배경 색깔을 바꾸면, row의 배경 색깔만 바뀌고, vert와 content의 배경 색깔은 안 바뀌어. vert와 content도 배경 색깔이 바뀌게 해 주고, 설정 초기화 시 vert와 content의 배경 색깔도 같이 초기화되게 해 줘.

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-06-04 12:47: 배경 색상 동기화 누락 수정 태스크 분석 및 cur_task.md 반영
