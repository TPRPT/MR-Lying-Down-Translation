using UnityEngine;
using Oculus.Interaction;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GrabInteractable))]
public class TrialSphere : MonoBehaviour
{
    private TrialManagerVR manager;
    private int startID;
    private Transform targetPoint;
    private Rigidbody rb;
    private GrabInteractable grabInteractable;

    [Header("Success Criteria")]
    [Tooltip("성공 판정 허용 오차 (미터). 예: 0.15 = 15cm")]
    public float successTolerance = 0.15f;
    [Tooltip("대략적 이동(coarse)으로 간주하는 거리 (미터)")]
    public float coarseThreshold = 0.10f;

    private bool grabbed = false;
    private Vector3 lastPos;
    private Quaternion lastRot;

    private float grabStartTime = -1f;
    private float coarseTime = -1f;
    private float moveDistance = 0f;
    private float rotAngle = 0f;

    public void Init(TrialManagerVR trialManager, int id, Transform target)
    {
        manager = trialManager;
        startID = id;
        targetPoint = target;

        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<GrabInteractable>();

        // 👉 항상 부유(중력/물리 영향 없음)
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (grabInteractable != null)
            grabInteractable.WhenPointerEventRaised += HandlePointerEvent;
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.WhenPointerEventRaised -= HandlePointerEvent;
    }

    private void Update()
    {
        if (grabbed && grabStartTime > 0f)
        {
            moveDistance += Vector3.Distance(lastPos, transform.position);
            rotAngle += Quaternion.Angle(lastRot, transform.rotation);
            lastPos = transform.position;
            lastRot = transform.rotation;

            if (coarseTime < 0f)
            {
                float d = Vector3.Distance(transform.position, targetPoint.position);
                if (d <= coarseThreshold)
                    coarseTime = Time.time - grabStartTime;
            }
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            grabbed = true;
            grabStartTime = Time.time;
            lastPos = transform.position;
            lastRot = transform.rotation;
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            grabbed = false;

            float manipulationTime = (grabStartTime > 0f) ? (Time.time - grabStartTime) : 0f;
            if (coarseTime < 0f) coarseTime = manipulationTime;
            float repositionTime = manipulationTime - coarseTime;

            // ✅ Pivot ↔ Pivot 기준 거리
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
