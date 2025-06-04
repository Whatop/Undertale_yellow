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

/// <summary>
///  Determination, // 의지 (빨강) - 강력 일격
/// Patience,      // 인내 (하늘) - 연사 총
/// Bravery,       // 용기 (주황) - 산탄 총
/// Integrity,     // 고결 (파랑) - 레이저
/// Perseverance,  // 끈기 (보라) - 유도 미사일
/// Kindness,      // 친절 (초록) - 방어벽 발사
/// Justice,       // 정의 (노랑) - 기본 총
/// None           // 기본값
/// </summary>

public enum WeaponType
{
    Revolver,          // 정의 (노랑) - 기본 총 
    NeedleGun,         // 인내 (하늘) - 연사 총 / 넵스타블룩 폐허
    Shotgun,           // 용기 (주황) - 산탄 총 / 파피루스? 스노우딘 
    BarrierEmitter,     // 친절 (초록) - 방어벽 발사 / 언다인? 워터폴
    HomingMissile,     // 끈기 (보라) - 유도 미사일 / 머펫? 
    LaserGun,          // 고결 (파랑) - 레이저 / 메타톤?알피스? 핫랜드
    Blaster,           // 의지 (빨강) - 샌즈? 강력 일격
    None           // 기본값
}

// 각 총의 특성을 나타내는 클래스
[System.Serializable]
public class Weapon
{
    public int id;             // 총의 고유한 ID
    public string WeaponName;  // 총의 이름
    public float damage;         // 총의 공격력
    public float current_Ammo;   // 현재 탄알집에 남아있는 총알 수
    public float magazine;       // 탄창 최대 총알수  
    public float current_magazine; // 현재 남아있는 총알 수
    public float maxAmmo;        // 최대 총알 수
    public float maxRange;       // 사거리
    public float bulletSpeed;  // 총알 속도
    public float accuracy;     // 총의 정확도
    public float reloadTime;     // 재장전 속도
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
        reloadTime = 1.5f;
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
            case WeaponType.Blaster:
                return new Color(1f, 0f, 0f); // 빨강
            case WeaponType.NeedleGun:
                return new Color(66f / 255f, 252f / 255f, 255f / 255f); // 하늘색
            case WeaponType.Shotgun:
                return new Color(252f / 255f, 166f / 255f, 0f); // 주황
            case WeaponType.LaserGun:
                return new Color(0f, 60f / 255f, 1f); // 파랑
            case WeaponType.HomingMissile:
                return new Color(213f / 255f, 53f / 255f, 217f / 255f); // 보라
            case WeaponType.BarrierEmitter:
                return new Color(0f, 192f / 255f, 0f); // 초록
            case WeaponType.Revolver:
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
    public Transform soulpoint;   // 영혼의 총 Transform 컴포넌트
    public GameObject bulletPrefab;   // 총알 프리팹
    public GameObject soulbulletPrefab; // 총알 프리팹
    public float bulletSpeed = 10f;   // 총알 발사 속도

    [Header("Shell Eject Setting")]
    public GameObject shellPrefab;   // 탄피 프리팹
    public Transform shellEjectPoint;// 탄피 배출 위치(권총 옆이나 탄창 부분)

    // 모든 무기 정의 (게임 시작 시 세팅)
    private List<Weapon> databaseWeapons = new List<Weapon>();
    public List<Weapon> playerWeapons = new List<Weapon>();
    public List<WeaponType> playerWeaponTypes = new List<WeaponType>();

    private int currentWeaponIndex = 0;

    public Weapon curweaponData;
    public GameObject Weapons;
    public GameObject muzzleFlashPrefab;  // 총구 화염 이펙트 프리팹
    public float muzzleFlashDuration = 0.1f; // 화염 이펙트 지속 시간
    public bool tutorialDontShot = true; // 처음에 총 못쏘도록 막는거

    // 반동 관련 변수 추가
    public float recoilDistance = 0.15f;  // 반동 거리 (총이 뒤로 밀리는 정도)
    public float recoilDuration = 0.1f;   // 반동 지속 시간
    public float recoilVerticalOffset = 0.05f; // 반동 시 위쪽으로 밀리는 정도
    private Vector3 originalWeaponPosition; // 원래 총의 위치


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

    [SerializeField]
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

    private GameObject currentLaser;
    private bool isLaserFiring = false;
    public GameObject laserCoreObject;

    private Coroutine pressFireCoroutine;
    private bool isPressFiring = false;
    public GameObject laserPrefab;        // 위에서 만든 “레이저 전용 프리팹”
    #region Test_code

