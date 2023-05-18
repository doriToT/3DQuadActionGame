using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rigid;

    void Start()
    {
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        // 물리적 속도는 Vector3.zero로 초기화
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        // 슈류탄 매쉬는 비활성화, 폭발은 활성화
        meshObj.SetActive(false); 
        effectObj.SetActive(true);

        RaycastHit[] rayHits = 
            Physics.SphereCastAll(transform.position,
                                  15, 
                                  Vector3.up, 0f, 
                                  LayerMask.GetMask("Enemy"));

        foreach(RaycastHit hitObj in rayHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        Destroy(gameObject, 5);
    }
}
