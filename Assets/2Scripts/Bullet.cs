using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;  // �������� ������ �ı����� �ʱ����� ����
    public bool isRock;

    void OnCollisionEnter(Collision collision)
    {
        if(!isRock && collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);  // 3�ʵڿ� ź�� �����
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall") //|| other.gameObject.tag == "Floor")
        {
            Destroy(gameObject); // �ٷ� �������
        }
    }
}
