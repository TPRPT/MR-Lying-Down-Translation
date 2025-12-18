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

    // private void Update()
    // {
    //     if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
    //     {
    //         Debug.Log("B버튼 누름");
    //         if (isHeadTracking)
    //             DisableHeadTracking();
    //         else
    //             EnableHeadTracking();
    //     }
    // }

    GameObject CreateStarting(float angle)
    {
        GameObject go = Instantiate(startingPrefab, parentGroup.transform);
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
        go.transform.localPosition = rot * Vector3.forward * distance;
        go.transform.localRotation = rot;
        return go;
    }

    void LateUpdate()
    {
        if (!isHeadTracking) return;

        // Parent가 머리를 따라 움직이게
        Transform head = Camera.main.transform;
        parentGroup.transform.position = head.position;
        parentGroup.transform.rotation = head.rotation;
    }

   public void DisableHeadTracking()
    {
        isHeadTracking = false;
        if (cube != null) cube.SetActive(false);
        Debug.Log("HeadTracking OFF");
    }

    public void EnableHeadTracking()
    {
        isHeadTracking = true;

        if (cube == null)
        {
            cube = Instantiate(cubePrefab, parentGroup.transform);
            cube.transform.localPosition = Vector3.forward * distance;
            cube.transform.localRotation = Quaternion.identity;
        }
        else
        {
            cube.SetActive(true);
        }

        Debug.Log("HeadTracking ON");
    }

    public Transform GetDestinationTransform() => destination != null ? destination.transform : null;

    public Transform[] GetStartingTransforms()
    {
        if (startings == null) return System.Array.Empty<Transform>();
        var arr = new Transform[startings.Length];
        for (int i = 0; i < startings.Length; i++) arr[i] = startings[i].transform;
        return arr;
    }

}
