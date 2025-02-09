using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Color 값
//Determination 의지, 불명(빨강) : 255 0 0
//Patience 인내(하늘) : 66 252 255
//Bravery 용기(주황) : 252 166 0
//Integrity 고결(파랑) : 0 60 255
//Perseverance 끈기(보라) : 213 53 217
//Kindness 친절(초록) : 0 192 0
//Justice 정의(노랑) : 255 255 0
public enum WeaponType
{
    Determination, // 의지 (빨강)
    Patience,      // 인내 (하늘)
    Bravery,       // 용기 (주황)
    Integrity,     // 고결 (파랑)
    Perseverance,  // 끈기 (보라)
    Kindness,      // 친절 (초록)
    Justice,       // 정의 (노랑)
    None           // 기본값
}
// 각 총의 특성을 나타내는 클래스
[System.Serializable]
public class Weapon
{
    public int id;             // 총의 고유한 ID
    public string WeaponName;  // 총의 이름
    public int damage;         // 총의 공격력
    public int current_Ammo;   // 현재 탄알집에 남아있는 총알 수
    public int magazine;       // 탄창 최대 총알수  
    public int current_magazine; // 현재 남아있는 총알 수
    public int maxAmmo;        // 최대 총알 수
    public int maxRange;       // 사거리
    public float bulletSpeed;  // 총알 속도
    public float accuracy;     // 총의 정확도
    public Transform firePoint; // 총알이 발사될 위치
    public WeaponType weaponType;// 무기 타입
    public Color weaponColor;    // 무기 색상
    public Weapon()
    {
        // 초기화 로직 추가 (예: 기본값 설정)
        id = 0;
        WeaponName = "None";
        damage = 1;
        maxAmmo = -1;
        current_Ammo = maxAmmo;
        magazine = 6;
        current_magazine = magazine;
        bulletSpeed = 1;
        accuracy = 1;
        weaponType = WeaponType.None;
        // 추가 데이터 초기화
    }

    // 무한 총알 상태 확인 메소드
    public bool IsInfiniteAmmo()
    {
        return current_Ammo == -1;  // Ammo가 -1이면 무한으로 간주
    }
    // WeaponType에 따라 색상을 반환하는 메서드
    public Color GetColor(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Determination:
                return new Color(1f, 0f, 0f); // 빨강
            case WeaponType.Patience:
                return new Color(66f / 255f, 252f / 255f, 255f / 255f); // 하늘색
            case WeaponType.Bravery:
                return new Color(252f / 255f, 166f / 255f, 0f); // 주황
            case WeaponType.Integrity:
                return new Color(0f, 60f / 255f, 1f); // 파랑
            case WeaponType.Perseverance:
                return new Color(213f / 255f, 53f / 255f, 217f / 255f); // 보라
            case WeaponType.Kindness:
                return new Color(0f, 192f / 255f, 0f); // 초록
            case WeaponType.Justice:
                return new Color(1f, 1f, 0f); // 노랑
            default:
                return new Color(0.5f, 0.5f, 0.5f); // 기본 회색
        }
    }

    // 무기 타입이 변경되었을 때 색상을 업데이트하는 메서드
    public void UpdateColor()
    {
        weaponColor = GetColor(weaponType);
    }
}

public class PlayerMovement : LivingObject
{
    // 공개 변수
    public float h;
    public float v;

    bool isMove = false;
    private bool isTouchingHorizontal = false; // 좌우 방향 충돌 상태
    private bool isTouchingVertical = false;   // 상하 방향 충돌 상태

    public float cooldownTime = 0.5f;
    private bool isCooldown = false;

    public GameObject Hands;
    public GameObject reloadPoint;
    public ObjectState objectState;

    GameObject scanObject;

    Animator WeaponsAnimator;
    public Transform WeaponTransform; // 총의 Transform 컴포넌트
    public Transform shotpoint;       // 총의 Transform 컴포넌트
    public Transform soulshotpoint;   // 영혼의 총 Transform 컴포넌트
    public GameObject bulletPrefab;   // 총알 프리팹
    public GameObject soulbulletPrefab; // 총알 프리팹
    public float bulletSpeed = 10f;   // 총알 발사 속도

