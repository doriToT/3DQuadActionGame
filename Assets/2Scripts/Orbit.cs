using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;
    public float orbitSpeed;
    Vector3 offSet;

    void Start()
    {
        //          슈루탄 위치          Player 위치
        offSet = transform.position - target.position;
    }

    void Update()
    {
        transform.position = target.position + offSet;
        // RotateAround(): 타겟 주위를 회전하는 함수
        transform.RotateAround(target.position,
                                Vector3.up,
                                orbitSpeed * Time.deltaTime);
        // RotateAround() 후의 위치를 가지고 목표와의 거리 유지
        offSet = transform.position - target.position;
    }
}
