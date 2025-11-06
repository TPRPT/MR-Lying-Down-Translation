using UnityEngine;
using System.Collections;
using TMPro;

public class TrialManager : MonoBehaviour
{
    [Header("Trial Settings")]
    public Transform[] startPoints;   // 시작 위치들
    public Transform targetPoint;     // 목표 위치
    public GameObject spherePrefab;   // (Rigidbody + GrabInteractable + TrialSphere)
    public Transform head;            // VR 카메라

    [Header("UI")]
    public TextMeshProUGUI feedbackText;

    [Header("Controller Input")]
    public OVRInput.Button startButton = OVRInput.Button.One;
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Header("Flow")]
    [Tooltip("Trial 간 대기(초). 너무 짧으면 Google Form이 드랍될 수 있음")]
    public float nextTrialDelay = 0.6f;

    private GameObject currentSphere;
    private Vector3 lastHeadPos;
    private float headDistance;

    private int trialNumber = 0;
    private int[] shuffledOrder;
    private int currentIndex = -1;

    private GoogleFormLogger formLogger;

    private void Start()
    {
        if (head == null) head = Camera.main.transform;
        lastHeadPos = head.position;

        if (feedbackText != null)
        {
            feedbackText.text = "Press Button to Start";
            feedbackText.color = Color.yellow;
        }

        // 업로드용 로거(Manager에 붙여 코루틴 안정화)
        formLogger = gameObject.AddComponent<GoogleFormLogger>();

        // 각 시작 위치를 한 번씩만(이전에 잘됐던 구조)
        shuffledOrder = GenerateShuffledOrder(startPoints.Length);
    }

    private void Update()
    {
        // 머리 이동 누적
        headDistance += Vector3.Distance(head.position, lastHeadPos);
        lastHeadPos = head.position;

        // if (OVRInput.GetDown(startButton, controller))
        //    StartTrial();
    }

    public void StartTrial()
    {
        if (currentSphere != null)
            Destroy(currentSphere);

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
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }

        int currentStartID = shuffledOrder[currentIndex];
        Vector3 startPos = startPoints[currentStartID].position;
        Quaternion startRot = startPoints[currentStartID].rotation;

        currentSphere = Instantiate(spherePrefab, startPos, startRot);
        currentSphere.transform.SetParent(null, true); // 부모 영향 제거

        var sphereScript = currentSphere.GetComponent<TrialSphere>();
        if (sphereScript == null) sphereScript = currentSphere.AddComponent<TrialSphere>();
        sphereScript.Init(this, currentStartID, targetPoint);

        if (feedbackText != null)
        {
            feedbackText.text = $"Trial {trialNumber} started (StartID={currentStartID})";
            feedbackText.color = Color.cyan;
        }
    }

    public void CompleteTrial(
        int startID, bool success,
        float manipulationTime, float coarseTime, float repositionTime,
        float handMoveDist, float handRotAngle
    )
    {
        // Google Form 업로드 (Manager가 코루틴 유지)
        if (formLogger != null)
        {
            StartCoroutine(formLogger.PostTrialData(
                trialNumber, startID,
                manipulationTime, coarseTime, repositionTime,
                handMoveDist, handRotAngle,
                headDistance, success ? 1 : 0
            ));
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
        yield return new WaitForSeconds(delay);
        StartTrial();
    }

    private int[] GenerateShuffledOrder(int n)
    {
        int[] arr = new int[n];
        for (int i = 0; i < n; i++) arr[i] = i;

        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}
