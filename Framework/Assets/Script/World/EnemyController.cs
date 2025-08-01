using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations;  // AnimatorControllerParameterType 사용을 위해

public enum VirtueType
{
    Bravery,       // 용기
    Justice,       // 정의
    Integrity,     // 고결
    Kindness,      // 친절
    Perseverance,  // 끈기
    Patience,      // 인내
    Determination  // 의지
}

public enum EnemyAttackType
{
    Melee,       // 돌진형
    Laser,       // 레이저 발사 (Gaster Blaster류)
    Bullet,      // 일반 탄환
    Sniper,      // 조준 후 강한 탄환
    Shotgun,     // 산탄
    Buff,        // 버프/자힐/강화
    Predictive,  // 예측 사격
    Trap_Laser,  // 설치형 함정
    Trap_Bullet, 
    Trap_Melee,
    Undying,     // 죽지 않음 (불사형, 샌즈류)
    Special,      // 기타 특수
    None
}
public enum TrapDir { 
    Left,
    LeftUp,
    LeftDown,
    Right,
    RightUp,
    RightDown,
    Up,
    Down,
    None
}

public class EnemyController : LivingObject
{
    // 새롭게 사용할 프리팹 이름 (생략 가능, 내부에서 자동 처리)
    [SerializeField] private string bulletPrefabName = "Enemy_None";
    public float bulletSpeed = 10f; // 총알 발사 속도
    public Weapon weaponData;          // 현재 사용 중인 총의 정보
    public Transform WeaponTransform;  // 총 모델의 Transform
    public Transform hand;  // 총 모델의 Transform 
    public ObjectState objectState;
    public float minDistance = 3f;  // 플레이어와의 최소 유지 거리
    public float maxDistance = 6f;  // 플레이어와의 최대 유지 거리

    public float shootCoolTime = 4;
    float curTime = 0;
    public bool isMove;
    public EnemyAttackType attackType;
    public TrapDir dir;
    [Header("데이터")]
    [SerializeField] private EnemyData enemyData;  // 인스펙터에서 할당
    public VirtueType virtue; // 각 몬스터에 하나씩 할당
    [SerializeField]
    private List<string> reactableEmotions = new List<string>();



    [Header("Target Indicator")]
    [Tooltip("가장 가까운 적 표시용 프리팹 (아웃라인 + 하트)")]
    private GameObject outlineObject;
    [SerializeField] private GameObject outlineHeart;
    private SpriteRenderer outlineSpriteRenderer;
    public Material outlineMaterial; // 외곽선 Material

    [Header("임시 체력")]
    public float testhp = 100;


    [Header("트랩 관련")]
    public bool isTrapActive = true;     // 트랩 활성화 여부
    public float trapShootInterval = 2f; // 트랩 발사 주기
    private float trapTimer = 0f;        // 트랩용 타이머
    [Header("레이저 관련")]
    public GameObject laserPrefab; // LaserFadeOut 프리팹
    private GameObject currentLaser; // 현재 생성된 레이저
    public bool iskeepLaser;

    private bool undying = false;

