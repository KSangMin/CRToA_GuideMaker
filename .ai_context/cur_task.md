# 📍 Current Task: 하단 주석 슬롯 (Comment Slot) 드래그 앤 드롭 구현

## 🎯 Current Goal
- 특수 슬롯 패널에서 주석 슬롯(`CommentSlot`)을 드래그앤드롭하여 특정 `CycleSlot` 하단에 주석을 달 수 있는 인게임 뷰를 활성화한다.
- `ResetSlot`을 드롭 시 주석이 리셋되고 비활성화되도록 연동한다.

---

## 📝 Todo List

### 1. 설계 및 기획 (현재 진행 중)
- [x] 드래그 앤 드롭 방식 요구사항 파악 및 설계
- [x] 유저 기획 확인 및 프리팹 명칭 피드백 완료 (CommentSlot 명명)

### 2. UI 및 프리팹 설정
- [x] 유저 수동 작업: `CycleSlot.prefab` 내부 UI 구조 세팅 (CommentInput 추가, LayoutGroup 및 SizeFitter 구성)
- [x] 유저 수동 작업: `ResetSlot.prefab`을 복제하여 `CommentSlot.prefab` 생성 및 컴포넌트 교체 (ResetSlot -> CommentSlot)

### 3. 스크립트 로직 연동
- [x] `CommentSlot.cs` 클래스 구현 (`SelfGhostSlot` 상속 및 `ProcessDrop` 구현)
- [x] `SpecialPanel.cs`에 `CommentSlot` 필드 등록 및 초기화
- [x] `CycleSlot.cs`에 `EnableComment()` 구현 및 `ResetSlot()` 연동
- [x] 주석 텍스트 변경 이벤트 발생 시 `LayoutRebuilder` 및 `ReBuildLayout` 처리

### 4. QA 및 마무리
- [x] 드래그 앤 드롭 동작 검증
- [x] 텍스트 타이핑 및 줄바꿈에 따른 레이아웃 늘어남 검증
- [x] 리셋 슬롯으로 주석 비활성화 검증
- [x] 이미지 캡처 시 주석이 정상 포함되는지 검증

---

## 💬 User Feedback & Requests (Cursor 지시용)
**[줄 단위 피드백 작성법]**
特定 항목에 대한 피드백은 해당 줄 바로 아래에 마크다운 인용구(`>`) 기호만 사용하여 달아주세요.

**[전체 피드백]**
- 에디터 스크립트 방식이 아닌, 인게임 특수 슬롯 패널에서 드래그하여 CycleSlot 위에 드롭하면 하단 주석이 켜지게 개발합니다.

## ⚠️ User Ad-hoc Notes & Change Logs
- 2026-05-21: `CommentSlot` 드래그 앤 드롭 및 하단 주석 입력 가변 레이아웃(동적 리사이즈) 최종 완료. 프리팹 제작은 수동으로 진행 및 완료 검증.
