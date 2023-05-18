using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };   // ���� Ÿ�� A, B, C, D / D�� ��������
    public Type enemyType;

    public int maxHealth;
    public int curHealth;
    public int score;

    public GameManager manager;
    public Transform target;
    public BoxCollider melleeArea; // ���� �������ݹ���
    public GameObject bullet;  // C, D ���� ���Ÿ����� ����
    public GameObject[] coins;

    public bool isChase;   // ������ �����ϴ� bool ����
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] mashs;
    // NavMesh: NavAgent�� ��θ� �׸��� ���� ���� / ���� Window->AI->Bake�� �� �������Ѵ�.
    // NavMesh�� Static ������Ʈ�� Bake ����
    public NavMeshAgent nav;
    public Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mashs = GetComponentsInChildren<MeshRenderer>(); // Material�� MeshRenderer ������Ʈ���� ���� ����
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }


    void Update()
    {
        if(nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);   // SetDestination(): ������ ��ǥ��ġ �����Լ�
            nav.isStopped = !isChase;  // isStopped�� ����Ͽ� �Ϲ��ϰ� ���ߵ���

        }
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; // angularVelocity: ���� ȸ�� �ӵ�
        }
    }

    void Targeting()
    {
        if(!isDead && enemyType != Type.D)
        {
            float targetRadius = 0f;  // �β�
            float targetRange = 0f;  // ���ݹ���

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits =
                Physics.SphereCastAll(transform.position,
                                      targetRadius,
                                      transform.forward,
                                      targetRange,
                                      LayerMask.GetMask("Player"));

            // rayHit ������ �����Ͱ� ������ ���� �ڷ�ƾ����
            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }

    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch(enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                melleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                melleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                melleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                melleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantbullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidbullet = instantbullet.GetComponent<Rigidbody>();
                rigidbullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }
    private void OnTriggerEnter(Collider other)
    {
        // ��������
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec, false));

        }
        else if (other.tag == "Bullet") // ���Ÿ�����
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec, false));

        }
    }
    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        foreach(MeshRenderer mesh in mashs)
            mesh.material.color = Color.red;   // �ǰݽ� ���������� ����

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            yield return new WaitForSeconds(0.1f);

            foreach (MeshRenderer mesh in mashs)
                mesh.material.color = Color.white;

            yield return new WaitForSeconds(0.1f);
        }
        else
        {
            foreach (MeshRenderer mesh in mashs)
                mesh.material.color = Color.gray;  // ����� ȸ������ �ٲ�

            gameObject.layer = 14;  // ������ EnemyDead ���̾� �����Է�
            isDead = true;
            isChase = false;
            // ��� ���׼��� �����ϱ� ���� NavAgent ��Ȱ��ȭ
            nav.enabled = false;
            anim.SetTrigger("doDie");
            Player player = target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch(enemyType)
            {
                case Type.A:
                    manager.enemyCntA--;
                    break;
                case Type.B:
                    manager.enemyCntB--;
                    break;
                case Type.C:
                    manager.enemyCntC--;
                    break;
                case Type.D:
                    manager.enemyCntD--;
                    break;
            }

            if (isGrenade)
            {
                // �˹� �����ϱ� ���� reactVec
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                // ����ź�� ���� ��� ���׼��� ū ���� ȸ�� �߰�
                rigid.freezeRotation = false;   
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);    // �˹� ����
            }

             Destroy(gameObject, 4);
        }
    }
}

// Nav���� Ŭ������ UnityEngine.AI ���ӽ����̽� �ʿ�
//NavMeshAgent: Navigation�� ����ϴ� �ΰ����� ������Ʈ