    protected override void Awake()
    {
        base.Awake(); // LivingObject의 Awake 메서드 호출
                      //animator.GetComponent<Animator>();
                      //
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        weaponData = new Weapon();
        if (enemyData != null)
        {
            // 기본 정보 적용
            virtue = enemyData.virtue;
            reactableEmotions = new List<string>(enemyData.reactableEmotions);

            maxHealth = enemyData.maxHealth;
            health = maxHealth;
            speed = enemyData.moveSpeed;
            shootCoolTime = enemyData.shootCooldown;
            bulletPrefabName = enemyData.bulletPrefabName;

            // 트랩 관련
            isTrapActive = enemyData.isTrapActive;
            trapShootInterval = enemyData.trapShootInterval;

            // 거리 유지
            minDistance = enemyData.minDistance;
            maxDistance = enemyData.maxDistance;

            Debug.Log($"[{gameObject.name}] EnemyData 적용 완료");
        }
        else
        {
            Debug.LogWarning("EnemyData가 할당되지 않았습니다.");
        }
        CreateOutline(); // 하이라이트용 외곽선 오브젝트 생성
        if (IsTrapType())
        {
            RotateToTrapDirection(); // 트랩일 경우 방향 회전
        }
    }
    // 외곽선 오브젝트 생성
    void CreateOutline()
    {
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localScale = Vector3.one * 1.1f; // 원래 크기보다 약간 크게 설정

        outlineSpriteRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineSpriteRenderer.color = Color.yellow;
        outlineSpriteRenderer.sprite = spriteRenderer.sprite;
        outlineSpriteRenderer.material = outlineMaterial; // Material을 외곽선 Material로 설정
        outlineSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        outlineSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // NPC보다 뒤에 표시되도록 정렬 순서 조정

        outlineObject.SetActive(false); // 처음에는 비활성화
    }
    /// <summary>
    /// 플레이어가 선택한 감정 표현을 받았을 때 호출
    /// </summary>
    /// <param name="emotion">감정 키워드 (예: "Mercy", "Anger" 등)</param>
    public void ReceiveEmotion(string emotion)
    {
        // 1) 반응 이펙트 재생
        Vector3 effectPos = transform.position + Vector3.up * 1.5f;
        // 이펙트 풀에 "Emotion_Mercy", "Emotion_Anger" 등 이름으로 미리 등록해 두세요
       // EffectManager.Instance.SpawnEffect($"Emotion_{emotion}", effectPos, Quaternion.identity); :contentReference[oaicite: 1]{ index = 1}

        // 2) 애니메이터 트리거 설정
        if (animator != null && HasTrigger(animator, emotion))
            animator.SetTrigger(emotion);

        // 3) 추가 로직: 예를 들어 감정에 따라 체력 증가/감소, 상태 이상 적용 등 구현
    }

