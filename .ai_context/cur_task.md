# 📍 Current Task: Slot 상속 구조 OOP 검증 및 코드 정돈 (완료)

## 🎯 Current Goal
- 파편화된 슬롯(`CycleSlot`, `SelectSlot`, `CountSlot`, `ResetSlot`)을 **`Slot` → `DraggableSlot` / `SelfGhostSlot`** 계층으로 재정립 완료.
- 유저 요청으로 2차 추출(`SkillSlotDisplay`)은 진행하지 않고, 현재의 안정적인 상속 구조에서 태스크를 종료한다.

---

## 📊 최종 코드 품질 판정 (2026-05-20 리팩토링 완료 기준)

| 항목 | 점수 | 요약 |
|------|------|------|
| 구조화 | ⭐⭐⭐⭐⭐ (5/5) | `Slot` → `DraggableSlot` / `SelfGhostSlot` 상속 계층 완비. 4종 슬롯 모두 정상 편입. |
| OOP 준수 | ⭐⭐⭐⭐ (4/5) | SRP·OCP·DRY 대폭 개선 및 Template Method 적용. (UI 로직 추가 캡슐화 생략으로 4점) |
| 유지보수성 | ⭐⭐⭐⭐⭐ (5/5) | 홀드 코루틴, 스크롤 전달, 고스트 파이프라인이 부모에 통합되어 중앙 제어 가능. |
| GC/성능 | ⭐⭐⭐⭐⭐ (5/5) | 정적 `RaycastBuffer` 재사용으로 드래그·드롭 시 매번 발생하던 List 할당 제거. |
| 도메인 분리 | ⭐⭐⭐⭐⭐ (5/5) | 인덱스 계산이 필요한 레이아웃 드롭과, 단순 UI 조작용 고스트 처리가 명확히 분리됨. |

**상태**: 본 태스크는 성공적으로 완전히 종료됨.

---

## 📝 Todo List

### 최종 마무리에 따른 정산
- [x] 베이스 계층(`Slot`, `DraggableSlot`, `SelfGhostSlot`) 구현 및 포인터 파사드 통합
- [x] 기존 4종 파생 슬롯(`CycleSlot`, `SelectSlot`, `CountSlot`, `ResetSlot`) 마이그레이션 완료
- [x] 플레이 모드 QA 진행 및 발견된 상태 불일치 버그(`CycleSlot.CancelHold`) 수정 완료
- [x] 인스펙터 Null-Check 자가 진단 코드 보강 및 메모리 누수 방지(`OnDestroy` 해제) 완료
- [x] (취소) `SkillSlotDisplay` 분리 등 2차 고도화 생략 (유저 요청)

### 2차 고도화: Slot 계층 구조 리팩토링 (LSP 위반 해결)
- [x] `SwapSlot.cs` 상단 쓰레기 네임스페이스(`System.Windows.Forms.VisualStyles`, `Unity.AppUI.UI`) 삭제.
- [x] 공통 부모 클래스 `SelectBaseSlot` 추출 및 공통 로직(고스트 드래그/드롭) 이관.
- [x] `SelectSlot`이 `SelectBaseSlot`을 상속하도록 변경 (스킬 UI 유지).
- [x] `SwapSlot`이 `SelectBaseSlot`을 상속하도록 변경 (교체 UI 특화 및 기본 텍스트 전달).

---

## 💬 User Feedback & Requests (Cursor 지시용)
**[줄 단위 피드백 작성법]**
특정 항목에 대한 피드백은 해당 줄 바로 아래에 마크다운 인용구(`>`) 기호만 사용하여 달아주세요.
(예시)
- [ ] `CountSlot` → `SelfGhostSlot` 상속, 도메인 로직만 잔류
> 이렇게 꺾쇠 기호만 사용해도 Cursor가 유저의 추가 코멘트로 완벽하게 파악합니다.

**[전체 피드백]**
전체적인 구조 변경이나 공통 요구사항이 있다면 아래에 적어주세요.
- 

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-05-18: **실측** — `Slot`/`DraggableSlot` 미구현 상태에서 설계.
- 2026-05-19: **구현 완료** — `SelfGhostSlot` 명칭으로 베이스 3종 + 4종 마이그레이션. QA(3단계) 대기.
- 2026-05-19: **QA 진행** — `CycleSlot` 스크롤 전달 오류(빈 베이스 메서드 호출) 발견 및 수정. `Slot`의 패널 스크롤 헬퍼(`TryBeginPanelScrollDrag` 등)로 완벽히 위임 완수.
- 2026-05-20: **구조 개선** — `SwapSlot` 및 `SelectSlot`의 LSP 위반 해결을 위해 템플릿 메서드 패턴을 적용한 `SelectBaseSlot` 추출 완료. 자식은 4개의 UI 프로퍼티만 넘기도록 구조 리팩토링.
- `TabSlot`/`BackgroundSlot`: Slot 계열 미편입 — 별도 태스크.
