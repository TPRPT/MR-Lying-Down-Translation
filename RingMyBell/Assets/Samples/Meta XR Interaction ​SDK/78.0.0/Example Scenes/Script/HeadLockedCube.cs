using UnityEngine;

public class HeadLockedCube : MonoBehaviour
{
    public float distance = 1f; // 머리 앞 고정 거리

    void LateUpdate()
    {
        Transform head = Camera.main.transform;

        // 1. 위쪽으로 띄울 offset을 정의합니다.
        Vector3 upOffset = head.up * 0.3f;

        // 2. 위치는 카메라의 앞 + 위로 고정
        transform.position = head.position + head.forward * distance + upOffset;

        // 3. 회전은 그대로
        transform.rotation = head.rotation;
    }
}