    // Animator에 해당 트리거가 있는지 확인 (NPC.cs의 HasTrigger 참고) :contentReference[oaicite:2]{index=2}
    private bool HasTrigger(Animator anim, string triggerName)
    {
        foreach (var param in anim.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == triggerName)
                return true;
        }
        return false;
    }
    /// <summary>
    /// BattleManager 에서 호출하는 통합 감정 상호작용 메서드
    /// </summary>
    /// <param name="emotion">예: "SpeedUp", "DamageDown", "Mercy", "Flee"</param>
    public void ProcessEmotion(string emotion)
    {
        // 지정된 감정이 반응 리스트에 없다면 무시
        if (!reactableEmotions.Contains(emotion)) return;

        Debug.Log($"[{gameObject.name}] {emotion} 감정 수신됨 / 미덕: {virtue}");

        switch (virtue)
        {
            case VirtueType.Bravery: // 용기
                if (emotion == "Flirt" || emotion == "Anger")
                {
                    ReceiveEmotion(emotion);
                    // 일시적으로 이동속도 증가
                    StartCoroutine(TempSpeedUp(1.5f, 2f));
                }
                break;

            case VirtueType.Justice: // 정의
                if (emotion == "Respect" || emotion == "Disgust")
                {
                    ReceiveEmotion(emotion);
                    // 탄속 증가
                    bulletSpeed += 2f;
                    Invoke(nameof(ResetBulletSpeed), 2f);
                }
                break;

            case VirtueType.Integrity: // 고결
                if (emotion == "Affirm" || emotion == "Deny")
                {
                    ReceiveEmotion(emotion);
                    // 일정시간 무적 (예: 피해 무시)
                }
                break;

            case VirtueType.Kindness: // 친절
                if (emotion == "Mercy" || emotion == "Pray")
                {
                    ReceiveEmotion(emotion);
                    // 체력 소폭 회복
                    Heal(10f);
                }
                break;

            case VirtueType.Perseverance: // 끈기
                if (emotion == "Sorrow" || emotion == "Fear")
                {
                    ReceiveEmotion(emotion);
                    // 죽었을 경우 1회 부활
                    if (health <= 0 && !undying)
                    {
                        undying = true;
                        health = maxHealth * 0.3f;
                        Debug.Log($"{gameObject.name}이 끈기로 다시 일어섰습니다.");
                    }
                }
                break;

            case VirtueType.Patience: // 인내
                if (emotion == "Ignore" || emotion == "Truth")
                {
                    ReceiveEmotion(emotion);
                    // 2초간 공격 정지
                    StopAllCoroutines();
                    StartCoroutine(DelayAttack(2f));
                }
                break;

            case VirtueType.Determination: // 의지
                if (emotion == "Love" || emotion == "Respect")
                {
                    ReceiveEmotion(emotion);
                    // 체력 50% 이하일 경우 모든 능력 강화
                    if (health < maxHealth * 0.5f)
                    {
                        speed *= 1.5f;
                        bulletSpeed += 3f;
                        shootCoolTime *= 0.8f;
                        Debug.Log($"{gameObject.name} 의지가 불타오른다!");
                    }
                }
                break;
        }
    }
    IEnumerator TempSpeedUp(float multiplier, float duration)
    {
        speed *= multiplier;
        yield return new WaitForSeconds(duration);
        speed /= multiplier;
    }
     
    IEnumerator DelayAttack(float delay)
    {
        float originalCool = shootCoolTime;
        shootCoolTime = 999f; // 매우 길게 설정해서 잠시 정지
        yield return new WaitForSeconds(delay);
        shootCoolTime = originalCool;
    }

    void ResetBulletSpeed()
    {
        bulletSpeed = 10f; // 기본값으로 되돌림
    }

    /// <summary>
    /// BattleManager에서 가장 가까운 적일 때 호출
    /// </summary>
    public void SetTargetingSprite(bool on)
    {
        if (outlineObject != null)
            outlineObject.SetActive(on);
        if (outlineHeart != null)
            outlineHeart.SetActive(on);
    }

    protected override void Update()
    {
        base.Update();
        if (!isDie)
        {
            if (attackType == EnemyAttackType.None)
                return;
            // 트랩은 플레이어 추격하지 않음
            if (!IsTrapType())
            {
                float distanceToPlayer = Vector2.Distance(gameManager.GetPlayerData().position, transform.position);

                if (distanceToPlayer > maxDistance && isMove)
                    ChasePlayer();
                else if (distanceToPlayer < minDistance)
                    MoveAwayFromPlayer();
                else
                    StopMoving();
            }
            else
            {
                StopMoving(); // 트랩은 고정
            }

            if (IsTrapType())
            {
                if (!isTrapActive) return;

                trapTimer += Time.deltaTime;
                if (trapTimer >= trapShootInterval)
                {
                    trapTimer = 0f;
                    Shoot(); // 트랩도 Shoot()을 이용함
                }
            }
            else
            {
                float curmagazine = weaponData.current_magazine;
                curTime += Time.deltaTime;

                Vector3 playerPosition = gameManager.GetPlayerData().position;
                Vector2 direction = (playerPosition - WeaponTransform.position).normalized;
                hand.up = direction;

                if (curTime > shootCoolTime && curmagazine > 0)
                {
                    Shoot();
                    weaponData.current_magazine -= 1;
                    weaponData.current_Ammo -= 1;
                    curTime = 0;
                }

                // 탄약 재장전
                if (weaponData.current_Ammo < weaponData.maxAmmo &&
                    weaponData.current_magazine < weaponData.magazine)
                {
                    weaponData.current_magazine = weaponData.magazine;
                }
            }
        }
        else
            StopMoving();

    }

    bool IsTrapType()
    {
        return attackType == EnemyAttackType.Trap_Bullet ||
               attackType == EnemyAttackType.Trap_Laser ||
               attackType == EnemyAttackType.Trap_Melee;
    }
    void ChasePlayer()
    {
        Vector2 direction = (gameManager.GetPlayerData().position - transform.position).normalized;
        rigid.velocity = direction * speed;
    }

    void MoveAwayFromPlayer()
    {
        Vector2 direction = (transform.position - gameManager.GetPlayerData().position).normalized;
        rigid.velocity = direction * speed;
    }

    void StopMoving()
    {
        rigid.velocity = Vector2.zero;
    }

    void Shoot()
    {
        string prefabName = GetBulletPrefabName(); // 타입에 따라 프리팹명 가져오기

        Vector2 spawnPos = WeaponTransform.position;
        Quaternion spawnRot = WeaponTransform.rotation;
        Vector2 direction = hand.up;

        if (attackType != EnemyAttackType.Trap_Laser && attackType != EnemyAttackType.Laser)
        {
            BattleManager.Instance.SpawnBulletAtPosition(
                GetBulletType(),          // 총알 종류 enum
                spawnPos,
                spawnRot,
                direction,
                prefabName,
                0,      // size
                0f,     // delay
                false,  // isFriend
                5f,     // maxRange
                bulletSpeed,
                1f,     // accuracy
                1f      // damage
            );

            if(bulletPrefabName == "BARK")
            SoundManager.Instance.SFXPlay("shotgun_shot_01", 102);
            else
            SoundManager.Instance.SFXPlay("shotgun_shot_01", 218);
            weaponData.current_magazine = weaponData.magazine;
        }
        else
        {
            FireLaser();
        }

    }
    void FireLaser()
    {
        if (currentLaser != null) return;

        currentLaser = Instantiate(laserPrefab, WeaponTransform.position, Quaternion.identity);
        LaserFadeOut laser = currentLaser.GetComponent<LaserFadeOut>();

        if (laser != null)
        {
            laser.laserOrigin = WeaponTransform;
            laser.obstacleMask = LayerMask.GetMask("Wall", "Barrier", "Player");
            laser.thickness = 0.6f;
            laser.growSpeed = 50f;
            laser.fadeDuration = 0.5f;
            laser.dotInterval = 0.2f;
            laser.autoFade = iskeepLaser; //  자동 페이드 설정
            laser.enabled = true;
        }

        currentLaser.transform.up = GetDirectionFromTrapDir(dir);

        SoundManager.Instance.SFXPlay("charge", 63);
    }


    IEnumerator DisableLaserAfterSeconds(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (currentLaser != null)
        {
         //    LaserFadeOut laser = currentLaser.GetComponent<LaserFadeOut>();
            currentLaser = null;
        }
    }
    public void DeactivateLaser()
    {
        if (currentLaser != null)
        {
            SoundManager.Instance.SFXStopLoop(226); //  반복 사운드 종료
            currentLaser = null;
        }
    }

    BulletType GetBulletType()
    {

        switch (attackType)
        {
            case EnemyAttackType.Bullet:
                return BulletType.Normal;
            case EnemyAttackType.Shotgun:
                return BulletType.Normal;
            case EnemyAttackType.Laser:
            case EnemyAttackType.Trap_Laser:
                return BulletType.Laser;
            case EnemyAttackType.Predictive:
                return BulletType.Speed;
            case EnemyAttackType.Trap_Bullet:
            case EnemyAttackType.Trap_Melee:
                return BulletType.Directional;
            default:
                return BulletType.Normal;
        }
    }
    string GetBulletPrefabName()
    {
        // 우선 무기에 이름이 지정되어 있으면 그걸 쓰고, 없으면 타입으로 분기
        if(bulletPrefabName == "BARK")
            return "BARK";

        switch (attackType)
        {
            case EnemyAttackType.Laser:
            case EnemyAttackType.Trap_Laser:
                return "Laser_Enemy";

            case EnemyAttackType.Trap_Bullet:
            case EnemyAttackType.Bullet:
                return "Enemy_None";

            case EnemyAttackType.Shotgun:
                return "Enemy_None";

            case EnemyAttackType.Sniper:
                return "Enemy_None";

            default:
                return "Enemy_None";
        }
    }
    Vector2 GetDirectionFromTrapDir(TrapDir dir)
    {
        Vector2 direction;

        switch (dir)
        {
            case TrapDir.Left: direction = Vector2.left; break;
            case TrapDir.LeftUp: direction = new Vector2(-1, 1).normalized; break;
            case TrapDir.LeftDown: direction = new Vector2(-1, -1).normalized; break;
            case TrapDir.Right: direction = Vector2.right; break;
            case TrapDir.RightUp: direction = new Vector2(1, 1).normalized; break;
            case TrapDir.RightDown: direction = new Vector2(1, -1).normalized; break;
            case TrapDir.Up: direction = Vector2.up; break;
            case TrapDir.Down: direction = Vector2.down; break;
            default: direction = Vector2.up; break;
        }

        return direction;
    }

    void RotateToTrapDirection()
    {
        Vector2 dirVector = GetDirectionFromTrapDir(dir);
        if (dirVector == Vector2.zero) return;

        // 원래는 Vector2.right → 이제는 Vector2.up 기준으로 회전 계산
        float angle = Vector2.SignedAngle(Vector2.up, dirVector);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

}