    List<Weapon> playerWeapons = new List<Weapon>();
    private int currentWeaponIndex = 0;

    public Weapon curweaponData;
    public GameObject Weapons;

    //Determination 의지, 불명(빨강) : 255 0 0
    //Patience 인내(하늘) : 66 252 255
    //Bravery 용기(주황) : 252 166 0
    //Integrity 고결(파랑) : 0 60 255
    //Perseverance 끈기(보라) : 213 53 217
    //Kindness 친절(초록) : 0 192 0
    //Justice 정의(노랑) : 255 255 0

    public GameObject feetPoint;
    private bool isEffectSpawning = false; // 이펙트 생성 중 여부 확인 변수


    private Vector2 rollDirection;
    private float rollSpeed = 16f;      // 구르기 속도
    private float rollDuration = 0.5f;  // 구르기 지속 시간
    private float rollTime;             // 구르기 시간

    private bool isReloading = false;
    private float reloadTime = 1.5f;    // 재장전 시간 (초)

    public GameObject soulObject;       // Soul GameObject
    public GameObject playerSprite;     // Player의 스프라이트 (흐림 효과 적용)
    public GameObject shadowObject;     // Player의 스프라이트 (흐림 효과 적용)
    public float playerTransparency = 0f; // Player 투명도 값 (흐릿한 효과)

    private bool isSoulActive = false; // Soul 모드 활성화 여부

    public int walkingSoundStartIndex = 220; // 걷는 효과음 시작 인덱스
    public int walkingSoundCount = 3;       // 걷는 효과음 개수
    public float walkingSoundInterval = 0.3f; // 효과음 재생 간격
    private int currentSoundIndex = 0;      // 현재 재생 중인 효과음 인덱스
    private bool isPlayingFootsteps = false; // 효과음 재생 여부

    private Vector2 previousPosition;  // 이전 위치
    private float distanceCovered = 0f;  // 누적 이동 거리
    public float distanceThreshold = 1f; // 소리 및 이펙트 발생 거리 기준
    private const float positionTolerance = 0.01f; // 위치 변화 허용 오차 (벽 비빔 방지)
  
    #region unity_code
    // Awake 메서드: 초기 설정
    protected override void Awake()
    {
        base.Awake();
        WeaponsAnimator = Weapons.GetComponent<Animator>();
        animator = GetComponent<Animator>();
        curweaponData = new Weapon();

        // 시작할 때 Soul 비활성화
        soulObject.SetActive(false);
        previousPosition = transform.position; // 초기 위치 설정
    }

    // Start 메서드: 게임 시작 시 초기화
    void Start()
    {
        playerData = gameManager.GetPlayerData();
        maxHealth = playerData.Maxhealth; // 최대 체력 설정
        playerData.playerAnimator = animator;
        playerData.isInvincible = isInvincible;
        health = maxHealth; // 현재 체력을 최대 체력으로 초기화

        SoundManager.Instance.BGSoundPlay(); // 배경음악 재생
        OffHpbar();
        transform.position = playerData.position;
        playerData.player = transform.gameObject;

        curweaponData.weaponType = WeaponType.Justice;
        curweaponData.UpdateColor();
        UIManager.Instance.ui_weaponImage.GetComponent<Image>().color = curweaponData.weaponColor;
    }

    protected override void Update()
    {
        if (isDie)
            return;
        base.Update();

        SoulRotateToMouse();
        HandleWeaponSwitchInput();
        playerData.isInvincible = isInvincible;

        if (!UIManager.Instance.isUserInterface && !gameManager.GetPlayerData().isStop && !gameManager.GetPlayerData().isDie)
        {
            HandleSoulMode(); // Soul 모드 처리
            if (UIManager.Instance.reloadSlider != null)
            {
                UIManager.Instance.reloadSlider.transform.position = reloadPoint.transform.position;
            }

            float angle = CalculateMouseAngle();
            Hands.gameObject.SetActive(true);

            // R키 입력 시 재장전
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && curweaponData.current_magazine != curweaponData.magazine && !UIManager.Instance.isInventroy)
            {
                SoundManager.Instance.SFXPlay("shotgun_reload_01", 217, 0.05f); // 재장전 사운드
                StartCoroutine(Reload());
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
            if (objectState != ObjectState.Roll && !UIManager.Instance.isInventroy)
            {
                ShootInput();
                SetAnimatorBooleansFalse();
                HandleObjectState(angle);
            }
            else
            {
                Hands.gameObject.SetActive(false);
            }

            if (isSoulActive)
            {
                SyncSoulWithPlayer(); // 플레이어와 Soul의 위치 동기화
            }

            if (Input.GetKeyDown(KeyCode.C) &&
                !UIManager.Instance.savePanel.activeSelf
                && gameManager.GetPlayerData().currentState == GameState.None)
            {
                UIManager.Instance.SetTextBar();
                UIManager.Instance.ChangeInventroy();
            }
        }
        else
        {
            h = 0f;
            v = 0f;
        }
    }

