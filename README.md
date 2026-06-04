# 🍪 쿠키런: 모험의 탑 - 딜 사이클 이미지 제작 툴 (CRToA GuideMaker)

[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-blue?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-WebGL%20%7C%20Windows-orange)](#)
[![Language](https://img.shields.io/badge/Language-C%23-green?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)

유저들이 **쿠키런: 모험의 탑(Cookie Run: Tower of Adventures)** 캐릭터들의 스킬 로테이션과 사이클을 직관적으로 디자인하고 고화질 공략 이미지로 저장할 수 있도록 지원하는 웹 기반 유틸리티 도구입니다.

### 🎮 플레이 해보기
### [웹에서 바로 실행하기](https://ksangmin.github.io/CRToA_GuideMaker/)

---

## 📸 미리보기
<img width="1275" height="713" alt="602626007-93cecdd6-26c7-49a2-a015-25a7a82b2eb2" src="https://github.com/user-attachments/assets/50674a88-6a4c-4e48-9593-8785bb9ba9eb" />

---

## ✨ 핵심 기능

### 1. 동적 리소스 로드 (Addressables)
- **ScriptableObject 기반 데이터**: 쿠키와 스킬의 메타데이터(이름, 속성, 클래스 등)를 모듈화하여 관리합니다.
- **원격 비동기 에셋 로드**: `Addressables` 시스템을 사용해 원격 서버로부터 고해상도 쿠키 초상화 및 스킬 아이콘을 실시간으로 다운로드하여 로드합니다. WebGL 빌드 용량을 최적화하고 에셋 업데이트를 용이하게 합니다.

### 2. 드래그 앤 드롭 사이클 에디터
- **좌측 패널 (선택 영역)**: 모든 쿠키의 스킬 슬롯과 특수 슬롯 리스트를 스크롤 뷰 형식으로 제공합니다.<br>
  <img width="442" height="646" alt="image" src="https://github.com/user-attachments/assets/8ba42850-3343-4cf6-88f9-2e0155afec0d" />

- **우측 패널 (편집 영역)**: 좌측 패널의 슬롯을 클릭하거나 꾹 누른 뒤 드래그하여 우측 패널에 동적으로 추가할 수 있습니다.<br>
  <img width="811" height="191" alt="image" src="https://github.com/user-attachments/assets/723e6259-7015-4395-b05d-af6f514a60a0" />
  - 패널의 설정도 변경할 수 있습니다.

- **자유로운 순서 편집**: 추가된 슬롯을 꾹 누른 뒤 드래그하여 임의의 위치로 이동할 수 있으며, 클릭 한 번으로 간편하게 삭제할 수 있습니다.<br>
  <img width="263" height="123" alt="image" src="https://github.com/user-attachments/assets/a125612b-ba00-4080-8907-2d39443f96c5" />


### 3. 특수 슬롯 시스템
단순한 스킬 아이콘 배치 외에도 정밀한 공략 가이드를 만들 수 있도록 다양한 보조 슬롯을 지원합니다.<br>
<img width="410" height="55" alt="image" src="https://github.com/user-attachments/assets/2bde3396-2196-48f2-bdd6-c23afab2863d" />
- **초기화 슬롯**: 특정 사이클 슬롯에 적용된 특수 슬롯을 초기화시킵니다.<br>
  <img width="66" height="88" alt="image" src="https://github.com/user-attachments/assets/7dba4195-fc57-438c-a259-4f39d6a9b24a" />
- **반복 슬롯**: 특정 액션의 반복 횟수를 숫자로 표시하고 증감 버튼을 통해 실시간으로 횟수를 제어합니다.<br>
  <img width="69" height="91" alt="image" src="https://github.com/user-attachments/assets/8e414eaf-dfaa-4ef0-8f63-af20a930c93a" />
  <img width="87" height="89" alt="image" src="https://github.com/user-attachments/assets/a74a7175-028a-44a3-863f-3b189de11bc5" />
- **차지 단 수 슬롯**: 특정 스킬의 충전 횟수를 지정하고 이를 시각적인 원형 비율(Image Fill)로 표시합니다.<br>
  <img width="69" height="89" alt="image" src="https://github.com/user-attachments/assets/0d12906b-6f6f-463d-b0d6-3dca986523b4" />
  <img width="85" height="91" alt="image" src="https://github.com/user-attachments/assets/5e2500c1-c08d-4301-bb1c-d33db6868bd8" />
- **주석 슬롯**: 개별 스킬 슬롯 하단에 텍스트 인풋을 제공하여 특정 스킬 사용 시점의 주의사항을 적을 수 있습니다.<br>
  <img width="65" height="92" alt="image" src="https://github.com/user-attachments/assets/4f430f74-d1ff-4cc3-ad86-72b67125f0ed" />
  <img width="78" height="126" alt="image" src="https://github.com/user-attachments/assets/ee38b81c-4e8f-4faa-87dc-1f1ac9f64ca8" />
- **영역 지정 컴포넌트**: 시작 마커와 끝 마커를 지정하여 사이클 내 특정 구간을 하나로 묶고 이름을 부여합니다.<br>
  <img width="158" height="89" alt="image" src="https://github.com/user-attachments/assets/95b25353-2229-4478-8eb1-12d1652458cc" /><br>
  <img width="530" height="117" alt="image" src="https://github.com/user-attachments/assets/81155a49-22b0-4159-b781-49cac340f2da" />
  - 여러 영역을 서로 다른 무작위 파스텔 톤 색상으로 깔끔하게 구분합니다.
- **반복 슬롯**: 시작 슬롯과 끝 슬롯을 지정하여 반복 구간을 화살표로 표시합니다.<br>
  <img width="157" height="93" alt="image" src="https://github.com/user-attachments/assets/716c2409-ca96-4b3c-b995-59a1d5663be5" /><br>
  <img width="346" height="106" alt="image" src="https://github.com/user-attachments/assets/28cc13ba-da41-434c-80a1-360e02b515e4" />
  - 레이아웃이 줄바꿈되거나 간격이 조정되어 슬롯의 위치가 실시간으로 변하더라도, 화살표의 앵커 좌표가 최신 상태로 추적되며 보정됩니다.<br>

### 5. 옵션 패널 (Option Panel)
<img width="807" height="138" alt="image" src="https://github.com/user-attachments/assets/21ec3bd6-0aa8-4d47-b01a-bf1e13c8e09d" />

- **배경 및 텍스트 색상 커스텀**: 색상 피커 UI를 통해 전체 에디터 배경색과 타이틀/텍스트의 폰트 색상을 입맛에 맞게 커스텀할 수 있습니다.<br>
  <img width="218" height="338" alt="image" src="https://github.com/user-attachments/assets/65c78bdb-9379-4f5b-916b-01a4761eabf3" />
- **줄 길이 조절**: 한 행에 표시할 최대 슬롯 개수를 설정할 수 있습니다.<br>
  <img width="128" height="71" alt="image" src="https://github.com/user-attachments/assets/73708245-1433-47a1-a213-82737ac88951" />
  - 이미 생성되어 있는 사이클 또한, 해당 설정에 따라 모양이 변경됩니다.
- **사이클 삭제**: 현재 생성되어 있는 사이클을 전부 삭제합니다.<br>
  <img width="96" height="56" alt="image" src="https://github.com/user-attachments/assets/2cfc1432-4107-4c13-9faa-5c240f79d4d5" />
- **설정 초기화**: 위에서 변경한 설정을 초기화합니다.<br>
  <img width="96" height="55" alt="image" src="https://github.com/user-attachments/assets/46e23362-fbfb-4662-aaa9-8a04310dfdfe" />
- **이미지 저장**: 생성된 사이클을 이미지로 다운로드합니다.<br>
  <img width="107" height="122" alt="image" src="https://github.com/user-attachments/assets/48fdb5ff-7c86-4a42-aff3-7d67ee5a1664" />


### 6. 고화질 이미지 캡처 & 플랫폼별 다운로드
- 타이틀, 각주, 영역 하이라이트 박스, 화살표 등 화면의 모든 요소를 해상도 왜곡 없이 `Texture2D`로 캡처합니다.
- **WebGL 환경**: `.jslib` 플러그인을 사용하여 캡처된 이미지를 Base64 스트림으로 변환 후 브라우저 다운로드로 즉시 연동합니다.
- **PC/에디터 환경**: 로컬 파일 시스템을 통해 디렉토리에 PNG 파일로 즉시 저장됩니다.

---

## 🛠️ 사용 방법

### 📥 슬롯 배치 및 편집
| 동작 | 방법 |
| :--- | :--- |
| **슬롯 추가** | 좌측 패널의 스킬/특수 슬롯을 **클릭**하거나 **드래그하여 우측 패널로 드롭**합니다. |
| **순서 변경** | 우측 패널에 추가된 아이콘을 **꾹 누른 뒤 원하는 위치로 드래그**하여 놓습니다. |
| **슬롯 삭제** | 우측 패널에 배치된 슬롯을 **클릭**하면 즉시 삭제됩니다. |

### 🔗 연계 화살표 연결하기
1. 좌측 패널의 `화살표 시작` 및 `화살표 끝` 슬롯을 우측 사이클 내부의 원하는 슬롯에 각각 배치합니다.
2. 화살표의 숫자를 클릭해서 반복 횟수를 설정합니다.

### 📦 영역(Phase) 묶기
1. 좌측 패널의 `영역 시작` 마커와 `영역 끝` 슬롯을 우측 사이클 내부의 원하는 슬롯에 각각 드롭합니다.
2. 영역 이름을 인풋 뷰에 입력하여 묶인 구간에 레이블을 부착할 수 있습니다.

### 💾 이미지로 저장
1. 우측 상단의 `이미지 저장` 버튼을 누르면 캡처 코루틴이 동작합니다.<br>
  <img width="558" height="103" alt="image" src="https://github.com/user-attachments/assets/6b80facf-cf7f-4d2e-9ed7-37dff2d01173" /><br>
2. 작성된 사이클 공략 이미지가 로컬 디바이스에 저장됩니다.

---

## 📝 패치노트

버그 수정 및 기능 업데이트 내역은 아래 릴리스 페이지에서 확인하실 수 있습니다.

<a href="https://github.com/KSangMin/CRToA_GuideMaker/releases"><img src="https://img.shields.io/badge/📦_Releases-Download-0078D7?style=for-the-badge&logo=github" alt="Releases" /></a>

---

> 이 콘텐츠는 데브시스터즈의 제휴, 후원 또는 승인을 받지 않은 콘텐츠입니다.
