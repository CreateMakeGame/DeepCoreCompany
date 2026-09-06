# 🎮 DeepCoreCompany

> Unity와 C#을 기반으로 개발 중인 3D 채광 프로젝트입니다.
![Unity](https://img.shields.io/badge/Unity-6000.0%2B-black?style=flat-square&logo=unity)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp)
![Platform](https://img.shields.io/badge/Platform-PC%20%2F%20Windows-blue?style=flat-square)
![Status](https://img.shields.io/badge/Status-In%20Development-orange?style=flat-square)

플레이어의 게임 플레이를 중심으로 **Player Controller, UI, Camera** 등의 시스템을 구현하고 있습니다.

단순한 기능 구현에 그치지 않고, 기능 간 의존성을 줄이고 유지보수하기 쉬운 구조를 만드는 것을 목표로 개발하고 있습니다.

---

## 📌 Project Overview

| 항목          | 내용             |
| ----------- | -------------- |
| Genre       | [채광, 어드벤처, 생존]        |
| Engine      | Unity [6]     |
| Language    | C#             |
| Platform    | PC             |
| Development | [개인 프로젝트]  |
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

* **Engine:** Unity [버전]
* **Language:** C#
* **Version Control:** Git / GitHub
* **IDE:** [Visual Studio / Rider]
* **Tools:** GitHub Desktop

### Unity Packages

* [Input System]
* [Cinemachine]
* [TextMeshPro]
* [기타 사용 패키지]

---

## 🎮 Main Features

### Player Controller

* 플레이어 이동
* 점프
* 플레이어 상태 관리
* UI 상태에 따른 플레이어 입력 제어

### Inventory System

* 아이템 데이터 관리
* 인벤토리 데이터 추가 및 제거
* 인벤토리 슬롯 관리
* Inventory와 UI 연동

### UI System

* UI 열기 / 닫기
* UI 상태 관리
* UI 활성화 상태에 따른 플레이어 입력 제어

### Camera System

* 플레이어 카메라 제어
* UI 활성화 상태에 따른 카메라 입력 제어

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
├── Scripts/
│   ├── Player/
│   ├── Inventory/
│   ├── UI/
│   └── Camera/
│
├── Prefabs/
├── Scenes/
├── Materials/
└── Resources/
```

| Directory           | Description      |
| ------------------- | ---------------- |
| `Scripts/Player`    | 플레이어 이동 및 상태 관리  |
| `Scripts/Inventory` | 인벤토리 데이터 및 로직    |
| `Scripts/UI`        | UI 상태 및 UI 관련 로직 |
| `Scripts/Camera`    | 카메라 제어           |
| `Prefabs`           | 재사용 가능한 게임 오브젝트  |
| `Scenes`            | 게임 씬             |

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

## 🎮 Controls

| Input         | Action    |
| ------------- | --------- |
| W / A / S / D | Move      |
| Space         | Jump      |
| Mouse         | Camera    |
| [Key]         | Inventory |
| [Key]         | UI / Menu |

---

## 🚀 How to Run

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

### Inventory System

* 인벤토리 데이터 구조 설계
* 아이템 데이터 추가
* Inventory 동작 테스트

### UI / Camera

* UI 열기 / 닫기 기능 구현
* UI 활성화 상태에 따른 Player Input 제어
* UI 활성화 상태에 따른 Camera Input 제어
* 관련 코드 Refactoring

---

## 🔮 Future Improvements

* [ ] [추가 기능]
* [ ] [시스템 개선]
* [ ] [성능 최적화]
* [ ] [UI 개선]
* [ ] [추가 게임 콘텐츠]

---

## 👤 Developer

**[이름 / GitHub ID]**

* GitHub: [GitHub Profile]
* Portfolio: [Portfolio Link]
* Email: [Email]

---

## 📄 License

This project is for portfolio and educational purposes.

Assets and third-party packages are subject to their respective licenses.
****
