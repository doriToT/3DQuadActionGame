using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades; // �����ϴ� ��ü�� ��Ʈ���ϱ� ���� �迭����
    public int hasGrenades;
    public GameObject grenadeObj;
    public Camera followCamera;
    public GameManager manager;


    public int ammo;  //�Ѿ�
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
    bool fireDown;  // ����
    bool grenadeDown;
    bool reloadDown;
    bool itemDown;   // eŰ�� ���� ������ �Լ��ϱ� ���� ����
    bool swapDown1;
    bool swapDown2;
    bool swapDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;  // ��ü �ð����� ���� ����
    bool isReload;
    bool isFireReady = true;
    bool isBorder;   // �� �浹 �÷��� ����
    bool isDamage;  // ������ ���� �� ����Ÿ�� ����
    bool isShop;
    bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;    // ȸ�ǵ��� ������ȯ�� ���� �ʵ��� ȸ�ǹ��� Vector3 �߰�

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;  // ������ ������ ���⸦ �����ϴ� ����
    int equipWeaponIndex = -1;  // ��ġ�� value�� 0�̱� ������ -1�� �ʱ�ȭ
    float fireDelay;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        //Debug.Log(PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", 120);    // PlayerPrefs: ����Ƽ���� �����ϴ� ������ ������
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
        horizonAxis = Input.GetAxisRaw("Horizontal");   // GetAxisRaw(): Axis ���� ������ ��ȯ�ϴ� �Լ�
        verticalAxis = Input.GetAxisRaw("Vertical");
        walkDown = Input.GetButton("Walk");  // project setting�� InputManager���� �߰����ش�. left shiftŰ�� ������ Ȱ��ȭ
        jumpDown = Input.GetButtonDown("Jump");
        fireDown = Input.GetButton("Fire1"); // Fire1�� ���콺 ����Ŭ��
        grenadeDown = Input.GetButtonDown("Fire2"); // Fire2�� ���콺 ������Ŭ��
        reloadDown = Input.GetButtonDown("Reload");
        itemDown = Input.GetButtonDown("Interation"); // project setting�� InputManager�� �߰����ش�. e Ű�� ������ Ȱ��ȭ �ȴ�.
        swapDown1 = Input.GetButtonDown("Swap1");  // project setting�� InputManager�� �߰����ش�.
        swapDown2 = Input.GetButtonDown("Swap2");
        swapDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(horizonAxis, 0, verticalAxis).normalized;   //normalized: ���� ���� 1�� ������ ����

        if (isDodge)
            moveVec = dodgeVec; // ȸ�� �߿��� ������ ���� -> ȸ�ǹ��� �ٲ�� ����

        // ���� ������, ������, �����߿��� ������ ����
        if (isSwap || isReload || !isFireReady || isDead)
            moveVec = Vector3.zero;

         // transform �̵��� �� Time.deltaTime ���� ���ؾ��Ѵ�.
         if(!isBorder)
            transform.position += moveVec * speed * (walkDown ? 0.3f : 1f) * Time.deltaTime;   // << bool ���� ���� ? true �϶� ��: false �� �� ��

        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", walkDown);
    }

    void Turn()
    {
        // Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec); // LookAt(): ������ ���͸� ���ؼ� ȸ�������ִ� �Լ�

        // ���콺�� ���� ȸ��
        if(fireDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);  // ScreenPointToRay(): ��ũ������ ����� Ray�� ��� �Լ�
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) // out: returnó�� ��ȯ���� �־��� ������ �����ϴ� Ű����
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // RayCastHit�� ���̴� �����ϵ��� ����
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if(jumpDown && moveVec == Vector3.zero && !isJump && !isDodge && !isDead)// !isDodge ���� �߰��ϸ� �����ϸ鼭 ȸ�Ǹ��ϰ�
        {
            rigid.AddForce(Vector3.up * 20, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Grenade()
    {
        // ����ź ���� ������ ��������
        if (hasGrenades == 0)
            return;

        if (grenadeDown && !isReload && !isSwap && !isDead)
        {
            // ���콺 ��ġ�� �ٷ� ���� �� �ֵ��� RayCast���
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) 
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10; // RayCastHit�� ���̴� �����ϵ��� ����

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                //������ ����ź�� Rigidbody�� Ȱ���Ͽ� ������.
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);  //ȸ��

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
            equipWeapon.Use(); // ������ �����Ǹ� ���⿡ �ִ� �Լ� ����
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot"); // ��������
            fireDelay = 0;  // ���ݵ����� 0���� ������ ���� ���ݱ��� ��ٸ���
        }
    }
     
    // �Ѿ� ����
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
        // �÷��̾ ������ ź(reAmmo)�� ����Ͽ� ���
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = equipWeapon.maxAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    // ȸ���ϴ� �Լ�
    void Dodge()
    {
        if (jumpDown && moveVec != Vector3.zero && !isJump && !isDodge && !isShop && !isSwap && !isDead)
        {
            dodgeVec = moveVec; 
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            // �ð��� �Լ� Invoke
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
        // ���� �ߺ� ��ü, ���� ���� Ȯ���� ���� ����
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
            // ����� �� ����
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

    // �÷��̾� ��ó�� ��ȣ�ۿ�Ǵ� item�� �ִٸ� �����ϴ� �Լ�
    void Interation()
    {
        if(itemDown && nearObject != null && !isJump && !isDodge && !isShop && !isDead)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                // ������ ������ ������ �ش� ���⸦ �Լ�
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;    // �ش� ���� �Լ�Ȯ��

                Destroy(nearObject);
            }
            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);  // this = Player �ڱ��ڽ� ������ �� this ���
                isShop = true;
            }
        }
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero; // angularVelocity: ���� ȸ�� �ӵ�

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

    // ���� ����
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
            // �̻��ϸ� rigidbody�� �ֱ⶧���� rigidbody ������������  Destroy() ȣ��
            // �÷��̾� ����Ÿ�Ӱ� ������� Destroy()�ǵ��� if�� �ٱ���
            if (other.GetComponent<Rigidbody>() != null)
            {
                Destroy(other.gameObject);
            }
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;  // ������ �޾Ҵٸ� 1�ʹ����ð�
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        // ���� �������� �˹�
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

        // ���� �������� �˹� �� �ǵ�����
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

// Collision Dection > Continuous�� Static�� �浹�� �� ȿ�����̴�.