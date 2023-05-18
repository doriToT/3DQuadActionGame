using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //                 근거리, 원거리
    public enum Type { Melee, Range };
    public Type type;  // 무기 타입
    public int damage;
    public float rate;  //  공격속도
    public int maxAmmo; // 최대탄창;
    public int curAmmo; // 현재탄약;

    public BoxCollider meleeArea;  // 범위
    public TrailRenderer trailEffect;  // 휘두를 때 효과

    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.16f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.35f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        //1. 총알발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;

        yield return null;
        //2. 탄피배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);   //AddTorque(): 회전함수
    }

    //일반함수:  Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴
    //코루틴함수: Use() 메인루틴 + Swing() 코루틴 >> 동시에 실행
    // yield: 결과를 전달하는 키워드, 여러개 사용하여 시간차 로직 작성가능
    // yield break로 코루틴 탈출 가능
}
