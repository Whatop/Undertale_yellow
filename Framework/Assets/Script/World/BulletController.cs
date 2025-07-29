using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum BulletType
{
    Normal,     // 기본 총알
    Homing,     // 유도 총알
    Spiral,     // 회오리 총알
    Split,      // 분열 총알
    Directional,// 방향 지정 총알
    FixedPoint,  // 특정 위치로 이동하는 총알
    GasterBlaster,
    Laser,
    Barrier,
    Speed, // 점점 빨라지는
    None
}

public class BulletController : MonoBehaviour
{
    public BulletType bulletType = BulletType.Normal; // 총알의 타입
    public float damage;
    public float speed = 5;
    public float accuracy;
    public float maxrange = 10f;
    public bool isFreind = false;

    private float maxLifetime = 10f; 
    private float gravityEffect = 0.3f;  // 포물선 중력 효과
    private float maxTurnAngle = 150f;  // 최대 회전 각도 제한
    private float homingDuration = 5f;  // 유도 지속 시간
    private float lifeTime = 30f;

    private float maxSpeed = 16f; // 최대 속도 제한
    private float speedIncreaseRate = 4f; // 초당 속도 증가량
    private bool isSplitted = false; // 분열 여부 확인
    private bool isHoming = false; // 추격 여부 확인
    private bool isSpiral = false; // 회전 여부 확인

    private Vector2 initialPosition; // 총알의 초기 위치
    private Vector2 targetPosition;  // 특정 위치로 이동할 경우 사용
    private Vector2 storedFireDirection;
    private Transform target; // 유도 탄환의 타겟
    private bool hasTarget = false; // 목표 위치 여부
    private Rigidbody2D rb;
    // Spiral 탄환 매 프레임 나선형 증가
    private float spiralAngle = 0f;
    private float spiralRadius = 0.5f;
    private float bulletSize = 0;

    private bool isLaser = false;
    public bool isPiercing = false; // 관통 여부
    private Dictionary<GameObject, float> hitTimer = new Dictionary<GameObject, float>();
    public float dotInterval = 0.2f; // 적당한 도트딜 간격

    // 추가: 원하는 이동 시간(초)와 회전 속도 계수(배율)
    public float travelTime = 1.2f;      // 총 이동에 걸릴 시간(거리 무관) – 늘릴수록 느려짐
    public float rotationMultiplier = 0.8f; // 1 = 한 바퀴, 0.5 = 반 바퀴, 2 = 두 바퀴
    [HideInInspector]
    public bool keepLaser = false; // 플레이어가 버튼을 떼기 전까지 true로 유지
    public float maxLaserDistance = 20f;
    public LineRenderer lineRenderer;  // 레이저 전용 컴포넌트
    public LayerMask hitMask;          // 레이저 충돌용 레이어
    private bool isFiringLaser = false;
    private bool didHitOnce = false;

    private bool isBlockedByBarrier = false;
    private bool laserInitialized = false;
    private float traveledDistance = 0f;
    private bool didHitBarrierOnce = false;
    private Coroutine lifeTimeRoutine;

    private bool isHeal = false;

