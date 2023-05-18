using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;  // 근접공격 범위가 파괴되지 않기위한 변수
    public bool isRock;

    void OnCollisionEnter(Collision collision)
    {
        if(!isRock && collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);  // 3초뒤에 탄피 사라짐
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall") //|| other.gameObject.tag == "Floor")
        {
            Destroy(gameObject); // 바로 사라지게
        }
    }
}
