using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades; // 공전하는 물체를 컨트롤하기 위한 배열변수
    public int hasGrenades;
    public GameObject grenadeObj;
    public Camera followCamera;
    public GameManager manager;


    public int ammo;  //총알
    public int coin;
    public int health;
    public int score;

    public int maxAmmo; 
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float horizonAxis;
    float verticalAxis;

    bool walkDown;
    bool jumpDown;
    bool fireDown;  // 공격
    bool grenadeDown;
    bool reloadDown;
    bool itemDown;   // e키를 눌러 아이템 입수하기 위한 변수
    bool swapDown1;
    bool swapDown2;
    bool swapDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;  // 교체 시간차를 위한 변수
    bool isReload;
    bool isFireReady = true;
    bool isBorder;   // 벽 충돌 플래그 변수
    bool isDamage;  // 데미지 받은 후 무적타임 변수
    bool isShop;
    bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;    // 회피도중 방향전환이 되지 않도록 회피방향 Vector3 추가

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;  // 기존에 장착된 무기를 저장하는 변수
    int equipWeaponIndex = -1;  // 망치의 value가 0이기 때문에 -1로 초기화
    float fireDelay;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        //Debug.Log(PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", 120);    // PlayerPrefs: 유니티에서 제공하는 간단한 저장기능
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interation();
    }

    void GetInput()
    {
        horizonAxis = Input.GetAxisRaw("Horizontal");   // GetAxisRaw(): Axis 값을 정수로 반환하는 함수
        verticalAxis = Input.GetAxisRaw("Vertical");
        walkDown = Input.GetButton("Walk");  // project setting의 InputManager에서 추가해준다. left shift키를 누르면 활성화
        jumpDown = Input.GetButtonDown("Jump");
        fireDown = Input.GetButton("Fire1"); // Fire1은 마우스 왼쪽클릭
        grenadeDown = Input.GetButtonDown("Fire2"); // Fire2은 마우스 오른쪽클릭
        reloadDown = Input.GetButtonDown("Reload");
        itemDown = Input.GetButtonDown("Interation"); // project setting의 InputManager에 추가해준다. e 키를 누르면 활성화 된다.
        swapDown1 = Input.GetButtonDown("Swap1");  // project setting의 InputManager에 추가해준다.
        swapDown2 = Input.GetButtonDown("Swap2");
        swapDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(horizonAxis, 0, verticalAxis).normalized;   //normalized: 방향 값이 1로 보정된 벡터

        if (isDodge)
            moveVec = dodgeVec; // 회피 중에는 움직임 벡터 -> 회피방향 바뀌도록 구현

        // 무기 스왑중, 공격중, 장전중에는 움직임 멈춤
        if (isSwap || isReload || !isFireReady || isDead)
            moveVec = Vector3.zero;

         // transform 이동은 꼭 Time.deltaTime 까지 곱해야한다.
         if(!isBorder)
            transform.position += moveVec * speed * (walkDown ? 0.3f : 1f) * Time.deltaTime;   // << bool 형태 조건 ? true 일때 값: false 일 때 값

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", walkDown);
    }

    void Turn()
    {
        // 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec); // LookAt(): 지정된 베터를 향해서 회전시켜주는 함수

        // 마우스에 의한 회전
        if(fireDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);  // ScreenPointToRay(): 스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) // out: return처럼 반환값을 주어진 변수에 저장하는 키워드
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // RayCastHit의 높이는 무시하도록 설정
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if(jumpDown && moveVec == Vector3.zero && !isJump && !isDodge && !isDead)// !isDodge 조건 추가하면 점프하면서 회피못하게
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Grenade()
    {
        // 슈류탄 쓰기 이전의 제한조건
        if (hasGrenades == 0)
            return;

        if (grenadeDown && !isReload && !isSwap && !isDead)
        {
            // 마우스 위치로 바로 던질 수 있도록 RayCast사용
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) 
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10; // RayCastHit의 높이는 무시하도록 설정

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                //생성된 슈류탄을 Rigidbody를 활용하여 던진다.
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);  //회전

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fireDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Use(); // 조건이 충족되면 무기에 있는 함수 실행
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot"); // 근접공격
            fireDelay = 0;  // 공격딜레이 0으로 돌려서 다음 공격까지 기다리기
        }
    }
     
    // 총알 장전
    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if (reloadDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead)
        {
            anim.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2f);

        }
    }

    void ReloadOut()
    {
        // 플레이어가 소지한 탄(reAmmo)을 고려하여 계산
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = equipWeapon.maxAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    // 회피하는 함수
    void Dodge()
    {
        if (jumpDown && moveVec != Vector3.zero && !isJump && !isDodge && !isShop && !isSwap && !isDead)
        {
            dodgeVec = moveVec; 
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            // 시간차 함수 Invoke
            Invoke("DodgeOut", 0.5f);
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

    void Swap()
    {
        // 무기 중복 교체, 없는 무기 확인을 위한 조건
        if (swapDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (swapDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (swapDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (swapDown1) weaponIndex = 0;
        if (swapDown2) weaponIndex = 1;
        if (swapDown3) weaponIndex = 2;

        if ((swapDown1 || swapDown2 || swapDown3) && !isJump && !isDodge && !isShop && !isDead)
        {
            // 빈손일 때 실행
            if(equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }

    }

    void SwapOut()
    {
        isSwap = false;
    }

    // 플레이어 근처에 상호작용되는 item이 있다면 실행하는 함수
    void Interation()
    {
        if(itemDown && nearObject != null && !isJump && !isDodge && !isShop && !isDead)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                // 아이템 정보를 가져와 해당 무기를 입수
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;    // 해당 무기 입수확인

                Destroy(nearObject);
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);  // this = Player 자기자신 접근할 때 this 사용
                isShop = true;
            }
        }
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero; // angularVelocity: 물리 회전 속도

    }
    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    // 착지 구현
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
             
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk));

            }
            // 미사일만 rigidbody가 있기때문에 rigidbody 유무조건으로  Destroy() 호출
            // 플레이어 무적타임과 관계없이 Destroy()되도록 if문 바깥에
            if (other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;  // 데미지 받았다면 1초무적시간
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        // 보스 점프공격 넉백
        if (isBossAtk)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if (health <= 0 && !isDead)
        {
            OnDie();
        }
        yield return new WaitForSeconds(1f);

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        // 보스 점프공격 넉백 후 되돌리기
        if (isBossAtk)
            rigid.velocity = Vector3.zero;


    }

    void OnDie()
    {
        anim.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;

    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
        else if (other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }
}

// Collision Dection > Continuous는 Static과 충돌할 때 효과적이다.