    private static readonly Dictionary<BulletType, Color> bulletColors = new Dictionary<BulletType, Color>
    {
        { BulletType.Normal, Color.white },
        { BulletType.Homing, Color.red },
        { BulletType.Spiral, Color.yellow },
        { BulletType.Split, Color.green },
        { BulletType.Barrier, Color.white },
        { BulletType.Directional, Color.white },
        { BulletType.Speed, Color.white },
        { BulletType.FixedPoint, Color.cyan },
        { BulletType.GasterBlaster, Color.white },
        { BulletType.Laser, Color.white },
        { BulletType.None, Color.white }
    }
    ; private void Awake()
    {
        if (bulletType == BulletType.Laser)
        {
            // 레이저 타입이라면 LineRenderer가 반드시 붙어 있어야 함
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                Debug.LogError("Laser 타입에 LineRenderer를 붙여주세요.");
            // LineRenderer 초기 세팅
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = false;  // 부모 오브젝트 로컬 좌표로 제어
            lineRenderer.enabled = false;        // 발사 직전에 켜둠
        }
        
            rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        initialPosition = transform.position;

        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().color = bulletColors[bulletType];
        }
    }
    private void OnEnable()
    {
        isHoming = false;
        isSpiral = false;
        isSplitted = false;
        isBlockedByBarrier = false;
        rb.velocity = Vector2.zero;
        hitTimer.Clear();
        keepLaser = false;
        didHitBarrierOnce = false;
        if (lineRenderer != null)
            lineRenderer.enabled = false;
        if (bulletColors.TryGetValue(bulletType, out Color color))
        {
            if (GetComponent<SpriteRenderer>() != null)
            {
                GetComponent<SpriteRenderer>().color = color;
            }
        }
    }
    private void OnDisable()
    {
        if (lifeTimeRoutine != null)
            StopCoroutine(lifeTimeRoutine);
        lifeTimeRoutine = null;
    }
    public void StartBulletLife(float duration)
    {
        if (lifeTimeRoutine != null)
            StopCoroutine(lifeTimeRoutine);

        lifeTimeRoutine = StartCoroutine(LifeTime(duration));
    }

    IEnumerator LifeTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        DestroyBullet(); // SetActive(false)
    }
    private void StartPattern()
    {

        if (!isFreind)//적
        {
            switch (bulletType)
            {
                case BulletType.Homing:
                    StartCoroutine(HomingMove());
                    StartCoroutine(MoveTargetPlayer());
                    break;
                case BulletType.Spiral:
                    StartCoroutine(SpiralBullets());
                    break;
                case BulletType.Split:
                    if (!isSplitted) StartCoroutine(SplitBullets(3));
                    break;
                case BulletType.Normal:
                    StartCoroutine(MoveTargetPlayer());
                    break;
                case BulletType.Speed:
                    StartCoroutine(MoveTargetPlayer());
                    StartCoroutine(IncreaseSpeedOverTime());
                    break;
            }
        }
        else //플레이어
        {

        }
    }
    public void InitializeBullet(Vector2 fireDirection, float bulletSpeed, float bulletAccuracy, float bulletDamage, float maxRange,
                                 float delay = 0, BulletType type = default, Transform target = null, int size = 0, bool isfreind = false, bool isheal = false)
    {  // 기존 값 설정 외에...
        this.isHoming = false;
        this.isSpiral = false;
        this.isSplitted = false;
        this.rb.velocity = Vector2.zero;
        this.bulletType = type;
        this.isHeal = false;

        // 색상 다시 설정
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = bulletColors[bulletType];

        this.speed = bulletSpeed;
        this.damage = bulletDamage;
        this.accuracy = bulletAccuracy;
        this.maxrange = maxRange;
        this.storedFireDirection = ApplyAccuracy(fireDirection);
        this.bulletSize = size;
        this.isFreind = isfreind;
        this.isHeal = isheal;
        initialPosition = transform.position;
       
        // 총알 색상 재설정
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && bulletColors.TryGetValue(bulletType, out Color baseColor))
        {
            sr.color = baseColor;
            if (this.isHeal)
                sr.color = Color.green;  // 회복 탄환은 항상 초록색
        }

        // ✅ 기존 코루틴 정리 후 새로 시작
        if (lifeTimeRoutine != null)
            StopCoroutine(lifeTimeRoutine);

        lifeTimeRoutine = StartCoroutine(LifeTime(lifeTime));
        if (target != null)
        {
            targetPosition = target.position;
            hasTarget = true;
            Debug.Log($"{bulletType} bullet 대기 위치: {targetPosition} / target 이름: {target.name}");

            StartCoroutine(MoveAndNext(type, delay));
            //스폰 위치로 이동후
        }
        else
        {
            if (type != BulletType.Laser)
            {
                ExecuteBulletPattern(type, storedFireDirection); // 기존 로직
            }
            else
            {
                // 레이저의 경우, velocity는 0으로 두고 코루틴에서 처리
                rb.velocity = Vector2.zero;
                keepLaser = true;                  // 발사 직후 true 설정
                ExecuteBulletPattern(type, storedFireDirection);
            }
        }
    }

    void Update()
    {

        // 거리 누적
        traveledDistance = Vector2.Distance(transform.position, initialPosition);
        if (traveledDistance >= maxrange&&bulletType != BulletType.GasterBlaster 
            && bulletType != BulletType.Homing )
        {
            DestroyBullet(); // 또는 gameObject.SetActive(false);
        }
        switch (bulletType)
        {
            case BulletType.Homing:
                if (isHoming)
                    UpdateHoming(); // ← 매 프레임 동작
                break;

            case BulletType.Spiral:
                if (isSpiral)
                    UpdateSpiral(); // ← 회전 반경 커지도록 설정
                break;

            case BulletType.Laser:
                if (keepLaser)
                    UpdateLaserLocal();
                break;
        }
    }

    // Homing 탄환 매 프레임 추적
    private void UpdateHoming()
    {
        float gravityEffect = 0.3f;
        float maxTurnAngle = 50f;
        float timer = 0f;

        if (timer < homingDuration)
        {
            if (rb != null)
            {
                Vector2 currentVelocity = rb.velocity;
                Vector2 targetDirection;

                if (isFreind)
                {
                    // 아군 총알: 적 타겟팅
                    Transform closestEnemy = GetClosestEnemy();
                    if (closestEnemy != null)
                        targetDirection = ((Vector2)closestEnemy.position - (Vector2)transform.position).normalized;
                    else
                        targetDirection = storedFireDirection.normalized;
                }
                else
                {
                    // 적 총알: 플레이어 타겟팅
                    Transform player = GameManager.Instance.GetPlayerData().player?.transform;
                    if (player != null)
                        targetDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
                    else
                        targetDirection = storedFireDirection.normalized;
                }

                Vector2 gravity = new Vector2(0, -gravityEffect * Time.deltaTime);
                currentVelocity += gravity;

                float currentAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
                float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxTurnAngle * Time.deltaTime);

                Vector2 newVelocity = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)) * speed;
                rb.velocity = newVelocity;
            }
            timer += Time.deltaTime;
        }
        else
        {
            rb.velocity = rb.velocity.normalized * speed;
            isHoming = false;
        }
    }
    private Transform GetClosestEnemy()
    {
        float closestDistance = float.MaxValue;
        Transform closest = null;

        foreach (GameObject enemy in BattleManager.Instance.curEnemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }


    private void UpdateSpiral()
    {
        if (rb == null) return;

        // 1) 각도를 '도'에서 '라디안'으로 변환하여 증가시키는 예시
        spiralAngle += 300f * Mathf.Deg2Rad * Time.deltaTime; // 초당 300도 회전
     
                                                                    // 3) 스파이럴 벡터 계산 (이미 라디안으로 cos/sin 사용)
        Vector2 spiral = new Vector2(
            Mathf.Cos(spiralAngle),
            Mathf.Sin(spiralAngle)
        ) * spiralRadius;

        // 4) velocity 지정 (Time.deltaTime은 빼고 speed만 곱)
        rb.velocity = spiral * speed;
    }

    public void ChangeBulletType(BulletType newType)
    {
        bulletType = newType;

        // 색상 변경
        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().color = bulletColors[newType];
        }

        StartPattern(); // 새로운 패턴 실행
    }

    // 정확도를 적용하여 방향을 조정하는 메서드
    private Vector2 ApplyAccuracy(Vector2 direction)
    {
        float randomAngle = Random.Range(-accuracy, accuracy);
        return Quaternion.AngleAxis(randomAngle, Vector3.forward) * direction;
    }


    // 특정 위치로 이동 후 패턴 실행
    private IEnumerator MoveAndNext(BulletType type = default, float delay = 0)
    {
        // 1) 시작 위치·도착 위치·초기 Z각도 캐싱
        Vector3 startPos = transform.position;
        Vector3 endPos = targetPosition;
        float initialZ = transform.eulerAngles.z;

        // 2) travelTime 동안 이동하도록 시간 고정
        float elapsed = 0f;
        if (type == BulletType.GasterBlaster)
            travelTime = 0.5f;

        while (elapsed < travelTime)
        {
            float progress = elapsed / travelTime;              // 0 → 1, 선형 진행
            transform.position = Vector3.Lerp(startPos, endPos, progress);

            if (type == BulletType.GasterBlaster)
            {
                // 3) 이동하면서 회전
                float newZ = initialZ + 360f * rotationMultiplier * progress;
                transform.rotation = Quaternion.Euler(0f, 0f, newZ);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4) 최종 위치·각도 보정
        transform.position = endPos;
        if (type == BulletType.GasterBlaster)
            transform.rotation = Quaternion.Euler(0f, 0f, initialZ + 360f * rotationMultiplier);

        // 5) 이후 로직
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        if (type == BulletType.GasterBlaster)
            GetComponent<GasterBlaster>()?.Shot();

        if (type != BulletType.None)
            ExecuteBulletPattern(type, storedFireDirection);
    }

    // 패턴 실행을 위한 분리된 메서드
    private void ExecuteBulletPattern(BulletType type, Vector2 dir = default)
    {
        switch (type)
        {
            case BulletType.Homing:
                StartCoroutine(DirectionalMove(dir));
                StartCoroutine(HomingMove());
                break;
            case BulletType.Spiral:
                StartCoroutine(SpiralBullets());
                break;
            case BulletType.Split:
                if (!isSplitted) StartCoroutine(SplitBullets(3));
                break;
            case BulletType.Normal:
                StartCoroutine(MoveTargetPlayer());
                break;
            case BulletType.Speed:
                StartCoroutine(MoveTargetPlayer());
                StartCoroutine(IncreaseSpeedOverTime());
                break;
            case BulletType.Directional:
                StartCoroutine(DirectionalMove(dir));
                break;
            case BulletType.GasterBlaster:
                StartCoroutine(LaserCheck());
                break;
            case BulletType.Barrier:
                StartCoroutine(BarrierMoveAndStay());
                break;
            case BulletType.Laser:
                // 레이저를 즉시 활성화하거나, 만약 일정 시간 지속하거나 Raycast 등 처리가 필요하다면
                isLaser = true;
                isFiringLaser = true;
                lineRenderer.useWorldSpace = false; 
                if (lineRenderer != null) lineRenderer.enabled = true;
                break;

            case BulletType.None:
                Debug.Log("총알대기");
                break;
        }
    }

    // 처음 플레이어 방향으로 일정 시간 동안 이동한 후, 해당 방향 유지
    private IEnumerator MoveTargetPlayer()
    {
        target = GameManager.Instance.GetPlayerData().player.transform; // 플레이어를 타겟으로 설정

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized; // 플레이어 방향 계산
                                                                                                 // 원래 방향 계산
        Vector2 rawDir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        // 보정된 방향
        Vector2 dir = ApplyAccuracy(rawDir);

        rb.velocity = dir * speed;
        yield return new WaitForSeconds(0.5f);
        rb.velocity = dir * speed;

    }

    // 점점 빨라지는 총알
    private IEnumerator IncreaseSpeedOverTime()
    {
        while (speed < maxSpeed)
        {
            speed += speedIncreaseRate * Time.deltaTime;
            speed = Mathf.Clamp(speed, 0, maxSpeed);
            yield return null;
        }
    }

    //  해당 방향 유지
    private IEnumerator DirectionalMove(Vector2 moveDirection)
    {
        if (rb == null) yield break;

        // 안전장치: 방향이 없다면 기존 속도 유지 or 기본값
        if (moveDirection == Vector2.zero)
            moveDirection = rb.velocity != Vector2.zero ? rb.velocity.normalized : Vector2.right;

        rb.velocity = moveDirection.normalized * speed;

        yield return new WaitForSeconds(0.5f);
        rb.velocity = moveDirection.normalized * speed;
    }

    // 개별 유도 탄환 동작
    private IEnumerator HomingMove()
    {
        isHoming = true;
        yield return null;
    }

    private IEnumerator SpiralBullets()
    {
           isSpiral = true;
        switch (bulletSize)
        {
            case 0:
                spiralRadius = 1.75f; // 작은원
                break;
            case 1:
                spiralRadius = 2.75f; // 중간원
                break;
            case 2:
                spiralRadius = 3.75f; // 큰원
                break;
        }

        yield return null;
    }


    private IEnumerator LaserCheck()
    {
        isLaser = true;
        //rb.velocity = storedFireDirection * speed;

        yield return null;

        isLaser = false;
    }

    private IEnumerator SplitBullets(int splitCount)
    {
        if (isSplitted) yield break;
        isSplitted = true;
        StartCoroutine(MoveTargetPlayer());
        yield return new WaitForSeconds(5f);

        for (int i = 0; i < splitCount; i++)
        {
            float angle = (360f / splitCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject newBullet = Instantiate(gameObject, transform.position, Quaternion.identity);
            BulletController bulletController = newBullet.GetComponent<BulletController>();
            bulletController.InitializeBullet(direction, speed, accuracy, damage, maxrange, 0, BulletType.Directional);

        }
        DestroyBullet();
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isFreind)
        {
            GameObject enemy = other.gameObject;

            if (!hitTimer.ContainsKey(enemy))
            {
                hitTimer[enemy] = Time.time;
                enemy.GetComponent<EnemyController>().TakeDamage(damage);
            }
            else
            {
                if (Time.time - hitTimer[enemy] >= dotInterval)
                {
                    hitTimer[enemy] = Time.time;
                    enemy.GetComponent<EnemyController>().TakeDamage(damage);
                }
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // **회복 탄환 처리**: 플레이어나 적에게 닿으면 체력 회복
        if (isHeal)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("Soul"))
            {
                LivingObject target = other.GetComponent<LivingObject>();
                if (target != null)
                {
                    target.Heal(damage);  // 데미지 대신 해당 값만큼 체력 회복
                }
            }
            DestroyBullet();
            return;  // 회복 탄환은 다른 충돌 로직 처리하지 않음
        }

        // **일반 탄환 처리**: 아군 탄환 -> 적에게 데미지 / 적 탄환 -> 플레이어에게 데미지
        if (other.CompareTag("Enemy") && isFreind)
        {
            LivingObject enemy = other.GetComponent<LivingObject>();
            if (enemy != null)
                enemy.TakeDamage(damage);
            if (bulletType != BulletType.GasterBlaster && bulletType != BulletType.Barrier)
                DestroyBullet();
        }
        else if (other.CompareTag("Soul") && !isFreind)
        {
            LivingObject player = other.GetComponent<LivingObject>();
            ObjectState state = GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().objectState;
            if (player != null && state != ObjectState.Roll)
                player.TakeDamage(damage);
            if (bulletType != BulletType.GasterBlaster && bulletType != BulletType.Barrier)
                DestroyBullet();
        }

        // **방어막 탄환 처리**: 적 탄환을 막고 반사
        if (other.CompareTag("Bullet") && bulletType == BulletType.Barrier)
        {
            BulletController bullet = other.GetComponent<BulletController>();
            if (bullet != null && !bullet.isFreind)
            {
                bullet.OnHitByShield();  // 레이저 포함한 탄환 소멸/반사 처리
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                EffectManager.Instance.SpawnEffect("barrier_block_flash", hitPoint, Quaternion.identity);
            }
        }
    }
    public  void DestroyBullet()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator BarrierMoveAndStay()
    {
        // 1) 초기 속도 설정
        Vector2 direction = storedFireDirection.normalized;
        rb.velocity = direction * speed;

        float moveDuration = 0.3f;  // 이동 시간
        float slowDownDuration = 0.4f; // 감속 시간
        float stayDuration = 2.5f; // 멈춘 뒤 존재 시간

        // 2) 일정 시간 동안 일정 속도 유지
        yield return new WaitForSeconds(moveDuration);

        // 3) 점점 느려지기
        float elapsed = 0f;
        Vector2 currentVelocity = rb.velocity;
        while (elapsed < slowDownDuration)
        {
            rb.velocity = Vector2.Lerp(currentVelocity, Vector2.zero, elapsed / slowDownDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4) 정지 후 유지
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(stayDuration);

        // 5) 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// (Local Space 모드) 매 프레임 레이저 길이 계산 & DoT 처리
    /// </summary>
    private void UpdateLaserLocal()
    {
        // 1) LineRenderer 활성화
        if (!lineRenderer.enabled)
            lineRenderer.enabled = true;

        // 2) 월드 좌표 기준으로 Raycast 거리 계산
        Vector3 startWorld = transform.position;   // 총구 위치 (부모(ShotPoint)의 월드 좌표)
        Vector3 dirWorld = transform.right;         // 부모 회전(–90°)을 반영해 화면 우측을 가리킴

        // Raycast 시작점 살짝 앞으로 밀어 셀프 히트 방지
        Vector3 originWorld = startWorld + dirWorld ;
        float rayLength = maxLaserDistance;
        RaycastHit2D[] hits = Physics2D.RaycastAll(originWorld, dirWorld, rayLength, hitMask);

        // 3) 가장 가까운 Barrier/Wall까지 거리 계산
        float nearestBarrierDist = rayLength;
        Vector3 nearestBarrierPoint = originWorld + dirWorld * rayLength;
        bool barrierFound = false;

        // 4) Enemy 히트 정보 저장
        List<RaycastHit2D> enemyHits = new List<RaycastHit2D>();

        foreach (var h in hits)
        {
            if (h.collider == null)
                continue;

            if (h.distance < 0.1f)
                continue;
            // (Collider2D를 쓰지 않으므로) 자기 자신 무시
            if (h.collider.gameObject == this.gameObject)
                continue;

            if (h.collider.CompareTag("Wall") || h.collider.CompareTag("Barrier"))
            {
                if (h.distance < nearestBarrierDist)
                {
                    nearestBarrierDist = h.distance;
                    nearestBarrierPoint = h.point;
                    barrierFound = true;
                }
            }
            else if (h.collider.CompareTag("Enemy") && isFreind)
            {
                enemyHits.Add(h);
            }
            else if (h.collider.CompareTag("Player") && !isFreind)
            {
                enemyHits.Add(h);
            }

        }

        
        // 7) 적에게 DoT(지속 데미지) 처리
        if (barrierFound)
        {
            // Barrier 앞에 있는 Enemy들만 처리
            foreach (var eh in enemyHits)
            {
                if (eh.distance < nearestBarrierDist)
                {
                    ApplyDotToEnemy(eh.collider.gameObject);
                }
            }
        }
        else
        {
            // Barrier 없으면 구간 내 모든 Enemy에게 처리
            foreach (var eh in enemyHits)
            {
                ApplyDotToEnemy(eh.collider.gameObject);
            }
        }

        // (6) 이펙트(Barrier Flash) 보여 주기
        if (barrierFound && !didHitBarrierOnce)
        {
            EffectManager.Instance.SpawnEffect("barrier_flash", nearestBarrierPoint, Quaternion.identity);
        }

        // (7) 레이저 길이 결정: Barrier를 찾았으면 “시작점(startWorld)~nearestBarrierPoint” 거리, 아니면 maxLaserDistance
        float actualDist = barrierFound
            ? Vector3.Distance(startWorld, nearestBarrierPoint)
            : maxLaserDistance;

        // (8) LineRenderer (UseWorldSpace = false 모드)로 그릴 때
        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, new Vector3(actualDist*2f, 0f, 0f));
    }

    /// <summary>
    /// 한 적에게 dotInterval 간격으로만 데미지 주도록 관리
    /// </summary>
    private void ApplyDotToEnemy(GameObject enemyObj)
    {
        if (!hitTimer.ContainsKey(enemyObj) || Time.time - hitTimer[enemyObj] >= dotInterval)
        {
            hitTimer[enemyObj] = Time.time;
            var living = enemyObj.GetComponent<LivingObject>();
            if (living != null)
            {
                // damage를 “초당 데미지”로 가정할 경우
                living.TakeDamage(damage);
                // 만약 1회당 고정 데미지면 `damage`만 넘겨도 됩니다.
            }
        }
    }

    /// <summary>
    /// 외부(PlayerMovement)에서 레이저 발사 시작 시 호출
    /// </summary>
    public void FireLaser()
    {
        hitTimer.Clear();
        didHitBarrierOnce = false;
        keepLaser = true;
    }

    /// <summary>
    /// 외부(PlayerMovement)에서 레이저 멈춤 시 호출
    /// </summary>
    public void StopLaser()
    {
        keepLaser = false;
        if (lineRenderer != null)
            lineRenderer.enabled = false;
        // 풀링을 사용한다면 SetActive(false)로,
        // 아니라면 Destroy(gameObject);
        Destroy(gameObject);
    } 


public void OnHitByShield()
    {
        if (isBlockedByBarrier) return;
        isBlockedByBarrier = true;

        switch (bulletType)
        {
            case BulletType.Normal:
            case BulletType.Directional:
            case BulletType.Speed:
            case BulletType.Spiral:
            case BulletType.Split:
            case BulletType.Homing:
                SoundManager.Instance.SFXPlay("barrier_block", 97);
                gameObject.SetActive(false); break;

            case BulletType.Laser:
            case BulletType.GasterBlaster:
                // 방어막 막힘 처리 (파괴는 하지 않음)
                break;
            default:
                gameObject.SetActive(false); break;
        }
    }



}
