using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GrabInteractable))]
public class TrialSphere : MonoBehaviour
{
    private TrialManager manager;
    private int startID;
    private Transform targetPoint;
    private Rigidbody rb;
    private GrabInteractable grabInteractable;

    [Header("Success Criteria")]
    [Tooltip("성공 판정 허용 오차 (미터). 예: 0.05 = 5cm")]
    public float successTolerance = 0.05f;
    [Tooltip("대략적 이동(coarse)으로 간주하는 거리 (미터)")]
    public float coarseThreshold = 0.10f;

    private Renderer sphereRenderer;
    private Color originalColor;

    [Header("Visual Feedback")]
    public Color successColor = Color.green;

    private bool grabbed = false;
    private Transform _controllerTransform;
    private Vector3 lastPos;
    private Quaternion lastRot;

    private float grabStartTime = -1f;
    private float coarseTime = -1f;
    private float moveDistance = 0f;
    private float rotAngle = 0f;

    public void Init(TrialManager trialManager, int id, Transform target)
    {
        manager = trialManager;
        startID = id;
        targetPoint = target;

        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<GrabInteractable>();

        // 항상 부유(중력/물리 영향 없음)
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (grabInteractable != null)
            grabInteractable.WhenPointerEventRaised += HandlePointerEvent;

        sphereRenderer = GetComponent<Renderer>();
        if (sphereRenderer != null)
        {
            originalColor = sphereRenderer.material.color;
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.WhenPointerEventRaised -= HandlePointerEvent;
    }

    private void Update()
    {
        if (grabbed && grabStartTime > 0f && _controllerTransform != null)
        {
            
            // 컨트롤러의 위치와 회전을 기준으로 거리와 각도 누적
            moveDistance += Vector3.Distance(lastPos, _controllerTransform.position);
            rotAngle += Quaternion.Angle(lastRot, _controllerTransform.rotation);

            // 다음 프레임을 위해 현재 컨트롤러의 위치/회전 값으로 업데이트
            lastPos = _controllerTransform.position;
            lastRot = _controllerTransform.rotation;

            if (targetPoint != null)
            {
                float d = Vector3.Distance(transform.position, targetPoint.position);

                if (d <= coarseThreshold  && coarseTime < 0f)
                {
                    coarseTime = Time.time - grabStartTime;
                }
                    
                if (d <= successTolerance)
                {
                    // 성공 범위 내에 있으면 색상 변경
                    sphereRenderer.material.color = successColor;
                }
                else
                {
                    // 범위를 벗어나면 원래 색상으로 복원
                    sphereRenderer.material.color = originalColor;
                }
            }
        }

    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            grabbed = true;
            grabStartTime = Time.time;

            if (evt.Data is GrabInteractor grabInteractor)
            {
                _controllerTransform = grabInteractor.transform;
                lastPos = _controllerTransform.position;
                lastRot = _controllerTransform.rotation;
            }
            else
            {
                Debug.LogError("The interactor is not a GrabInteractor. Cannot track movement.");
                _controllerTransform = null;
            }
        }
        else if (evt.Type == PointerEventType.Unselect && grabbed) // ← 조건 강화
        {
            grabbed = false;

            float manipulationTime = (grabStartTime > 0f) ? (Time.time - grabStartTime) : 0f;
            if (coarseTime < 0f) coarseTime = manipulationTime;
            float repositionTime = manipulationTime - coarseTime;

            // 목표점과 거리 계산
            Vector3 sphereCenter = transform.position;
            Vector3 targetCenter = targetPoint.position;
            float dist = Vector3.Distance(sphereCenter, targetCenter);
            bool success = dist <= successTolerance;

            manager.CompleteTrial(
                startID, success,
                manipulationTime, coarseTime, repositionTime,
                moveDistance, rotAngle
            );

            Destroy(gameObject);
            Debug.Log($"[TrialSphere] Trial {startID} end. Dist={dist:F3} m, Success={success}");
        }
    }
}
