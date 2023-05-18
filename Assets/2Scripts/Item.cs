using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon};
    public Type type;
    public int value;

    Rigidbody rigid;
    SphereCollider sphereCollider;

     void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }

    // GetComponent()함수는 첫번째 컴포넌트를 가져온다.
    // 즉, 물리담당하는 Collider가 위에 있어야한다.
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            // 외부물리효과에 의해 움직이지 않게한다.
            rigid.isKinematic = true;   
            sphereCollider.enabled = false;
        }
    }
}

// enum : 열거형 타입 (타입 이름 지정필요)
