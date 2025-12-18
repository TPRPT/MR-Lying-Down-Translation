# MR-Lying-Down-Translation

Meta Quest 환경에서 **3D Translation (Grab & Move) 조작 과제**를 구현하기 위한  
Unity (C#) 기반 연구용 프로젝트입니다.

본 저장소는 **Meta XR Interaction SDK**를 활용하여  
컨트롤러 기반 객체 조작 과제를 구성하고,  
조작 시간·이동 거리·회전량·성공 여부 등의 **정량적 성능 지표를 자동 측정**하는 데 목적이 있습니다.

---

## 📌 개발 환경

- **HMD**: Meta Quest 3  
- **Engine**: Unity  
- **SDK**: Meta XR Interaction SDK (v78.0.0)  
- **Interaction**: Controller 기반 Grab (Hand Tracking 미사용)

---

## 📁 디렉토리 구조

```
RingMyBell/
└── Assets/
    └── Samples/
        └── Meta XR Interaction SDK/
            └── 78.0.0/
                └── Example Scenes/
                    ├── MainTranslationTask.unity
                    ├── ArmReachTest.unity
                    └── Script/
                        ├── HeadLockedParentPoints.cs
                        ├── TrialManager.cs
                        ├── TrialSphere.cs
                        ├── GoogleFormLogger.cs
                        └── *.cs.meta
```

---


---

## 🎬 Example Scenes 설명

### `MainTranslationTask.unity` (메인 실험 씬)

실제 **Translation 조작 과제**가 수행되는 메인 씬입니다.

- 머리 기준 Destination / Starting Points 생성
- Trial 흐름 및 컨트롤러 입력 관리
- 성능 지표 측정 및 데이터 로깅

> ✔️ 실험 수행 및 데이터 수집 시 **반드시 이 씬을 사용**

---

### `ArmReachTest.unity`

사용자의 **팔 도달 범위 및 작업 공간 감각**을 확인하기 위한 테스트 씬입니다.

- 오브젝트 배치 거리 및 각도 설정 전 참고용
- 실험 설계 보조 목적

> ❌ 정식 데이터 수집에는 사용되지 않음

---

## 🧩 주요 스크립트 설명

### `HeadLockedParentPoints.cs`

머리 기준(head-centered) 좌표계에서  
**목표 지점(Destination)과 시작 지점(Starting Points)** 을 생성·관리하는 스크립트입니다.

**주요 기능**
- 머리 정면 기준 Destination 생성
- 좌/우 각도 기반 Starting Points 생성
- 모든 기준점을 머리 위치·회전에 고정
- Trial 중 Head Tracking ON / OFF 제어

**설계 개념**
> 모든 조작 기준 위치는 사용자의 머리를 원점으로 정의됨

---

### `HeadLockedCube.cs`

특정 오브젝트를 **항상 머리 앞에 고정(head-locked)** 시키는 보조 스크립트입니다.

**주요 기능**
- 카메라 forward + up offset 위치 유지
- 실험 기준점 또는 디버그 오브젝트 표시 용도

---

### `TrialManager.cs`

**전체 Trial 흐름을 제어하는 핵심 매니저 스크립트**입니다.

**주요 기능**
- 컨트롤러 입력(A / B 버튼) 처리
- Trial 시작, 중단, 반복 관리
- 시작 지점 랜덤 셔플 및 반복 횟수 제어
- 머리 이동 거리 누적 측정
- TrialSphere 및 Logger 연동

**입력 동작**
- **A 버튼**: 연습 Trial (데이터 업로드 ❌)
- **B 버튼**: 본 Trial (데이터 업로드 ⭕)

---

### `TrialSphere.cs`

조작 대상인 **Sphere 오브젝트에 부착되는 스크립트**입니다.

**주요 기능**
- Grab / Release 이벤트 감지
- 성능 지표 계산
  - Manipulation Time
  - Coarse Translation Time
  - Reposition Time
  - Hand Movement Distance
  - Hand Rotation Angle
- 목표 지점과의 거리 기반 성공 판정
- 성공 범위 진입 시 색상 변화 (시각 피드백)

**구현 특징**
- 컨트롤러 Transform 기반 정밀 측정
- Rigidbody는 Kinematic 상태 유지

---

### `GoogleFormLogger.cs`

Trial 결과를 **Google Form으로 전송**하는 로깅 스크립트입니다.

**주요 기능**
- HTTP POST 방식 데이터 업로드
- Trial 번호, 시간/거리/회전 지표, 성공 여부 기록

> ⚠️ Google Form URL 및 entry ID는 실험마다 수정 필요

---

## 📝 참고 사항

- `.meta`파일을 삭제하거나 새로 생성 시 참조가 끊어질 수 있으므로 수동 수정은 권장하지 않습니다.
- 본 코드는 연구 및 실험 목적으로 작성되었으므로, 학술적 활용 시 적절한 인용을 권장합니다.
- 거리, 각도, 성공 기준 등 주요 파라미터는 실험 목적에 맞게 조정 가능합니다.
