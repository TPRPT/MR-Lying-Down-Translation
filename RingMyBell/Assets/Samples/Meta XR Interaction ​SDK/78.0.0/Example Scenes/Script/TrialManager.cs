using UnityEngine;
using System.Collections;
using TMPro;

public class TrialManager : MonoBehaviour
{
    [Header("Trial Settings")]
    private Transform[] startPoints;   // 시작 위치들
    private Transform targetPoint;     // 목표 위치
    public GameObject spherePrefab;   // (Rigidbody + GrabInteractable + TrialSphere)
    public Transform head;            // VR 카메라
    public int repetitions = 2; 

    [Header("UI")]
    public TextMeshProUGUI feedbackText;

    [Header("Controller Input")]
    public OVRInput.Button startButton = OVRInput.Button.One;
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Header("Flow")]
    [Tooltip("Trial 간 대기(초). 너무 짧으면 Google Form이 드랍될 수 있음")]
    public float nextTrialDelay = 0.6f;

    [Header("Head Tracking Reference")]
    public HeadLockedParentPoints headLocked;

    private GameObject currentSphere;
    private Vector3 lastHeadPos;
    private float headDistance;

    private int trialNumber = 0;
    private int[] shuffledOrder;
    private int currentIndex = -1;

    private GoogleFormLogger formLogger;
    // private GoogleFormLogger1 formLogger1;

    private bool isRunningTrial = false;
    private bool shouldUploadToForm = false;

    private void Start()
    {
        if (head == null) head = Camera.main.transform;
        lastHeadPos = head.position;

        if (feedbackText != null)
        {
            feedbackText.text = "Press Button to Start";
            feedbackText.color = Color.yellow;
        }

        formLogger = gameObject.AddComponent<GoogleFormLogger>();
        //formLogger1 = gameObject.AddComponent<GoogleFormLogger1>();


        if (headLocked == null)
        headLocked = FindFirstObjectByType<HeadLockedParentPoints>(); // 2022+ 권장

        StartCoroutine(WirePointsFromHeadLockedNextFrame());
    }

    private System.Collections.IEnumerator WirePointsFromHeadLockedNextFrame()
    {
        yield return null;
        if (headLocked != null)
        {
            var dest = headLocked.GetDestinationTransform();
            var starts = headLocked.GetStartingTransforms();

            if (dest != null) targetPoint = dest;
            if (starts != null && starts.Length > 0)
            {
                startPoints = starts;
                shuffledOrder = GenerateShuffledOrder(startPoints.Length); // 이곳으로 코드 이동
            }

            Debug.Log($"[TrialManager] Wired from HeadLocked: target={targetPoint?.name}, starts={startPoints?.Length}");
        }
    }

    private void Update()
    {
        headDistance += Vector3.Distance(head.position, lastHeadPos);
        lastHeadPos = head.position;

        // ✨ 추가: 검지 트리거 입력 확인 (오른손 컨트롤러)
        bool triggerPressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);

        bool aButtonPressed = OVRInput.GetDown(startButton, controller);
        bool bButtonPressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);

        if (isRunningTrial)
        {
            // 2. A나 B 버튼이 눌렸다면 세션 중단
            if (aButtonPressed || bButtonPressed)
            {
                Debug.Log("Trial 진행 중 버튼이 눌려 세션 중단.");
                ResetSession();
                return;
            }
        }

        if (!isRunningTrial && (aButtonPressed || bButtonPressed)) {
            // A 버튼을 눌렀을 때 (업로드하지 않음)
            if (OVRInput.GetDown(startButton, controller))
            {
                Debug.Log("A 버튼 클릭 - Google Form 업로드 OFF");
                shouldUploadToForm = false;
                HandleTrialStart();
            }

            // ✨ 추가: B 버튼을 눌렀을 때 (업로드함)
            if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            {
                Debug.Log("B 버튼 클릭 - Google Form 업로드 ON");
                shouldUploadToForm = true;
                HandleTrialStart();
            }
        }
    }

    public void ResetSession()
    {
        if (currentSphere != null)
        {
            Destroy(currentSphere);
            currentSphere = null;
        }

        isRunningTrial = false;
        currentIndex = -1;
        trialNumber = 0;

        if (headLocked != null)
        {
            headLocked.EnableHeadTracking();
        }
        
        if (feedbackText != null)
        {
            feedbackText.text = "Task Aborted! Press Button to Start Again";
            feedbackText.color = Color.yellow;
        }
    }

    private void HandleTrialStart()
    {
        // 이미 Trial이 실행 중이면 무시
        if (isRunningTrial) return;

        // Trial 시작 시 헤드 트래킹 비활성화
        if (headLocked != null)
        {
            headLocked.DisableHeadTracking();
        }
        
        // 모든 Trial이 끝난 후 다시 시작할 경우 초기화
        if (currentIndex >= shuffledOrder.Length)
        {
            Debug.Log("Task 재시작!");
            currentIndex = -1;
            trialNumber = 0;
            shuffledOrder = GenerateShuffledOrder(startPoints.Length);
        }
        else
        {
            Debug.Log("Task 처음 시작!");
        }

        StartTrial();
    }


    public void StartTrial()
    {
        Debug.Log("Trial 시작");
        if (currentSphere != null)
        {
            Destroy(currentSphere);
        }

        headDistance = 0f;
        trialNumber++;

        currentIndex++;
        if (currentIndex >= shuffledOrder.Length)
        {
            if (feedbackText != null)
            {
                feedbackText.text = "All trials finished!";
                feedbackText.color = Color.green;
            }
            Debug.Log("[TrialManager] All trials completed.");

            if (headLocked != null)
            {
                headLocked.EnableHeadTracking();
            }

            isRunningTrial = false; // Trial 종료 상태
            return;
        }

        Debug.Log("Trial 진행중");
        isRunningTrial = true;

        int currentStartID = shuffledOrder[currentIndex];
        Vector3 startPos = startPoints[currentStartID].position;
        Quaternion startRot = startPoints[currentStartID].rotation;

        currentSphere = Instantiate(spherePrefab, startPos, startRot);
        currentSphere.transform.SetParent(null, true);

        var sphereScript = currentSphere.GetComponent<TrialSphere>();
        if (sphereScript == null) sphereScript = currentSphere.AddComponent<TrialSphere>();
        sphereScript.Init(this, currentStartID, targetPoint);

        if (feedbackText != null)
        {
            string trialType = shouldUploadToForm ? "Trial" : "Practice";
            feedbackText.text = $"{trialType} {trialNumber} started (StartID={currentStartID})";
            feedbackText.color = Color.cyan;
        }
    }

    public void CompleteTrial(
        int startID, bool success,
        float manipulationTime, float coarseTime, float repositionTime,
        float handMoveDist, float handRotAngle
    )
    {
        // ✨ 수정: shouldUploadToForm이 true일 때만 Google Form 업로드
        if (shouldUploadToForm)
        {
            Debug.Log("Google Form 업로드 호출");
            if (formLogger != null)
            // if (formLogger1 != null)
            {
                StartCoroutine(formLogger.PostTrialData(
                // StartCoroutine(formLogger1.PostTrialData(
                    trialNumber, startID,
                    manipulationTime, coarseTime, repositionTime,
                    handMoveDist, handRotAngle,
                    headDistance, success ? 1 : 0
                ));
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text = success ? "Success!" : "Fail!";
            feedbackText.color = success ? Color.green : Color.red;
        }

        StartCoroutine(StartNextTrialAfterDelay(nextTrialDelay));
    }

    private IEnumerator StartNextTrialAfterDelay(float delay)
    {
        Debug.Log("다음 Trial 시작");
        yield return new WaitForSeconds(delay);
        isRunningTrial = false;
        StartTrial();
    }

    private int[] GenerateShuffledOrder(int n)
    {
        // n은 startPoints.Length (4)
        int totalTrials = n * repetitions;
        int[] arr = new int[totalTrials];

        // 모든 시작 ID를 반복 횟수만큼 배열에 채워넣기
        for (int i = 0; i < totalTrials; i++)
        {
            arr[i] = i % n;
        }

        // 배열 섞기
        for (int i = totalTrials - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}