    // Soul 모드 처리 테스트 용도입니다
    private void HandleSoulMode()
    {
        if (Input.GetKeyDown(KeyCode.E)) // E 키로 Soul 모드 활성화/비활성화 전환
        {
            isSoulActive = !isSoulActive; // Soul 모드 전환

            if (isSoulActive)
            {
                tutorialDontShot = false;
                EnableSoul();
            }
            else
            {
                DisableSoul();
            }
        }
    }

    #endregion 

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

        curweaponData.weaponType = WeaponType.Revolver;
        curweaponData.UpdateColor();
        soulObject.GetComponent<SpriteRenderer>().color = curweaponData.weaponColor;

        originalWeaponPosition = WeaponTransform.localPosition; // 원래 위치 저장
        InitializeWeapons();
        gameManager.SaveWeaponData(curweaponData);
        laserCoreObject.SetActive(false);
        SelectWeapon(0);

    }
    protected override void Update()
    {
        HandleSoulMode();//테스트 용도 @@

        if (isDie)
            return;
        base.Update();

        HandleWeaponSwitchInput();
        // HandleMouseWheelInput();
        playerData.isInvincible = isInvincible;

        if (!UIManager.Instance.isUserInterface && !gameManager.GetPlayerData().isStop && !gameManager.GetPlayerData().isDie)
        {
           
            float angle = CalculateMouseAngle();
            Hands.gameObject.SetActive(true);

            // R키 입력 시 재장전
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && curweaponData.current_magazine != curweaponData.magazine && !UIManager.Instance.isInventroy)
            {
                if (isSoulActive)
                    SoundManager.Instance.SFXPlay("soul_reload_01", 47, 0.05f); // 재장전 사운드
                else
                    SoundManager.Instance.SFXPlay("shotgun_reload_01", 217, 0.05f); // 재장전 사운드

                StartCoroutine(Reload());
            }

            // 우클릭 입력 시 구르기 시작
            if (Input.GetMouseButtonDown(1) && !isCooldown && objectState != ObjectState.Roll)
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

            if (!UIManager.Instance.isInventroy)
            {

                // 
                if (!tutorialDontShot)
                {
                    Weapons.SetActive(true);
                    if (curweaponData.weaponType == WeaponType.LaserGun)
                    {
                        UIManager.Instance.SetAmmoUIVisible(false);
                        UIManager.Instance.laserAmmoSlider.gameObject.SetActive(true); // 레이저 슬라이더 표시
                        UpdateLaserWeapon(); // ← 레이저 전용 로직 따로
                    }
                    else
                    {
                        UIManager.Instance.laserAmmoSlider.gameObject.SetActive(false); // 레이저 슬라이더 숨김
                        UIManager.Instance.SetAmmoUIVisible(true);
                        ShootInput();
                    }
                }
                else
                {
                    Weapons.SetActive(false);
                    Hands.gameObject.SetActive(false);
                }


                if (objectState != ObjectState.Roll) // 🔹 구르기 중에는 애니메이션 초기화 X
                {
                    SetAnimatorBooleansFalse();
                    HandleObjectState(angle);
                }
                else
                {
                    Hands.gameObject.SetActive(false);
                }
            }




            if (isSoulActive)
            {
                SoulRotateToMouse();
                SyncSoulWithPlayer(); // 플레이어와 Soul의 위치 동기화
                Weapons.SetActive(false);
                Hands.gameObject.SetActive(false);

                // 소울 (Tab 키 홀드)
                if (Input.GetKeyDown(UIManager.Instance.GetKeyCode(7)))
                    UIManager.Instance.ToggleRadialMenu(RadialMenuType.Item);
                if (Input.GetKeyUp(UIManager.Instance.GetKeyCode(7)))
                    UIManager.Instance.ConfirmRadialSelectionAndClose();
            }
            else
            {
                if (Input.GetKeyDown(UIManager.Instance.GetKeyCode(7)) &&
                    !UIManager.Instance.savePanel.activeSelf
                    && gameManager.GetPlayerData().currentState == GameState.None)
                {
                    UIManager.Instance.SetTextBar();
                    UIManager.Instance.ChangeInventroy();
                }
            }
            if (Input.GetMouseButtonUp(0) && curweaponData.weaponType == WeaponType.LaserGun && pressFireCoroutine != null)
            {
                StopCoroutine(pressFireCoroutine);
                pressFireCoroutine = null;
                isPressFiring = false;
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
  
    // Soul 모드 활성화: 투명도 끄기
    public void EnableSoul()
    {
        playerTransparency = 0;
        isSoulActive = true;
        soulObject.SetActive(true); // Soul 활성화
        SetTransparency(playerSprite, playerTransparency); // 플레이어를 흐리게
        SetTransparency(Hands, playerTransparency);
        SetTransparency(Weapons, playerTransparency);
        shadowObject.SetActive(false);
    }
    // Soul 모드 활성화: 투명도 조절
    public void EnableSoul(float a)
    {
        isSoulActive = true;
        playerTransparency = a;
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
    #endregion 

    #region shot_code
    // 총알 발사 입력 처리
    IEnumerator Reload()
    {
        isReloading = true;
        WeaponsAnimator.SetTrigger("Reload");
        UIManager.Instance.ShowReloadSlider(true); // 슬라이더 활성화
        UIManager.Instance.SetReloadSliderMaxValue(curweaponData.reloadTime);

        UIManager.Instance.SetReloadSliderValue(0);
        if (!isSoulActive)
        {
            // --- [리볼버 탄피 6개 생성] -------------------
            for (int i = 0; i < 6; i++)
            {
                EjectShell();
            }
            // -------------------------------------------
        }


        // 사운드 및 애니메이션 실행  
        if (isSoulActive)
            SoundManager.Instance.SFXPlay("soul_reload_01", 47, 0.05f); // 재장전 사운드
        else
            SoundManager.Instance.SFXPlay("shotgun_reload_01", 217, 0.05f); // 재장전 사운드
        float reloadProgress = 0f;
        while (reloadProgress < curweaponData.reloadTime)
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
    private void EjectShell()
    {
        if (shellPrefab == null || shellEjectPoint == null) return;

        GameObject shellObj = Instantiate(shellPrefab,
                                          shellEjectPoint.position,
                                          Quaternion.identity);

        // 기본 방향: 플레이어 쪽으로
        Vector2 dir = (soulpoint.position - shellEjectPoint.position).normalized;

        // 좌우로 튀도록 수직 벡터 추가 (Vector2.Perpendicular 활용)
        Vector2 perpendicular = Vector2.Perpendicular(dir); // 방향에 수직인 벡터
        float sideOffset = UnityEngine.Random.Range(-0.5f, 0.35f); // 좌우 랜덤 편차

        // 최종 방향 계산
        Vector2 finalDir = (dir + perpendicular * sideOffset).normalized;

        // 속도 랜덤
        float randSpeed = UnityEngine.Random.Range(2f, 4f);

        // ShellBehavior에 속도 할당
        ShellBehavior shell = shellObj.GetComponent<ShellBehavior>();
        if (shell != null)
        {
            shell.velocity = finalDir * randSpeed;
        }
    }

    void ShootInput()
    {
        curweaponData = gameManager.GetWeaponData();
        float current_magazine = curweaponData.current_magazine;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - WeaponTransform.position).normalized;
        WeaponTransform.up = direction;

        if (Input.GetMouseButtonDown(0) &&
       current_magazine > 0 && (curweaponData.IsInfiniteAmmo() || curweaponData.current_Ammo > 0) && !isReloading && !tutorialDontShot)
        {
            Shoot();

            // 무기별 탄약 소모 분리
            switch (curweaponData.weaponType)
            {
                case WeaponType.Revolver: // 정의
                    if (!curweaponData.IsInfiniteAmmo())
                        curweaponData.current_Ammo -= 1;
                    break;

                case WeaponType.NeedleGun: // 인내 - 1발씩 연사지만 총 3발이므로 3 소모
                    if (!curweaponData.IsInfiniteAmmo())
                    curweaponData.current_Ammo -= 3;
                    break;

                case WeaponType.Shotgun: // 용기 - 산탄 다발 5발로 계산
                    if (!curweaponData.IsInfiniteAmmo())
                    curweaponData.current_Ammo -= 1;
                    break;

                case WeaponType.BarrierEmitter: // 친절 - 방어벽 1개 생성
                    if (!curweaponData.IsInfiniteAmmo())
                    curweaponData.current_Ammo -= 1;
                    break;

                case WeaponType.HomingMissile: // 끈기 - 1개 유도탄
                    if (!curweaponData.IsInfiniteAmmo())
                    curweaponData.current_Ammo -= 1;
                    break;


                case WeaponType.Blaster: // 의지 - 대형 강탄, 1소모
                    if (!curweaponData.IsInfiniteAmmo())
                    curweaponData.current_Ammo -= 1;
                    break;

                default:
                    curweaponData.current_Ammo -= 1;
                    break;
            }

            curweaponData.current_magazine -= 1;
            gameManager.SaveWeaponData(curweaponData);
        }

        else if (current_magazine == 0 && !isReloading)
        {

            if (isSoulActive)
                SoundManager.Instance.SFXPlay("soul_reload_01", 47, 0.05f); // 재장전 사운드
            else
                SoundManager.Instance.SFXPlay("shotgun_reload_01", 217, 0.05f); // 재장전 사운드

            StartCoroutine(Reload());
        }
    }
    /// <summary>
    /// 레이저 전용 업데이트 (마우스 눌러있는 동안 연속 발사)
    /// </summary>
    private void UpdateLaserWeapon()
    {
        Weapon weapon = curweaponData;
        float currentAmmo = weapon.current_Ammo;

        // 1) 마우스 왼쪽 버튼이 눌린 상태 && 남은 탄약이 있으면 StartLaser()
        if (Input.GetMouseButton(0) && currentAmmo > 0f)
        {
            if (!isLaserFiring)
                StartLaser();

            // 2) 레이저 중인 동안 탄약 소모 (초당 40 단위 예시)
            weapon.current_Ammo -= (40f * Time.deltaTime);
            weapon.current_Ammo = Mathf.Clamp(weapon.current_Ammo, 0f, weapon.maxAmmo);

            // 3) UI 슬라이더(레이저용) 업데이트
            UIManager.Instance.UpdateLaserSlider(weapon.current_Ammo, weapon.maxAmmo);
        }
        else
        {
            // 버튼을 뗐거나 탄약이 바닥났으면 StopLaser()
            if (isLaserFiring)
                StopLaser();
        }
    }

    private void StartLaser()
    {
        if (isLaserFiring) return;
        isLaserFiring = true;

        // 1) 차지(발사 준비) 사운드 재생
        SoundManager.Instance.SFXPlay("se_gapower", 63);
        if (laserCoreObject != null)
            laserCoreObject.SetActive(true);

        Vector3 spawnPos = soulshotpoint.position;
        Quaternion spawnRot = GetMouseRotation();

        currentLaser = Instantiate(laserPrefab, spawnPos, spawnRot);
        currentLaser.transform.SetParent(soulshotpoint);
        currentLaser.transform.localPosition = Vector3.zero;
        currentLaser.transform.localRotation = Quaternion.identity;
        currentLaser.transform.localScale = Vector3.one;

        BulletController bc = currentLaser.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.damage = curweaponData.damage;
            bc.FireLaser();  // ← 현재 BulletController에 없음! (컴파일 오류)
        }
        else
        {
            Debug.LogError("[PlayerMovement] 생성된 레이저 오브젝트에 BulletController가 없습니다.");
        }
    }

    private void StopLaser()
    {
        isLaserFiring = false;

        if (laserCoreObject != null)
            laserCoreObject.SetActive(false);

        if (currentLaser != null)
        {
            BulletController bc = currentLaser.GetComponent<BulletController>();
            if (bc != null)
            {
                bc.StopLaser();  // ← 마찬가지로 BulletController에 없음
            }
            currentLaser = null;
        }
    }

    /// <summary>
    /// 마우스 위치를 기준으로 “화면 우측”을 바라보게 만드는 회전값 계산
    /// </summary>
    private Quaternion GetMouseRotation()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 dir = (mouseWorld - WeaponTransform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, angle);
    }

    void Shoot()
    {
        // 현재 Soul 모드인지 확인하여 적절한 총알을 생성
     
        if (!isSoulActive)
        {
            // 총알 스폰
            BattleManager.Instance.SpawnBulletAtPosition(
             BulletType.Directional,
             shotpoint.position,
             WeaponTransform.rotation,
             WeaponTransform.up,
             "Player_Normal",0,0,true,
            curweaponData.maxRange, curweaponData.bulletSpeed, curweaponData.damage);

            // 총구 화염 이펙트 생성
            StartCoroutine(ShowMuzzleFlash(shotpoint));
            // 반동 효과 추가
            StartCoroutine(ApplyRecoil());
        }
        else
        {
            //타입에 따른 총알생성
            HandleWeaponSpecificMovement();
        }


        // 사운드 및 애니메이션 실행  
        ShotSounds();
        WeaponsAnimator.SetTrigger("Shot");
    }
    // 🔥 반동 효과 적용
    IEnumerator ApplyRecoil()
    {
        // 현재 총기의 원래 위치 저장
        Vector3 originalPosition = WeaponTransform.localPosition;

        // 마우스 위치 가져오기
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // 2D 환경이므로 Z축 무시

        // 플레이어 위치
        Vector3 playerPosition = transform.position;

        // 마우스가 어느 방향에 있는지 판별
        bool isLeft = mousePosition.x < playerPosition.x; // 왼쪽
        bool isRight = mousePosition.x > playerPosition.x; // 오른쪽
        bool isUp = mousePosition.y > playerPosition.y;   // 위쪽
        bool isDown = mousePosition.y < playerPosition.y; // 아래쪽

        // 기본 반동 방향 (총이 바라보는 방향의 반대)
        Vector3 recoilOffset = -WeaponTransform.up * recoilDistance;

        // 위쪽(Y축)으로 살짝 랜덤하게 튀도록 추가
        float randomVerticalOffset = UnityEngine.Random.Range(-recoilVerticalOffset, recoilVerticalOffset);

        // 반동 방향 조정 (마우스 위치에 따라)
        if (isLeft)
        {
            // 왼쪽에서는 반동 방향을 반전
            recoilOffset.x = -recoilOffset.x;
        }

        if (isUp || isDown)
        {
            // 위/아래 방향 반동이 너무 강하면 Y축 반동을 약하게 조절
            randomVerticalOffset *= 0.5f; // 위/아래 반동을 50%로 줄임
        }

        // 최종 반동 위치 계산
        recoilOffset += new Vector3(0, randomVerticalOffset, 0);
        Vector3 targetPosition = originalPosition + recoilOffset;

        // 반동 적용
        WeaponTransform.localPosition = targetPosition;

        float elapsedTime = 0f;
        while (elapsedTime < recoilDuration)
        {
            WeaponTransform.localPosition = Vector3.Lerp(targetPosition, originalPosition, elapsedTime / recoilDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        WeaponTransform.localPosition = originalPosition;
    }


    // 총구 화염 이펙트 생성 후 일정 시간 후 제거
    IEnumerator ShowMuzzleFlash(Transform firePoint)
    {
        GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation, firePoint);
        yield return new WaitForSeconds(muzzleFlashDuration);
        Destroy(muzzleFlash);
    }


    public void AddWeapon(Weapon weapon)
    {
        if (!playerWeapons.Contains(weapon))
        {
            playerWeapons.Add(weapon);
            Debug.Log($"무기 추가됨: {weapon.WeaponName}");
        }
    }
    void ShotSounds()
    {
        if (!isSoulActive)
            SoundManager.Instance.SFXPlay("shotgun_shot_01", 218);
        else
        {
            switch (curweaponData.weaponType)
            {
                case WeaponType.Revolver:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;

                case WeaponType.Blaster:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;

                case WeaponType.NeedleGun:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;

                case WeaponType.Shotgun:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;

                case WeaponType.LaserGun:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;

                case WeaponType.HomingMissile:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;

                case WeaponType.BarrierEmitter:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;

                default:
                    SoundManager.Instance.SFXPlay("soul_shot_01", 124);
                    break;
            }
        }
    }
    public void HandleWeaponSpecificMovement()
    {
        switch (curweaponData.weaponType)
        {
            case WeaponType.Revolver:
                ShootBasic(); break;

            case WeaponType.NeedleGun:
                StartCoroutine(ShootNeedleGun()); break;

            case WeaponType.Shotgun:
                ShootShotgun(); break;

            case WeaponType.BarrierEmitter:
                ShootBarrier(); break;

            case WeaponType.HomingMissile:
                ShootHoming(); break;

            case WeaponType.Blaster:
                ShootBlaster(); break;

            default:
                Debug.Log("잘못된 무기 설정");
                break; // ✅ 추가
        }

    }

    void ShootBasic()
    {
        BattleManager.Instance.SpawnBulletAtPosition(
            BulletType.Directional,
            soulshotpoint.position,
            WeaponTransform.rotation,
            WeaponTransform.up,
            "Player_Normal", 0, 0, true,
            curweaponData.maxRange, curweaponData.bulletSpeed, curweaponData.damage);
    }

    IEnumerator ShootNeedleGun()
    {
        int count = 3;
        float delay = 0.1f;

        for (int i = 0; i < count; i++)
        {
            ShootBasic();
            yield return new WaitForSeconds(delay);
        }
    }

    void ShootShotgun()
    {
        float spread = 15f;
        for (int i = -2; i <= 2; i++)
        {
            Quaternion rot = Quaternion.Euler(0, 0, WeaponTransform.eulerAngles.z + i * spread);
            Vector2 dir = rot * Vector2.up;

            BattleManager.Instance.SpawnBulletAtPosition(
                BulletType.Directional,
                soulshotpoint.position,
                rot,
                dir,
                "Player_Normal", 0, 0, true,
            curweaponData.maxRange, curweaponData.bulletSpeed, curweaponData.damage);
        }
    }

    void ShootBarrier()
    {
        BattleManager.Instance.SpawnBulletAtPosition(
            BulletType.Barrier, // 별도 타입 필요
            soulshotpoint.position,
            WeaponTransform.rotation,
            WeaponTransform.up,
            "Barrier", 0, 0, true,
            curweaponData.maxRange,curweaponData.bulletSpeed,curweaponData.damage);
    }

    void ShootHoming()
    {
        BattleManager.Instance.SpawnBulletAtPosition(
            BulletType.Homing,
            soulshotpoint.position,
            WeaponTransform.rotation,
            WeaponTransform.up,
            "Homing", 0, 0, true,
            curweaponData.maxRange, curweaponData.bulletSpeed, curweaponData.damage);
    }

    private IEnumerator PressFireLoop()
    {
        isPressFiring = true;

        // Core 활성화
        laserCoreObject.SetActive(true);

        while (Input.GetMouseButton(0) && curweaponData.current_Ammo > 0)
        {

            BattleManager.Instance.SpawnBulletAtPosition(
                BulletType.Directional,
                soulshotpoint.position,
                WeaponTransform.rotation,
                WeaponTransform.up,
                "Laser", 0, 0, true,
            curweaponData.maxRange, curweaponData.bulletSpeed, curweaponData.damage);

            curweaponData.current_Ammo -= 1;
            gameManager.SaveWeaponData(curweaponData);

            yield return new WaitForSeconds(0.01f); // 연사 간격 짧게
        }

        isPressFiring = false;
        laserCoreObject.SetActive(false);
    }

    void ShootBlaster()
    {
        BattleManager.Instance.SpawnBulletAtPosition(
            BulletType.GasterBlaster,
            soulshotpoint.position,
            WeaponTransform.rotation,
            WeaponTransform.up,
            "Blaster", 0, 0, true);


    }

    public void OpenSoulUnlockShop()
    {
        List<WeaponType> lockedWeapons = GetLockedWeapons();
        foreach (var weapon in lockedWeapons)
        {
            Debug.Log($"{weapon} - {GetUnlockCost(weapon)}G");
        }

        // UI 표시 로직은 이후 작성
    }

    private List<WeaponType> GetLockedWeapons()
    {
        List<WeaponType> allWeapons = new List<WeaponType>
    {
        WeaponType.Blaster,
        WeaponType.NeedleGun,
        WeaponType.Shotgun,
        WeaponType.LaserGun,
        WeaponType.HomingMissile,
        WeaponType.BarrierEmitter
    };

        List<WeaponType> unlockedWeapons = GameManager.Instance.GetPlayerData()
            .player.GetComponent<PlayerMovement>().playerWeaponTypes;

        return allWeapons.FindAll(w => !unlockedWeapons.Contains(w));
    }

    private int GetUnlockCost(WeaponType weapon)
    {
        // 무기별 비용 설정
        return weapon switch
        {
            WeaponType.Blaster => 100,
            WeaponType.NeedleGun => 120,
            WeaponType.Shotgun => 130,
            WeaponType.LaserGun => 140,
            WeaponType.HomingMissile => 150,
            WeaponType.BarrierEmitter => 160,
            _ => 999
        };
    }
    #endregion

    #region roll_code
    // 쿨타임 시작
    void StartCooldown()
    {
        isCooldown = true;
    }

    // 구르기 코루틴
    IEnumerator Roll()
    {
        isMove = false;
        float soulEffectSpawnInterval = 0.04f; // Soul 이펙트 생성 간격
        float lastSpawnTime = 0f; // 마지막 이펙트 생성 시간

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

        objectState = ObjectState.Roll;
        SetAnimatorBooleansFalse();
        rollTime = 0;
        HandleRollAnimation(rollDirection);
        animator.SetTrigger("IsRoll");
        if (isSoulActive)
        {
            SoundManager.Instance.SFXPlay("soul_roll_01", 157, 0.05f); // 구르기 사운드
            rollSpeed = 10f;
        }
        else
        {
            SoundManager.Instance.SFXPlay("dodge_roll_01", 219, 0.05f); // 구르기 사운드
            EffectManager.Instance.SpawnEffect("rolleffect1", feetPoint.transform.position, Quaternion.identity);
            rollSpeed = 16f;

        }
        bool effectPlayed = false; // 효과가 이미 재생되었는지 확인하는 플래그

        while (rollTime < rollDuration)
        {
            rollTime += Time.deltaTime;
            float t = rollTime / rollDuration;
            // 자연스러운 구르기 동작을 위해 속도를 일정하게 유지
            rigid.velocity = rollDirection * rollSpeed * Mathf.Lerp(1f, 0f, t);

            if (!isSoulActive && !effectPlayed && rollTime > rollDuration - 0.3f)
            {
                EffectManager.Instance.SpawnEffect("rolleffect2", feetPoint.transform.position, Quaternion.identity);
                effectPlayed = true;
            }
            if (isSoulActive && Time.time - lastSpawnTime >= soulEffectSpawnInterval)
            {
                EffectManager.Instance.SpawnEffect("soul_rolleffect", transform.position, soulObject.transform.rotation, soulObject.GetComponent<SpriteRenderer>().color); ;
                lastSpawnTime = Time.time;
            }

            yield return null;
        }

        rigid.velocity = Vector2.zero;
        objectState = ObjectState.None;

        // 🔹 구르기 종료 후 `isMove` 상태 복구
        isMove = (h != 0 || v != 0);
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
        databaseWeapons.Add(new Weapon
        {
            id = 0,
            WeaponName = "노랑",
            weaponType = WeaponType.Revolver,
            damage = 3,
            magazine = 6,
            current_magazine = 6,
            maxAmmo = -1,
            maxRange = 6f,
            current_Ammo = -1,
            bulletSpeed = 8f,
            accuracy = 1f,
            reloadTime = 1.5f
        });; databaseWeapons.Add(new Weapon
        {
            id = 1,
            WeaponName = "하늘",
            weaponType = WeaponType.NeedleGun,
            damage = 2,
            magazine = 15,
            current_magazine = 15,
            maxAmmo = 90,
            maxRange = 8f,
            current_Ammo = 90,
            bulletSpeed = 9f,
            accuracy = 1.2f,
            reloadTime = 1.2f
        })
            ; databaseWeapons.Add(new Weapon
        {
            id = 2,
            WeaponName = "주황",
            weaponType = WeaponType.Shotgun,
            damage = 4,
            magazine = 4,
            current_magazine = 4,
            maxAmmo = 30,
            maxRange = 4f,
            current_Ammo = 30,
            bulletSpeed = 6f,
            accuracy = 0.6f,
            reloadTime = 2.0f
        }); 
        databaseWeapons.Add(new Weapon
        {
            id = 3,
            WeaponName = "초록",
            weaponType = WeaponType.BarrierEmitter,
            damage = 0,
            magazine = 4,
            current_magazine = 4,
            maxAmmo = 16,
            current_Ammo = 16,
            bulletSpeed = 5f,
            maxRange = 10f,
            accuracy = 1f,
            reloadTime = 3.0f
        });
        databaseWeapons.Add(new Weapon
        {
            id = 4,
            WeaponName = "보라",
            weaponType = WeaponType.HomingMissile,
            damage = 2,
            magazine = 3,
            current_magazine = 3,
            maxAmmo = 50,
            current_Ammo = 50,
            maxRange = 20f,
            bulletSpeed = 5f,
            accuracy = 1f,
            reloadTime = 2.5f
        });
        databaseWeapons.Add(new Weapon
        {
            id = 5,
            WeaponName = "파랑",
            weaponType = WeaponType.LaserGun,
            damage = 100f,
            magazine = 700,
            current_magazine = 700,
            maxAmmo = 700,
            current_Ammo = 700,
            maxRange = 9f,
            bulletSpeed = 7f,
            accuracy = 1f,
            reloadTime = 3.5f
        });
        databaseWeapons.Add(new Weapon
        {
            id = 6,
            WeaponName = "빨강",
            weaponType = WeaponType.Blaster,
            damage = 7,
            magazine = 1,
            current_magazine = 1,
            maxAmmo = 10,
            maxRange = 15f,
            current_Ammo = 5,
            bulletSpeed = 0f,
            accuracy = 1f,
            reloadTime = 4.0f
        });
        databaseWeapons.Add(new Weapon { WeaponName = "빨강", weaponType = WeaponType.Blaster, id = 6});
        AddWeapon(databaseWeapons[0]);
        AddWeapon(databaseWeapons[1]);
        AddWeapon(databaseWeapons[2]);
        AddWeapon(databaseWeapons[3]);
        AddWeapon(databaseWeapons[4]);
        AddWeapon(databaseWeapons[5]);
        AddWeapon(databaseWeapons[6]);
        // 각 무기의 색상 업데이트
        foreach (var weapon in databaseWeapons)
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
       soulObject.GetComponent<SpriteRenderer>().color = currentWeapon.weaponColor;
        // UIManager.Instance.weaponNameText.text = currentWeapon.WeaponName;
    }
    public void SelectWeapon(int index)
    {
        if (currentWeaponIndex == index)
            return;

        currentWeaponIndex = index;

        // UI 업데이트
        soulObject.GetComponent<SpriteRenderer>().color = playerWeapons[currentWeaponIndex].weaponColor;

        Debug.Log($"Selected Weapon: {playerWeapons[currentWeaponIndex].WeaponName}");
        curweaponData = playerWeapons[currentWeaponIndex];
        gameManager.SaveWeaponData(curweaponData);
        // 2) UIManager에 알리기
        if(curweaponData.weaponType != WeaponType.LaserGun)
        UIManager.Instance.OnWeaponChanged(curweaponData);
    }
    public void SetWeaponData(Weapon weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("무기 데이터가 null입니다.");
            return;
        }

        curweaponData = weapon;

        // 무기 색상 UI 적용
        UIManager.Instance.UpdateSoulUIColor();

        // 총알 수, 탄창 수 등 UI 갱신
        UIManager.Instance.UpdateUI();

        // 필요시 무기 사운드/이펙트 초기화 가능
        // soundManager.PlayWeaponEquipSound(weapon.weaponType);
        // effectManager.PlayWeaponEquipEffect(weapon.weaponType);

        Debug.Log($"무기 설정됨: {weapon.WeaponName}");
    }

    public void EquipWeapon(WeaponType weaponType)
    {
        Weapon weaponToEquip = databaseWeapons.Find(w => w.weaponType == weaponType);

        if (weaponToEquip == null)
        {
            Debug.LogWarning($"{weaponType} 무기를 데이터베이스에서 찾을 수 없습니다.");
            return;
        }

        curweaponData = weaponToEquip;

        SetWeaponData(curweaponData);
        GameManager.Instance.SaveWeaponData(curweaponData);
        UIManager.Instance.UpdateSoulUIColor();

        Debug.Log($"[무기 전환] {weaponToEquip.WeaponName} 무기로 교체 완료");
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
        if (objectState != ObjectState.Roll) // 🔹 구르기 중이 아닐 때만 `isMove` 값 변경
        {
            isMove = (h != 0 || v != 0);
        }

        // 충돌 상태에 따른 이동 제한 로직
        if (h != 0 && v != 0)
        {
            if ((isTouchingHorizontal && h != 0) && (isTouchingVertical && v != 0))
            {
                isMove = false;
            }
        }
        else
        {
            if ((isTouchingHorizontal && h != 0) || (isTouchingVertical && v != 0))
            {
                isMove = false;
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

    float CalculateMouseAngle()
    {
        Vector3 mouseScreenPos = Input.mousePosition;

        // 1) 카메라 위치가 -10이면, 절댓값 10을 넣어야
        //    '카메라 → 화면' 평면이 월드 z = 0에 매핑된다.
        mouseScreenPos.z = -10;

        // 2) 이제 ScreenToWorldPoint를 호출하면
        //    항상 월드 z = 0 평면에서 올바른 (x,y) 좌표를 반환한다.
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f; // 2D 환경이므로 강제로 z = 0으로 고정

        // 3) 플레이어 위치와의 방향 벡터 산출
        Vector2 dir = (mouseWorldPos - gameManager.GetPlayerData().player.transform.position);

        // 4) 방향 벡터로부터 방위 각(도)을 계산
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
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
    public void MakePlayerTransparent()
    {
        // 예: 2초 동안 천천히 페이드아웃
        StartCoroutine(FadeOutSprite(playerSprite, 1.0f));
    }

    private List<string> unlockedEmotions = new List<string>
{
    "자비", "긍정", "무시", "유혹", "분노", "부정"
};

    // 외부에서 조회할 수 있도록 공개 메서드 제공
    public List<string> GetUnlockedEmotions()
    {
        return unlockedEmotions;
    }
    public void UnlockEmotion(string emotionName)
    {
        if (!unlockedEmotions.Contains(emotionName))
        {
            unlockedEmotions.Add(emotionName);
        }
    }
}