    void FixedUpdate()
    {
        if (objectState != ObjectState.Roll &&
            !UIManager.Instance.isUserInterface &&
            !gameManager.GetPlayerData().isStop &&
            !UIManager.Instance.isInventroy &&
            !UIManager.Instance.savePanel.activeSelf &&
            !gameManager.GetPlayerData().isDie)
        {
            Move();
        }
        else if (objectState == ObjectState.Roll)
        {
            // Debug.Log("구른다");
        }
        else
        {
            animator.SetBool("isMove", false);
            rigid.velocity = Vector2.zero;
            h = 0;
            v = 0;
        }
        if (!gameManager.GetPlayerData().isStop && !isDie && !isSoulActive)
        {
            CalculateDistanceAndTriggerEffects();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector2 normal = contact.normal;

                if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y)) // 수평 충돌
                {
                    isTouchingHorizontal = true;
                }
                else if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x)) // 수직 충돌
                {
                    isTouchingVertical = true;
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector2 normal = contact.normal;

                if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y)) // 수평 충돌
                {
                    isTouchingHorizontal = false;
                }
                else if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x)) // 수직 충돌
                {
                    isTouchingVertical = false;
                }
            }
        }
    }
    #endregion

    #region soul_code
    // Soul 모드 처리
    private void HandleSoulMode()
    {
        if (Input.GetKeyDown(KeyCode.E)) // E 키로 Soul 모드 활성화/비활성화 전환
        {
            isSoulActive = !isSoulActive; // Soul 모드 전환

            if (isSoulActive)
            {
                EnableSoul();
            }
            else
            {
                DisableSoul();
            }
        }
    }

    // Soul 모드 활성화: 투명도 조절
    public void EnableSoul()
    {
        isSoulActive = true;
        soulObject.SetActive(true); // Soul 활성화
        SetTransparency(playerSprite, playerTransparency); // 플레이어를 흐리게
        SetTransparency(Hands, playerTransparency);
        SetTransparency(Weapons, playerTransparency);
        shadowObject.SetActive(false);
    }

    // Soul 모드 비활성화: 투명도 원상복귀
    private void DisableSoul()
    {
        soulObject.SetActive(false); // Soul 비활성화
        shadowObject.SetActive(true);
        SetTransparency(playerSprite, 1f); // 플레이어 투명도 복원
        SetTransparency(Hands, 1f);
        SetTransparency(Weapons, 1f);
    }

    // 투명도 설정 함수
    private void SetTransparency(GameObject obj, float alpha)
    {
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    // Soul과 플레이어의 위치 및 회전 동기화
    private void SyncSoulWithPlayer()
    {
        soulObject.transform.position = transform.position; // Soul을 플레이어와 같은 위치로 설정
    }

    void SoulRotateToMouse()
    {
        // 마우스의 스크린 좌표를 월드 좌표로 변환
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // 2D 게임이므로 Z 축을 0으로 고정

        // 오브젝트와 마우스 좌표 간의 방향 벡터 계산
        Vector3 direction = mousePosition - transform.position;

        // 방향 벡터로부터 각도 계산 (라디안 -> 각도)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 오브젝트 회전 적용
        soulObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
    }
    #endregion soul_code

    #region shot_code
    // 총알 발사 입력 처리
    IEnumerator Reload()
    {
        isReloading = true;
        WeaponsAnimator.SetTrigger("Reload");
        UIManager.Instance.ShowReloadSlider(true); // 슬라이더 활성화
        UIManager.Instance.SetReloadSliderMaxValue(reloadTime);
        UIManager.Instance.SetReloadSliderValue(0);

        float reloadProgress = 0f;
        while (reloadProgress < reloadTime)
        {
            reloadProgress += Time.deltaTime;
            UIManager.Instance.SetReloadSliderValue(reloadProgress);
            yield return null;
        }

        // 재장전 완료
        curweaponData.current_magazine = curweaponData.magazine;
        gameManager.SaveWeaponData(curweaponData);
        UIManager.Instance.ShowReloadSlider(false); // 슬라이더 비활성화
        isReloading = false;
    }
    void ShootInput()
    {
        curweaponData = gameManager.GetWeaponData();
        int current_magazine = curweaponData.current_magazine;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - WeaponTransform.position).normalized;
        WeaponTransform.up = direction;

        if (Input.GetMouseButtonDown(0) &&
            current_magazine > 0 && (curweaponData.IsInfiniteAmmo() || curweaponData.current_Ammo > 0) && !isReloading)
        {
            Shoot();
            if(!curweaponData.IsInfiniteAmmo())
                curweaponData.current_Ammo -= 1;

            curweaponData.current_magazine -= 1;

            gameManager.SaveWeaponData(curweaponData);
        }
        else if (current_magazine == 0 && !isReloading)
        {
            SoundManager.Instance.SFXPlay("shotgun_reload_01", 217,0.05f); // 재장전 사운드
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (!isSoulActive)
        {
            GameObject bullet = Instantiate(bulletPrefab, shotpoint.position, WeaponTransform.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            bulletRb.velocity = WeaponTransform.up * bulletSpeed;
            SoundManager.Instance.SFXPlay("shotgun_shot_01", 218, 0.05f); // 총 사운드
            WeaponsAnimator.SetTrigger("Shot");
        }
        else
        {
            GameObject bullet = Instantiate(soulbulletPrefab, soulshotpoint.position, WeaponTransform.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            bulletRb.velocity = WeaponTransform.up * bulletSpeed;
            SoundManager.Instance.SFXPlay("soul_shot_01", 124, 0.05f); // 총 사운드
            WeaponsAnimator.SetTrigger("Shot");
        }
    }
    #endregion shot_code

    #region roll_code
    // 쿨타임 시작
    void StartCooldown()
    {
        isCooldown = true;
    }

    // 구르기 코루틴
    IEnumerator Roll()
    {
        objectState = ObjectState.Roll;
        SetAnimatorBooleansFalse();
        EffectManager.Instance.SpawnEffect("rolleffect1", feetPoint.transform.position, Quaternion.identity);
        isMove = false;
        // UIManager의 GetKeyCode를 통해 방향키 입력 감지
        if (Input.GetKey(UIManager.Instance.GetKeyCode(0)) && Input.GetKey(UIManager.Instance.GetKeyCode(3)))
        {
            rollDirection = new Vector2(1, 1).normalized; // 오른쪽 위 대각선
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(0)) && Input.GetKey(UIManager.Instance.GetKeyCode(2)))
        {
            rollDirection = new Vector2(-1, 1).normalized; // 왼쪽 위 대각선
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(1)) && Input.GetKey(UIManager.Instance.GetKeyCode(3)))
        {
            rollDirection = new Vector2(1, -1).normalized; // 오른쪽 아래 대각선
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(1)) && Input.GetKey(UIManager.Instance.GetKeyCode(2)))
        {
            rollDirection = new Vector2(-1, -1).normalized; // 왼쪽 아래 대각선
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(0)))
        {
            rollDirection = Vector2.up; // 위쪽
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(1)))
        {
            rollDirection = Vector2.down; // 아래쪽
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(3)))
        {
            rollDirection = Vector2.right; // 오른쪽
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(2)))
        {
            rollDirection = Vector2.left; // 왼쪽
        }
        else
        {
            yield break; // 입력값이 없으면 구르기를 시작하지 않음
        }

        rollTime = 0;
        HandleRollAnimation(rollDirection);
        animator.SetTrigger("IsRoll");

        SoundManager.Instance.SFXPlay("dodge_roll_01", 219, 0.05f); // 구르기 사운드
        bool effectPlayed = false; // 효과가 이미 재생되었는지 확인하는 플래그

        while (rollTime < rollDuration)
        {
            rollTime += Time.deltaTime;
            float t = rollTime / rollDuration;
            // 자연스러운 구르기 동작을 위해 속도를 일정하게 유지
            rigid.velocity = rollDirection * rollSpeed * Mathf.Lerp(1f, 0f, t);

            if (!effectPlayed && rollTime > rollDuration - 0.3f)
            {
                EffectManager.Instance.SpawnEffect("rolleffect2", feetPoint.transform.position, Quaternion.identity);
                effectPlayed = true;
            }
            yield return null;
        }

        rigid.velocity = Vector2.zero;
        objectState = ObjectState.None; 

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
                animator.SetBool("IsSide", true);
            }
            else // 오른쪽 이동
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1; // 스케일 그대로 설정
                animator.SetBool("IsSide", true);
            }
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
                currentScale.x = Mathf.Abs(currentScale.x) * -1;
                animator.SetBool("IsAngle", true);
            }
            else if (hir > 0 && vir > 0) // 오른쪽 위 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1;
                animator.SetBool("IsAngle", true);
            }
            else if (hir < 0 && vir < 0) // 왼쪽 아래 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * -1;
                animator.SetBool("IsSide", true);
            }
            else if (hir < 0 && vir > 0) // 오른쪽 아래 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1;
                animator.SetBool("IsSide", true);
            }
            else
            {
                animator.SetBool("isMove", false);
            }
        }
        transform.localScale = currentScale;
        animator.SetTrigger("IsRoll");
    }
    #endregion roll_code

    #region weapon_code
    void InitializeWeapons()
    {
        // 7가지 무기를 초기화
        playerWeapons.Add(new Weapon { WeaponName = "Determination", weaponType = WeaponType.Determination });
        playerWeapons.Add(new Weapon { WeaponName = "Patience", weaponType = WeaponType.Patience });
        playerWeapons.Add(new Weapon { WeaponName = "Bravery", weaponType = WeaponType.Bravery });
        playerWeapons.Add(new Weapon { WeaponName = "Integrity", weaponType = WeaponType.Integrity });
        playerWeapons.Add(new Weapon { WeaponName = "Perseverance", weaponType = WeaponType.Perseverance });
        playerWeapons.Add(new Weapon { WeaponName = "Kindness", weaponType = WeaponType.Kindness });
        playerWeapons.Add(new Weapon { WeaponName = "Justice", weaponType = WeaponType.Justice });

        // 각 무기의 색상 업데이트
        foreach (var weapon in playerWeapons)
        {
            weapon.UpdateColor();
        }
    }
    void HandleWeaponSwitchInput()
    {
        for (int i = 1; i <= playerWeapons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i - 1))
            {
                SelectWeapon(i - 1);
            }
        }
    }
    void HandleMouseWheelInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % playerWeapons.Count;
            SelectWeapon(currentWeaponIndex);
        }
        else if (scroll < 0f)
        {
            currentWeaponIndex = (currentWeaponIndex - 1 + playerWeapons.Count) % playerWeapons.Count;
            SelectWeapon(currentWeaponIndex);
        }
    }
    public void UpdateWeaponUI(Weapon currentWeapon)
    {
        UIManager.Instance.ui_weaponImage.GetComponent<Image>().color = currentWeapon.weaponColor;
       // UIManager.Instance.weaponNameText.text = currentWeapon.WeaponName;
    }
    void SelectWeapon(int index)
    {
        currentWeaponIndex = index;

        // UI 업데이트
        UIManager.Instance.ui_weaponImage.GetComponent<Image>().color = playerWeapons[currentWeaponIndex].weaponColor;

        Debug.Log($"Selected Weapon: {playerWeapons[currentWeaponIndex].WeaponName}");
    }

    #endregion
    public void updateLoad()
    {
        UIManager.Instance.isInventroy = false;
        gameManager.GetPlayerData().isStop = false;
        gameManager.GetPlayerData().isDie = false;
        isDie = false;
        //그 방에 맞는 배경음악을 불러온다.
        SoundManager.Instance.BGSoundPlayDelayed(0, 1f);
        playerData = gameManager.GetPlayerData();
        maxHealth = gameManager.GetPlayerData().Maxhealth; // 최대 체력 설정
        health = gameManager.GetPlayerData().health;       // 최대 체력 설정
        transform.position = playerData.position;
        playerData.playerAnimator = animator;
        playerData.isInvincible = isInvincible;
    }

    private void CalculateDistanceAndTriggerEffects()
    {
        Vector2 currentPosition = transform.position; // 현재 위치
        float distance = Vector2.Distance(previousPosition, currentPosition); // 이전 위치와의 거리 계산

        if (distance > positionTolerance) // 위치 변화가 허용 오차를 초과할 경우
        {
            distanceCovered += distance; // 누적 거리 증가
            previousPosition = currentPosition; // 현재 위치를 이전 위치로 업데이트

            if (distanceCovered >= distanceThreshold) // 기준 거리 이상 이동 시
            {
                TriggerWalkingEffects();
                distanceCovered = 0f; // 누적 거리 초기화
            }
        }
    }
    private void TriggerWalkingEffects()
    {
        // 걷는 소리 재생
        int soundIndex = walkingSoundStartIndex + currentSoundIndex;
        SoundManager.Instance.SFXPlay($"footstep_{soundIndex}", soundIndex, 0.1f);

        // 다음 효과음 인덱스로 이동
        currentSoundIndex = (currentSoundIndex + 1) % walkingSoundCount;

        // 발자국 이펙트 생성
        EffectManager.Instance.SpawnEffect("foot", feetPoint.transform.position, Quaternion.identity);
    }
    #region move_animation
    // 플레이어 이동 처리
    void Move()
    {
        // UIManager의 GetKeyCode를 통해 방향키 입력 감지
        h = 0f;
        v = 0f;

        if (Input.GetKey(UIManager.Instance.GetKeyCode(2))) // Left
        {
            h = -1f;
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(3))) // Right
        {
            h = 1f;
        }

        if (Input.GetKey(UIManager.Instance.GetKeyCode(0))) // Up
        {
            v = 1f;
        }
        else if (Input.GetKey(UIManager.Instance.GetKeyCode(1))) // Down
        {
            v = -1f;
        }

        if (gameManager.GetPlayerData().currentState == GameState.Event)
        {
            animator.SetBool("isMove", false);
            animator.SetInteger("v", 0);
            animator.SetInteger("h", 0);
            speed = 0f;
        }
        else
        {
            UpdateAnimatorMovement();
            speed = 4f;
        }


        rigid.velocity = new Vector2(h, v) * speed;
        playerData.position = transform.position;
        playerData.health = health;
    }

    // 애니메이터 활성화/비활성화
    public void SetAnimatorEnabled(bool isEnabled)
    {
        animator.enabled = isEnabled;
        rigid.velocity = Vector2.zero;
    }


    // 애니메이터 이동 업데이트
    void UpdateAnimatorMovement()
    {
        // 기본 이동 상태 계산
        isMove = (h != 0 || v != 0);

        // 동시 입력 처리
        if (h != 0 && v != 0)
        {
            // 두 방향 중 하나라도 충돌하지 않으면 이동 가능
            if ((isTouchingHorizontal && h != 0) && (isTouchingVertical && v != 0))
            {
                isMove = false; // 둘 다 충돌 시에만 이동 불가
            }
        }
        else
        {
            // 개별 입력 처리
            if ((isTouchingHorizontal && h != 0) || (isTouchingVertical && v != 0))
            {
                isMove = false; // 해당 방향으로 이동하려 할 경우 이동 불가
            }
        }

        // 애니메이터에 이동 상태 전달
        animator.SetBool("isMove", isMove);

        // 애니메이터에 이동 방향 전달
        if (h != animator.GetInteger("h"))
            animator.SetInteger("h", (int)h);
        else if (v != animator.GetInteger("v"))
            animator.SetInteger("v", (int)v);
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
            FlipCharacter(angle < 90f ? 1 : -1);
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
    #endregion
    public void TeleportPlayer(Vector2 pos)
    {
        transform.position = pos;
    }

}
