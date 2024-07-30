using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : LivingObject
{
    // 공개 변수
    public float h;
    public float v;

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

    private Vector2 rollDirection;
    private float rollSpeed = 8f; // 구르기 속도
    private float rollDuration = 0.5f; // 구르기 지속 시간
    private float rollTime; // 구르기 시간

    // Awake 메서드: 초기 설정
    protected override void Awake()
    {
        base.Awake(); // LivingObject의 Awake 메서드 호출
        WeaponsAnimator = Weapons.GetComponent<Animator>();
        weaponData = new Weapon();
    }

    // Start 메서드: 게임 시작 시 초기화
    void Start()
    {
        playerData = gameManager.GetPlayerData();
        maxHealth = playerData.health; // 최대 체력 설정
        health = maxHealth; // 현재 체력을 최대 체력으로 초기화

        SoundManager.Instance.BGSoundPlay(); // 배경음악 재생
    }

    // Update 메서드: 매 프레임마다 호출
    protected override void Update()
    {
        base.Update();

        if (!UIManager.Instance.isUserInterface)
        {
            float angle = CalculateMouseAngle();
            Hands.gameObject.SetActive(true);

            // R키 입력 시 재장전 
            if (Input.GetKeyDown(KeyCode.R))
            {
                WeaponsAnimator.SetTrigger("Reload");
                SoundManager.Instance.SFXPlay("mus_piano1", 32); // 재장전 사운드
            }

            // 우클릭 입력 시 구르기 시작
            if (Input.GetMouseButtonDown(1) && !isCooldown && isMove && objectState != ObjectState.Roll)
            {
                StartCooldown();
                StartCoroutine(Roll());
            }

            // 쿨다운 시간 감소
            if (isCooldown)
            {
                cooldownTime -= Time.deltaTime;
                if (cooldownTime <= 0)
                {
                    isCooldown = false;
                    cooldownTime = 0.75f;
                    objectState = ObjectState.None;
                }
            }

            // 구르기 상태가 아닐 때 각도에 따른 상태 설정
            if (objectState != ObjectState.Roll)
            {
                ShootInput();
                SetAnimatorBooleansFalse();
                HandleObjectState(angle);
            }
            else
            {
                Hands.gameObject.SetActive(false);
            }
        }
    }

    // 애니메이터 활성화/비활성화
    public void SetAnimatorEnabled(bool isEnabled)
    {
        animator.enabled = isEnabled;
    }

    // 총알 발사 입력 처리
    void ShootInput()
    {
        weaponData = gameManager.GetWeaponData();
        int current_magazine = weaponData.current_magazine;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - WeaponTransform.position).normalized;
        WeaponTransform.up = direction;

        if (Input.GetMouseButtonDown(0) && current_magazine > 0 && weaponData.current_Ammo > 0)
        {
            Shoot();
            weaponData.current_magazine -= 1;
            weaponData.current_Ammo -= 1;
            gameManager.SaveWeaponData(weaponData);
        }

        if (weaponData.current_Ammo < weaponData.maxAmmo && Input.GetKeyDown(KeyCode.R) && weaponData.current_magazine < weaponData.magazine)
        {
            weaponData.current_magazine = weaponData.magazine;
            gameManager.SaveWeaponData(weaponData);
        }
    }

    // 총알 발사
    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, shotpoint.position, WeaponTransform.rotation);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = WeaponTransform.up * bulletSpeed;
        SoundManager.Instance.SFXPlay("mus_piano1", 32); // 총 사운드
    }

    // 구르기 코루틴
    IEnumerator Roll()
    {
        objectState = ObjectState.Roll;
        SetAnimatorBooleansFalse();
        rollDirection = new Vector2(Mathf.Sign(Input.GetAxis("Horizontal")), Mathf.Sign(Input.GetAxis("Vertical"))).normalized;
        rollTime = 0;
        HandleRollAnimation(rollDirection);
        animator.SetTrigger("IsRoll");
        SoundManager.Instance.SFXPlay("mus_piano1", 32); // 구르기 사운드

        while (rollTime < rollDuration)
        {
            rollTime += Time.deltaTime;
            float t = rollTime / rollDuration;
            rigid.velocity = rollDirection * rollSpeed * (1f - t); // 점점 속도가 줄어듦
            yield return null;
        }

        rigid.velocity = Vector2.zero;
        objectState = ObjectState.None;
    }

    // 물리 업데이트
    void FixedUpdate()
    {
        if (objectState != ObjectState.Roll && !UIManager.Instance.isUserInterface)
        {
            Move();
        }
        else if (objectState == ObjectState.Roll && !UIManager.Instance.isUserInterface)
        {
            rigid.velocity = rollDirection * rollSpeed;
        }
        else
        {
            rigid.velocity = Vector2.zero;
        }
    }
    private void HandleRollAnimation(Vector2 rollDirection)
    {
        float vir = rollDirection.normalized.x;
        float hir = rollDirection.normalized.y;

        Vector3 currentScale = transform.localScale;
        if (vir != 0 && hir == 0) // 왼쪽 또는 오른쪽으로 이동할 때
        {
            if (vir < 0) // 왼쪽 이동
            {
                currentScale.x = Mathf.Abs(currentScale.x) * -1; // 스케일 반대로 설정
            }
            else // 오른쪽 이동
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1; // 스케일 그대로 설정
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
                currentScale.x = Mathf.Abs(currentScale.x) * -1; // 스케일 반대로 설정
                animator.SetBool("IsAngle", true);
            }
            else if (hir > 0 && vir > 0) // 오른쪽 위 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1; // 스케일 그대로 설정
                animator.SetBool("IsAngle", true);
            }
            else if (hir < 0 && vir < 0) // 왼쪽 아래 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * -1; // 스케일 반대로 설정
                animator.SetBool("IsSide", true);
            }
            else if (hir < 0 && vir > 0) // 오른쪽 아래 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1; // 스케일 그대로 설정
                animator.SetBool("IsSide", true);
            }
        }
        transform.localScale = currentScale;
        animator.SetTrigger("IsRoll");
    }
    // 플레이어 이동 처리
    void Move()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        UpdateAnimatorMovement();

        if (Input.GetKey(KeyCode.LeftShift))
            speed = 6f;
        else
            speed = 4f;

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

    // 애니메이터 이동 업데이트
    void UpdateAnimatorMovement()
    {
        isMove = h != 0 || v != 0;
        animator.SetBool("isMove", isMove);

        if (h != animator.GetInteger("h"))
            animator.SetInteger("h", (int)h);
        else if (v != animator.GetInteger("v"))
            animator.SetInteger("v", (int)v);
    }

    // 쿨타임 시작
    void StartCooldown()
    {
        isCooldown = true;
    }

    // 오브젝트 상태 설정
    void SetObjectState(ObjectState newState)
    {
        switch (newState)
        {
            case ObjectState.Angle:
                animator.SetBool("IsAngle", true);
                break;
            case ObjectState.Down:
                animator.SetBool("IsDown", true);
                break;
            case ObjectState.Side:
                animator.SetBool("IsSide", true);
                break;
            case ObjectState.Up:
                animator.SetBool("IsUp", true);
                break;
            default:
                break;
        }
    }

    // 마우스 각도 계산
    float CalculateMouseAngle()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 direction = targetPosition - transform.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    // 애니메이터의 모든 불린 값 초기화
    void SetAnimatorBooleansFalse()
    {
        animator.SetBool("IsUp", false);
        animator.SetBool("IsSide", false);
        animator.SetBool("IsDown", false);
        animator.SetBool("IsAngle", false);
    }

    // 오브젝트 상태 처리
    void HandleObjectState(float angle)
    {
        SetAnimatorBooleansFalse();
        if (angle > -45f && angle <= 15f)
        {
            FlipCharacter(1);
            SetObjectState(ObjectState.Side);
        }
        else if (angle > 45f && angle <= 135f)
        {
            SetObjectState(ObjectState.Up);
        }
        else if ((angle > 165f && angle <= 180f) || (angle >= -180f && angle < -135f))
        {
            FlipCharacter(-1);
            SetObjectState(ObjectState.Side);
        }
        else if (angle >= -135f && angle < -45f)
        {
            SetObjectState(ObjectState.Down);
        }
        else
        {
            FlipCharacter(angle >= 15f ? 1 : -1);
            SetObjectState(ObjectState.Angle);
        }
    }

    // 캐릭터 뒤집기
    void FlipCharacter(int direction)
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * direction;
        transform.localScale = currentScale;
    }

    // 애니메이션 업데이트
    void UpdateAnimation(Vector2 moveInput)
    {
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
