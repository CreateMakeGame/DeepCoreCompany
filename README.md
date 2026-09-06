# 🎮 DeepCoreCompany

> Unity와 C#을 기반으로 개발 중인 3D 채광 프로젝트입니다.

![Unity](https://img.shields.io/badge/Unity-6000.0%2B-black?style=flat-square&logo=unity)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-PC%20%2F%20Windows-blue?style=flat-square)
![Status](https://img.shields.io/badge/Status-In%20Development-orange?style=flat-square)

플레이어의 게임 플레이를 중심으로 **Player Controller, UI, Camera** 등의 시스템을 구현하고 있습니다.

단순한 기능 구현에 그치지 않고, 기능 간 의존성을 줄이고 유지보수하기 쉬운 구조를 만드는 것을 목표로 개발하고 있습니다.

---

## 🎥 Gameplay

### Main Gameplay

![Gameplay](docs/images/gameplay.gif)

### Inventory

![Inventory](docs/images/inventory.gif)

### UI Interaction

![UI](docs/images/ui.gif)

> Gameplay 영상 및 GIF는 개발 진행에 따라 업데이트할 예정입니다.

---

## 📌 Project Overview

본 프로젝트는 Unity 엔진 기반의 3D 프로젝트로, **절차적 맵 생성 로직**과 **확장 가능한 UI 및 데이터 관리 아키텍처** 구축에 중점을 두고 개발되었습니다.

동적 지형 생성을 위한 **절차적 동굴 생성 알고리즘**을 도입하고, 상호작용 가능한 객체(의뢰 게시판, 아이템 슬롯 등)와 시스템 간의 결합도를 낮추기 위해 **인터페이스 기반 설계 및 Dynamic Registry Pattern**을 적용했습니다. 

또한, **Cinemachine v3**와 **Unity Input System**을 연동하여 UI 활성화 시 카메라 시선 및 캐릭터 이동 조작이 원활하게 제어되도록 설계했습니다.

- **핵심 목표**: 매번 새로워지는 탐색 환경(동굴 생성) 구축 및 UI 상태 전환 시 플레이어 입력 제어의 안정성 확보
- **주요 구현**: 절차적 동굴 생성, 플레이어 이동/카메라 제어, 의뢰서 시스템, 아이템 슬롯 UI, UI Stack 관리

| 항목          | 내용             |
| ----------- | -------------- |
| Genre       | [채광, 어드벤처, 생존]        |
| Engine      | Unity 6 (6000.3.10f1)    |
| Language    | C#             |
| Platform    | PC (Windows)   |
| Development | 개인 프로젝트 |
| Status      | In Development |

---

## 🎯 Development Goals

* Unity 기반 게임 플레이 시스템 구현
* C#을 활용한 객체지향적 코드 작성
* 게임 시스템 간 의존성을 고려한 구조 설계
* UI와 게임 플레이 시스템의 상태 관리
* Git / GitHub를 활용한 개발 과정 및 버전 관리

---

## 🛠 Tech Stack

- **Engine & Core**: Unity, C#
- **Camera & Input**: Unity Cinemachine (v3), Unity Input System (`InputReader` 이벤트 기반 처리)
- **Procedural Generation**: Cell Automata / Marching Cubes Algorithm (동굴 지형 생성)
- **UI System**: TextMeshPro, UnityEngine.UI (UGUI), Dynamic LayoutRebuilder
- **Architecture**: Generic Singleton Pattern, Interface-based Interaction Framework, UI Stack Architecture
- **Version Control**: Git / GitHub


## 🎮 Main Features

### 1. 플레이어 컨트롤러 & 카메라 (Player Controller & Camera System)
- **Input System 기반 이동**: Unity New Input System을 연동하여 지연 없는 플레이어 이동 및 회전 처리.
- **Cinemachine v3 연동**: 카메라 감도 수평/수직 제어 및 입력축(`CinemachineInputAxisController`) 동적 비활성화 조작 지원.

### 2. 절차적 동굴 생성 및 맵 제작 (Procedural Cave Generation & Map Building)
- **동적 맵 생성 알고리즘**: 시드(Seed) 값을 기반으로 동굴 지형 및 벽면, 통로를 런타임에 절차적으로 생성.
- **맵 디스플레이 및 타일 바인딩**: 생성된 지형 데이터를 바탕으로 메시/메시 콜라이더를 동적 배치하여 재탐색 가치 부여.

### 3. 의뢰서 시스템 (Contract System)
- **데이터 기반 의뢰 매핑**: `ContractManager` 데이터를 파싱하여 의뢰 리스트 스크롤 뷰에 동적 프리팹 생성.
- **상세 패널 바인딩**: 의뢰 선택 시 우측 상세 패널(`Target`, `Quantity`, `Reward` 등) 텍스트 데이터 갱신 및 `LayoutRebuilder`를 이용한 포맷팅.

### 4. 아이템 슬롯 시스템 (Item Slot System)
- **슬롯 UI 데이터 표현**: 아이템 아이콘, 개수 텍스트 및 희귀도 프레임 표시 기능.
- **인터랙션 연동**: 아이템 슬롯 선택, 호버링 시 툴팁 반환 및 슬롯 데이터 상태 동기화.

### 5. UI Stack & 입력 통제 관리 (UI Manager Architecture)
- **LIFO 기반 팝업 관리**: `openUIStack`을 활용하여 중첩 팝업 UI 상태 관리 및 `ESC` 키 입력(Cancel Event) 처리.
- **입력 권한 동기화**: UI 열림/닫힘 상태에 맞춰 마우스 커서 상태(`LockMode`, `visible`) 및 카메라/플레이어 이동 입력 자동 차단/복구.

---

## 💻 Technical Implementation

### UI와 Player Camera 입력 제어

#### Problem

UI가 활성화된 상태에서도 플레이어의 카메라 입력이 동작하여,
UI를 조작하는 동안 의도하지 않게 카메라가 움직이는 문제가 발생했습니다.

#### Cause

UI의 활성화 상태와 Player / Camera Input의 제어가 분리되어 있어
UI 상태가 변경되어도 게임 플레이 입력이 정상적으로 차단되지 않았습니다.

#### Solution

UI 상태를 기준으로 Player Controller와 Camera Controller의
입력 처리를 제어하도록 구조를 수정했습니다.

#### Result

* UI 활성화 시 플레이어 이동 및 카메라 입력 제한
* UI 종료 후 기존 게임 플레이 입력 복구
* UI와 게임 플레이 시스템 간 상태 관리 개선

---

## 🏗 Project Structure

```text
Assets/
├── 00.Scenes/
├── 01.ScripableObject/
├── 02.Scripts/
│   ├── 00.Player/
│   ├── 10.Voxel/
│   ├── 11.Item/
│   ├── 12.Contract/
│   ├── 94.System/
│   ├── 95.UI/
│   ├── 96.Static/
│   ├── 97.Manager/
│   └── 99.Interface/
│
├── 03.Prefabs/
├── 04.Animations/
├── 05.Art/
├── 07.InputSystem/
└── 99.Asset/
```

| Directory           | Description      |
| ------------------- | ---------------- |
| `Scenes`            | 게임 씬          |
| `ScripableObject`   | 게임 데이터 관리  |
| `Scripts/Player`    | 플레이어 이동 및 상태 관리  |
| `Scripts/Voxel`     | 지형 및 동굴 관련 로직    |
| `Scripts/System`    | 게임 시스템 관련 로직 |
| `Scripts/UI`        | UI 및 팝업 관련 로직  |
| `Prefabs`           | 재사용 가능한 게임 오브젝트  |
| `Animations`        | 게임 애니메이션 관련 데이터   |
| `Art`               | 이미지 소스 관련 데이터   |
| `InputSystem`       | 입력 시스템 관련 로직 데이터   |
| `Asset`             | 에셋 관련 데이터 폴더   |

---

## 🎮 Controls

| Input Action       | Action        |
| ------------------ | ------------- |
| Move               | W / A / S / D |
| Jump               | Space         |
| Look               | Mouse         |
| Dig                | left_Click    |
| Cancel / Option UI | ESC           |

---

## 🚀 How to Run
Requirements
- Unity 6 (6000.0 이상 권장)

- Windows OS

### Requirements

* Unity [버전]
* Windows [버전]

### Steps

1. Clone this repository.
2. Open the project using Unity Hub.
3. Select the required Unity version.
4. Open `[Main Scene 경로]`.
5. Press **Play** to start the game.

---

## 🔧 Troubleshooting

### UI를 열었을 때 카메라가 계속 움직이는 문제

**Problem**

UI가 활성화된 상태에서도 마우스 입력이 Camera Controller에 전달되는 문제가 발생했습니다.

**Cause**

UI 상태와 게임 플레이 입력 상태가 독립적으로 관리되고 있었습니다.

**Solution**

UI 상태를 기준으로 Player / Camera Controller의 입력 처리를 제어하도록 수정했습니다.

**Result**

UI 활성화 상태에서는 게임 플레이 입력을 제한하고,
UI가 닫히면 정상적으로 입력을 복구하도록 개선했습니다.

---

## 📚 Development Log

### Player & Camara

* Input System 연동 및 이동 제어
* Cinemachine Camera 입력축 동적 제어

### Voxel 

* Voxel데이터 테이블 생성
* Voxel를 활용한 지형 생성
* 동굴 생성 및 랜덤 시드 적용
* 동굴 아이템 생성 적용

### Contract & Slot System

* 의뢰서 데이터 추가
* 의뢰서 목록 호출

* 아이템 슬롯 데이터 구조 설계
* 아이템 데이터 추가
* 마우스 힐을 통한 선택 추가

### UI

* UI 열기 / 닫기 기능 구현
* UI 활성화 상태에 따른 Player Input 제어
* UI 활성화 상태에 따른 Camera Input 제어
* 관련 코드 UImanger.cs

---

## 🔮 Future Improvements

* [ ] 동굴 몬스터/오브젝트 무작위 스폰: 생성된 동굴 내부 영역을 감지하여 적 및 수집 오브젝트 자동 스폰 로직 연동
* [ ] 채광 시 딜레이 되는 문제점 수정 예정

---

## 👤 Developer

**[김태겸 / BeautifulMaple]**

* GitHub: [[GitHub Profile]](https://github.com/BeautifulMaple)
* Portfolio: [Portfolio Link]
* Email: [Email](xorua4510@gmail.com)

---

## 📄 License

This project is for portfolio and educational purposes.

Assets and third-party packages are subject to their respective licenses.
****
