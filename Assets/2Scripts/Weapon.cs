using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //                 �ٰŸ�, ���Ÿ�
    public enum Type { Melee, Range };
    public Type type;  // ���� Ÿ��
    public int damage;
    public float rate;  //  ���ݼӵ�
    public int maxAmmo; // �ִ�źâ;
    public int curAmmo; // ����ź��;

    public BoxCollider meleeArea;  // ����
    public TrailRenderer trailEffect;  // �ֵθ� �� ȿ��

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
        //1. �Ѿ˹߻�
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;

        yield return null;
        //2. ź�ǹ���
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);   //AddTorque(): ȸ���Լ�
    }

    //�Ϲ��Լ�:  Use() ���η�ƾ -> Swing() �����ƾ -> Use() ���η�ƾ
    //�ڷ�ƾ�Լ�: Use() ���η�ƾ + Swing() �ڷ�ƾ >> ���ÿ� ����
    // yield: ����� �����ϴ� Ű����, ������ ����Ͽ� �ð��� ���� �ۼ�����
    // yield break�� �ڷ�ƾ Ż�� ����
}
