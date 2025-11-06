using UnityEngine;

public class HeadLockedButton : MonoBehaviour
{
    public float distance = 2f; 

    void LateUpdate()
    {
        Transform head = Camera.main.transform;
        
        // 카메라 앞 위치
        transform.position = head.position + head.forward * distance;

        // 버튼이 항상 카메라를 바라보도록 회전
        transform.rotation = Quaternion.LookRotation(transform.position - head.position);
    }
}
