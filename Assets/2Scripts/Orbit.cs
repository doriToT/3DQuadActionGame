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
        //          ����ź ��ġ          Player ��ġ
        offSet = transform.position - target.position;
    }

    void Update()
    {
        transform.position = target.position + offSet;
        // RotateAround(): Ÿ�� ������ ȸ���ϴ� �Լ�
        transform.RotateAround(target.position,
                                Vector3.up,
                                orbitSpeed * Time.deltaTime);
        // RotateAround() ���� ��ġ�� ������ ��ǥ���� �Ÿ� ����
        offSet = transform.position - target.position;
    }
}