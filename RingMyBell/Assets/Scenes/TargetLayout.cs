using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetLayout : MonoBehaviour
{
    // 시작 버튼 오브젝트
    public GameObject startbutton;
    // 원형으로 배치할 타겟 오브젝트 배열
    public GameObject[] targets;
    // 별도로 배치되는 보조 타겟
    public GameObject auxiliaryTarget;
    // 위치 기준점 (오프셋)
    private Vector3 Offset;

    public GameObject startPositionReference;

    void Start()
    {
        // 시작 버튼을 고정된 위치에 배치하는 대신,
        // 참조 오브젝트의 위치로 설정
        if (startPositionReference != null)
        {
            startbutton.transform.position = startPositionReference.transform.position;
        }
        else
        {
            // 참조 오브젝트가 할당되지 않았을 경우 기본 위치 사용
            startbutton.transform.position = new Vector3(0, 1, 2f);
        }

        // 시작 버튼 위치를 기준 오프셋으로 저장
        Offset = startbutton.transform.position;
    }

    // 주어진 반지름(radius)과 크기(width)를 사용해 타겟들을 원형으로 배치
    public void PositionObjectsInCircle(float radius, float width)
    {   
        // 원의 중심점을 (0, 1.0, 0)으로 초기화하는 대신, 참조 오브젝트의 위치로 설정
        if (startPositionReference != null)
        {
            Offset = startPositionReference.transform.position;
        }
        else
        {
            // 참조 오브젝트가 할당되지 않았을 경우 기본 위치 사용
            Offset = new Vector3(0, 1.0f, 0);
        }

        // 배치할 타겟 개수
        int numberOfObjects = targets.Length;
        // 각 타겟 사이의 각도 간격
        float angleStep = 360f / numberOfObjects;

        // 보조 타겟은 원 앞쪽(Z축 방향) 고정 위치에 배치
        auxiliaryTarget.transform.position = Offset + new Vector3(0, 0, 7.5f);

        // 모든 타겟을 순회하면서 위치 배치
        for (int i = 0; i < numberOfObjects; i++)
        {
            // 현재 타겟에 대한 각도 계산 (90도 보정으로 위쪽부터 시작)
            float angle = (360 - i * angleStep + 90) % 360;
            // 삼각함수 사용을 위해 라디안 단위로 변환
            angle *= Mathf.Deg2Rad;

            // 원의 방정식을 이용해 타겟 위치 계산
            Vector3 position = new Vector3(
                Offset.x + Mathf.Cos(angle) * radius, // X 좌표
                Offset.y + Mathf.Sin(angle) * radius, // Y 좌표
                Offset.z + 2.5f                       // Z 좌표는 살짝 앞으로 고정
            );

            // 계산된 위치를 타겟에 적용
            targets[i].transform.position = position;
            // 타겟 크기를 width 값으로 균일하게 조정
            targets[i].transform.localScale = new Vector3(width, width, width);

            // 타겟 색상을 빨간색으로 변경 (Renderer가 있을 경우만)
            Renderer targetRenderer = targets[i].GetComponent<Renderer>();
            if (targetRenderer != null)
                targetRenderer.material.color = Color.red;
        }
    }
}
