#### 1. Objective (목표)
- ChargeCountSlot 드롭 시 CycleSlot의 ChargeCountBackground 이미지를 Filled 타입으로 전환하고, `_curCount / _maxCount` 비율에 따라 fillAmount를 적용

#### 2. Implementation Design (구현 설계 / 아키텍처 변경점)
- 변경/추가될 클래스: 
  - `Assets/Scripts/UI/Panel/Special/BaseCountSlot.cs` (`ApplyToCycleSlot` 시그니처에 `maxCount` 파라미터 추가)
  - `Assets/Scripts/UI/Panel/Special/CountSlot.cs` (`ApplyToCycleSlot` 시그니처 변경 대응)
  - `Assets/Scripts/UI/Panel/Special/ChargeCountSlot.cs` (`ApplyToCycleSlot` 시그니처 변경 및 `SetChargeCount` 호출 시 `maxCount` 전달)
  - `Assets/Scripts/UI/Result/Cycle/CycleSlot.cs` (`SetChargeCount`에서 `maxCount` 수신 및 `Image.fillAmount` 조작)
- 데이터 흐름: `BaseCountSlot`의 `ProcessDrop` 시 `_maxCount`도 함께 넘김 -> `ChargeCountSlot`이 이를 받아 `CycleSlot`에 전달 -> `CycleSlot` 내부에서 `chargeCountBackground.GetComponent<Image>()`를 가져와 `Filled` 설정 및 `fillAmount` 적용

#### 3. Tasks (작업 리스트)

##### 1단계: 설계 및 데이터 구조화 (Planner)
- [x] `.ai_context/cur_task.md` 갱신 (Image Fill 로직 반영)

##### 2단계: 핵심 로직 구현 (Builder)
- [x] `Assets/Scripts/UI/Panel/Special/BaseCountSlot.cs` 수정 (`ApplyToCycleSlot` 시그니처: `int count, int maxCount`)
- [x] `Assets/Scripts/UI/Panel/Special/CountSlot.cs` 수정 (시그니처 변경 대응)
- [x] `Assets/Scripts/UI/Panel/Special/ChargeCountSlot.cs` 수정 (시그니처 변경 대응, `cycleSlot.SetChargeCount(count, maxCount)` 호출)
- [x] `Assets/Scripts/UI/Result/Cycle/CycleSlot.cs` 수정 (`SetChargeCount` 메서드 파라미터 추가, `Image` 컴포넌트 획득 후 `type`, `fillMethod`, `fillOrigin`, `fillAmount` 설정 로직 추가)

##### 3단계: 방어적 검증 및 예외 처리 (QA_Debugger)
- [x] `CycleSlot` 프리팹의 `ChargeCountBackground`에 `Image` 컴포넌트 정상 작동 확인 및 UI 렌더링 확인
- [x] 런타임 테스트: ChargeCountSlot 드래그 앤 드롭 시 `maxCount` 비율에 맞게 UI Fill이 시각적으로 올바르게 갱신되는지 검증
- [x] 0으로 나눔(DivideByZero)이나 `_maxCount`가 0일 때 방어 로직 검증

---

## 💬 User Feedback & Requests
- 2026-06-03 19:03: ChargeCountSlot을 CycleSlot에 드래그앤드롭했을 때, 단순히 CycleSlot의 ChargeCount를 온오프하는 게 아니라, ChargeCount 이미지를 filled로 바꾸고, _max 값의 _cur 값 비율만큼 fill되게 만들고 싶어. 텍스트는 그대로 남겨둘 거야.

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-06-03 19:04: Planner를 통한 ChargeCount Background Fill 로직 설계 및 Task 정의 완료

## 🌲 Proposed Hierarchy
```text
▼ CycleSlot (Prefab)
  ▼ ChargeCountBackground (Image Component - Filled 처리 예정)
    ► ChargeCountText
```
