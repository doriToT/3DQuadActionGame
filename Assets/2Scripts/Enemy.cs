using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };   // 몬스터 타입 A, B, C, D / D는 보스몬스터
    public Type enemyType;

    public int maxHealth;
    public int curHealth;
    public int score;

    public GameManager manager;
    public Transform target;
    public BoxCollider melleeArea; // 몬스터 근접공격범위
    public GameObject bullet;  // C, D 몬스터 원거리공격 변수
    public GameObject[] coins;

    public bool isChase;   // 추적을 결정하는 bool 변수
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] mashs;
    // NavMesh: NavAgent가 경로를 그리기 위한 바탕 / 또한 Window->AI->Bake를 꼭 눌러야한다.
    // NavMesh는 Static 오브젝트만 Bake 가능
    public NavMeshAgent nav;
    public Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mashs = GetComponentsInChildren<MeshRenderer>(); // Material은 MeshRenderer 컴포넌트에서 접근 가능
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
            nav.SetDestination(target.position);   // SetDestination(): 도착할 목표위치 지정함수
            nav.isStopped = !isChase;  // isStopped를 사용하여 완뱍하게 멈추도록

        }
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; // angularVelocity: 물리 회전 속도
        }
    }

    void Targeting()
    {
        if(!isDead && enemyType != Type.D)
        {
            float targetRadius = 0f;  // 두께
            float targetRange = 0f;  // 공격범위

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

            // rayHit 변수에 데이터가 들어오면 공격 코루틴실행
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
        // 근접공격
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec, false));

        }
        else if (other.tag == "Bullet") // 원거리공격
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
            mesh.material.color = Color.red;   // 피격시 빨간색으로 나옴

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
                mesh.material.color = Color.gray;  // 사망시 회색으로 바뀜

            gameObject.layer = 14;  // 설정한 EnemyDead 레이어 숫자입력
            isDead = true;
            isChase = false;
            // 사망 리액션을 유지하기 위해 NavAgent 비활성화
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
                // 넉백 구현하기 위한 reactVec
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                // 슈류탄에 의한 사망 리액션은 큰 힘과 회전 추가
                rigid.freezeRotation = false;   
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);    // 넉백 구현
            }

             Destroy(gameObject, 4);
        }
    }
}

// Nav관련 클래스는 UnityEngine.AI 네임스페이스 필요
//NavMeshAgent: Navigation을 사용하는 인공지능 컴포넌트
