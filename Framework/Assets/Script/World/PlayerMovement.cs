using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerMovement : LivingObject
{
    public float h;
    public float v;

    // Hands Left 0, Right 1
    bool isMove = false;
    public float cooldownTime = 0.5f;
    private bool isCooldown = false;
    
    public GameObject Hands;
    public GameObject Weapons;
    public ObjectState objectState;

    GameObject scanObject;
    Animator WeaponsAnimator;

    public Transform WeaponTransform; // 총의 Transform 컴포넌트
    public Transform shotpoint; // 총의 Transform 컴포넌트
    public GameObject bulletPrefab; // 총알 프리팹
    public float bulletSpeed = 10f; // 총알 발사 속도
    Weapon weaponData;

    protected override void Awake()
    {
        base.Awake(); // LivingObject의 Awake 메서드 호출
        WeaponsAnimator = Weapons.GetComponent<Animator>();

        weaponData = new Weapon();
    }
     void Start()
    {
        playerData = gameManager.GetPlayerData();
        maxHealth = playerData.health; // 최대 체력 설정
        health = maxHealth; // 현재 체력을 최대 체력으로 초기화
    }

    protected override void Update()
    {
        
        base.Update();

        if (!UIManager.Instance.isUserInterface)
        {
            float angle = CalculateMouseAngle();
            Hands.gameObject.SetActive(true);
            ShootInput();
            if (Input.GetKeyDown(KeyCode.R))
            {
                WeaponsAnimator.SetTrigger("Reload");
            }

            //Debug.Log("현재 각도: " + angle);
            if (Input.GetMouseButtonDown(1) && !isCooldown && isMove && objectState != ObjectState.Roll)
            {
                // 우클릭 입력이 감지되면 처리하고 쿨타임 시작
                StartCooldown();
                StartCoroutine(Roll());
            }

            // 쿨타임 감소
            if (isCooldown)
            {
                cooldownTime -= Time.deltaTime;

                // 쿨타임이 끝나면 상태 초기화
                if (cooldownTime <= 0)
                {
                    isCooldown = false;
                    cooldownTime = 0.75f; // 원하는 쿨타임 값으로 설정
                    objectState = ObjectState.None;
                }
            }

            if (objectState != ObjectState.Roll)//구르기는 각도처리를 따로해서
            {
                animator.SetBool("IsUp", false);
                animator.SetBool("IsSide", false);
                animator.SetBool("IsDown", false);
                animator.SetBool("IsAngle", false);

                if (angle > -45f && angle <= 15f)
                {
                    Vector3 currentScale = transform.localScale;
                    currentScale.x = Mathf.Abs(currentScale.x) * 1;
                    transform.localScale = currentScale;

                    SetObjectState(ObjectState.Side);
                }
                else if (angle > 45f && angle <= 135f)
                {
                    SetObjectState(ObjectState.Up);
                }
                else if ((angle > 165f && angle <= 180f) || (angle >= -180f && angle < -135f))
                {
                    Vector3 currentScale = transform.localScale;
                    currentScale.x = Mathf.Abs(currentScale.x) * -1;
                    transform.localScale = currentScale;
                    SetObjectState(ObjectState.Side);
                }
                else if (angle >= -135f && angle < -45f)
                {
                    SetObjectState(ObjectState.Down);
                }
                else if (angle < 45f && angle >= 15f)
                {
                    Vector3 currentScale = transform.localScale;
                    currentScale.x = Mathf.Abs(currentScale.x) * 1;
                    transform.localScale = currentScale;

                    SetObjectState(ObjectState.Angle);
                }
                else
                {
                    Vector3 currentScale = transform.localScale;
                    currentScale.x = Mathf.Abs(currentScale.x) * -1;
                    transform.localScale = currentScale;

                    SetObjectState(ObjectState.Angle);
                }


            }
            else
            {
                Hands.gameObject.SetActive(false);
            }
        }
    }
    void ShootInput()
    {
        weaponData = gameManager.GetWeaponData();
        int current_magazine = weaponData.current_magazine;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - WeaponTransform.position).normalized;
        WeaponTransform.up = direction;

        // 마우스 왼쪽 버튼을 클릭하면 총알을 발사합니다.
        if (Input.GetMouseButtonDown(0) && current_magazine > 0 && weaponData.current_Ammo > 0)
        {
            Shoot();
            weaponData.current_magazine -= 1;
            weaponData.current_Ammo -= 1;
            gameManager.SaveWeaponData(weaponData);
        }

        if (weaponData.current_Ammo < weaponData.maxAmmo &&
            Input.GetKeyDown(KeyCode.R) &&
            weaponData.current_magazine < weaponData.magazine)
        {
            weaponData.current_magazine = weaponData.magazine;
            gameManager.SaveWeaponData(weaponData);
        }
    }
    void Shoot()
    {
        // 총알을 생성하고 초기 위치를 총의 위치로 설정합니다.
        GameObject bullet = Instantiate(bulletPrefab, shotpoint.position, WeaponTransform.rotation);


        // 총알에 속도를 적용하여 발사합니다.
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = WeaponTransform.up * bulletSpeed;
    }
    IEnumerator Roll()
    {
        objectState = ObjectState.Roll;

        animator.SetBool("IsUp", false);
        animator.SetBool("IsSide", false);
        animator.SetBool("IsDown", false);
        animator.SetBool("IsAngle", false);

        // 키 입력에 따른 이동 방향
        Vector2 rollDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

        // 구르기 애니메이션 재생 등의 초기화
        // Your animation or other initialization code here
        float vir = rollDirection.normalized.x;
        float hir = rollDirection.normalized.y;

        Vector3 currentScale = transform.localScale;
        if (vir != 0 && hir == 0) // 왼쪽 또는 오른쪽으로 이동할 때
        {
            if (vir < 0) // 왼쪽 이동
            {
                currentScale.x = Mathf.Abs(currentScale.x) * -1;// 스케일 반대로 설정
            }
            else // 오른쪽 이동
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1;//  스케일 그대로 설정
            }
            animator.SetBool("IsSide", true);
        }
        else if (vir == 0 && hir != 0) // 위 또는 아래로 이동할 때
        {
            animator.SetBool("IsUp", hir > 0);
            animator.SetBool("IsDown", hir < 0);
        }
        else // 대각선 이동일 때
        {
            if (hir > 0 && vir < 0) // 왼쪽 위 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * -1;// 스케일 반대로 설정
                animator.SetBool("IsAngle", true);
            }
            else if (hir > 0 && vir > 0) // 오른쪽 위 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1;//  스케일 그대로 설정
                animator.SetBool("IsAngle", true);
            }
            else if (hir < 0 && vir < 0) // 왼쪽 아래 대각선
            {
                animator.SetBool("IsSide", true);
                currentScale.x = Mathf.Abs(currentScale.x) * -1;// 스케일 반대로 설정
            }
            else if (hir < 0 && vir > 0) // 오른쪽 아래 대각선
            {
                animator.SetBool("IsSide", true);
                currentScale.x = Mathf.Abs(currentScale.x) * 1;//  스케일 그대로 설정
            }

            //왼쪽 대각선 아래면 스케일반대로, 오른쪽 아래 대각선이면 그대로 
        }
        transform.localScale = currentScale;
        animator.SetTrigger("IsRoll");
        yield return null;
    }
    void FixedUpdate()
    {
        if (objectState != ObjectState.Roll && !UIManager.Instance.isUserInterface)
            Move();
        else if(objectState == ObjectState.Roll && !UIManager.Instance.isUserInterface)
        
                    rigid.velocity = new Vector2(h, v) * speed * 1.5f;

    }
    void Move()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if (h != animator.GetInteger("h"))
        {
            isMove = true;
            animator.SetInteger("h", (int)h);

        }
        else if (v != animator.GetInteger("v"))
        {
            isMove = true;
            animator.SetInteger("v", (int)v);
        }

        if (v == 0 && h == 0)
        {
            isMove = false;
        }
        animator.SetBool("isMove", isMove);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = 6f;
        }
        else
        {
            speed = 4f;
        }
        if (gameManager.GetPlayerData().currentState == GameState.Event)
        {
            animator.SetInteger("v", 0);
            animator.SetInteger("h", 0);
            speed = 0f;
        }
        rigid.velocity = new Vector2(h, v) * speed;
        playerData.position = transform.position;
        playerData.health = health;
    }
    void StartCooldown()
    {
        // 쿨타임 시작
        isCooldown = true;
    }
    void SetObjectState(ObjectState newState)
    {

        // 각 상태에 따른 초기화 등의 로직을 추가할 수 있습니다.
        switch (newState)
        {
            case ObjectState.Angle:
                // Walk 상태에 대한 초기화 로직
                animator.SetBool("IsAngle", true);
                break;
            case ObjectState.Down:
                // Idle 상태에 대한 초기화 로직
                animator.SetBool("IsDown", true);
                break;
            case ObjectState.Side:
                // Roll 상태에 대한 초기화 로직
                animator.SetBool("IsSide", true);
                break;
            case ObjectState.Up:
                // None 상태에 대한 초기화 로직
                animator.SetBool("IsUp", true);
                break;
            default:
                break;
        }
    }
    float CalculateMouseAngle()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 direction = targetPosition - transform.position;


        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    void UpdateAnimation(Vector2 moveInput)
    {
        // moveInput에 따라 애니메이션을 변경합니다.
        if (moveInput.magnitude > 0)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
        }
        else
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
        }
    }
}
