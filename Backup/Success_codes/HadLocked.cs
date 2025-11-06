using UnityEngine;

public class HeadLockedParentPoints : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject cubePrefab;
    public GameObject destinationPrefab;
    public GameObject startingPrefab;

    [Header("Settings")]
    public float distance = 0.49f;
    public float[] angles = { 35f, 55f };

    private GameObject parentGroup;
    private GameObject cube;
    private GameObject destination;
    private GameObject[] startings;

    private bool isHeadTracking = true; // 기본값: 켜짐

    void Start()
    {
        // 부모 오브젝트 생성
        parentGroup = new GameObject("HeadLockedPointsGroup");

        // 중앙 큐브
        cube = Instantiate(cubePrefab, parentGroup.transform);
        cube.transform.localPosition = Vector3.forward * distance;

        // Destination (큐브와 동일 위치)
        destination = Instantiate(destinationPrefab, parentGroup.transform);
        destination.transform.localPosition = Vector3.forward * distance;
        destination.transform.localRotation = Quaternion.identity; // 초기값

        // Starting Points
        startings = new GameObject[angles.Length * 2];
        int idx = 0;
        foreach (float angle in angles)
        {
            startings[idx++] = CreateStarting(-angle); // 좌측
            startings[idx++] = CreateStarting(angle);  // 우측
        }
    }

    GameObject CreateStarting(float angle)
    {
        GameObject go = Instantiate(startingPrefab, parentGroup.transform);
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
        go.transform.localPosition = rot * Vector3.forward * distance;
        go.transform.localRotation = rot;
        return go;
    }

    void Update()
    {
        // 오른쪽 컨트롤러 A 버튼 → 헤드트래킹 토글
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            isHeadTracking = !isHeadTracking;

            // 큐브만 활성/비활성
            if (cube != null)
                cube.SetActive(isHeadTracking);

            Debug.Log("HeadTracking: " + (isHeadTracking ? "ON" : "OFF"));
        }
    }

    void LateUpdate()
    {
        if (!isHeadTracking) return;

        // Parent가 머리를 따라 움직이게
        Transform head = Camera.main.transform;
        parentGroup.transform.position = head.position;
        parentGroup.transform.rotation = head.rotation;
    }
}
