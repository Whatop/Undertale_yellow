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

    private bool isBlockedByBarrier = false;
    private bool laserInitialized = false;
    private float traveledDistance = 0f;

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
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        initialPosition = transform.position;

        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().color = bulletColors[bulletType];
        }
        StartCoroutine(LifeTime());
    }
    private void OnEnable()
    {
        isHoming = false;
        isSpiral = false;
        isSplitted = false;
        isBlockedByBarrier = false;
        rb.velocity = Vector2.zero;
        hitTimer.Clear();

        // 필요하다면 scale, rotation, 색상 초기화 등도 여기서
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
                                 float delay = 0, BulletType type = default, Transform target = null,int size = 0, bool isfreind = false)
    {  // 기존 값 설정 외에...
        isHoming = false;
        isSpiral = false;
        isSplitted = false;
        rb.velocity = Vector2.zero;

        // 색상 다시 설정
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.color = bulletColors[bulletType];

        speed = bulletSpeed;
        damage = bulletDamage;
        accuracy = bulletAccuracy;
        maxrange = maxRange;
        bulletType = type;
        storedFireDirection = fireDirection;
        bulletSize= size;
        isFreind = isfreind;
        initialPosition = transform.position;

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
            ExecuteBulletPattern(type, storedFireDirection);
        }
    }

    void Update()
    {

        // 거리 누적
        traveledDistance = Vector2.Distance(transform.position, initialPosition);
        if (traveledDistance >= maxrange)
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
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        return rotation * direction;
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
                StartCoroutine(LaserBullet());
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
        GetComponent<Rigidbody2D>().velocity = direction * speed; // 처음 속도 설정

        yield return new WaitForSeconds(0.5f); // 0.5초 동안 플레이어 방향 유지

        // 이후에는 해당 방향을 유지하면서 직진
        GetComponent<Rigidbody2D>().velocity = direction * speed;

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
        //Debug.Log($"총알이 {other.gameObject.name}과 충돌");
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
        else if (other.CompareTag("Soul") && !isFreind && GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().objectState != ObjectState.Roll)
        {
            GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().TakeDamage(damage, GameManager.Instance.GetPlayerData().player.transform.position);
        }

        if (other.CompareTag("Bullet") && bulletType == BulletType.Barrier)
        {
            var bullet = other.GetComponent<BulletController>();
            if (bullet != null && !bullet.isFreind)
            {
                bullet.OnHitByShield(); // laser 포함

                // 충돌 지점 계산: 이 방어막 오브젝트 기준 가장 가까운 위치
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                EffectManager.Instance.SpawnEffect("barrier_block_flash", hitPoint, Quaternion.identity);
            }
        }
    }
    public  void DestroyBullet()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(lifeTime);
        DestroyBullet();
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

    private IEnumerator LaserBullet()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        // ① 스프라이트 실제 월드 단위 크기 (스케일=1일 때)
        float spriteUnitWidth = sr.sprite.bounds.size.x; // 예: 0.25
        float spriteUnitHeight = sr.sprite.bounds.size.y; // 예: 1

        // ② 마우스 방향으로 레이저 회전
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dirToMouse = (mouseWorld - transform.position).normalized;
        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // ③ 초기 두께(높이) 세팅
        float initialThickness = 0.6f;
        float scaleY = initialThickness / spriteUnitHeight;
        // 피벗이 오른쪽이므로, 고작 0.01만큼만 음수 스케일을 줘서 거의 보이지 않게 시작
        transform.localScale = new Vector3(-0.01f, scaleY, 1f);
        laserInitialized = true;

        float growDuration = 0.15f;
        float maxLaserLength = 20f;

        float t = 0f;
        bool didHitOnce = false;

        // ────────────────────────────────────────────────────────
        // (1) 성장 단계: growDuration 동안 “충돌 거리”를 매 프레임 계산하면서 스케일 보간
        while (t < growDuration)
        {
            float deltaTime = Time.deltaTime;
            t += deltaTime;

            // (a) 매 프레임 RaycastAll: 벽/Barrier 충돌 거리 nearestDist 계산
            Vector2 startPos = transform.position;
            Vector2 laserDir = transform.right; // (피벗이 오른쪽이므로 transform.right는 (-1,0)일 것)

            RaycastHit2D[] hitsAll = Physics2D.RaycastAll(startPos, laserDir, maxLaserLength);
            float nearestDist = maxLaserLength;
            Vector2 nearestPoint = startPos + laserDir * maxLaserLength;
            string nearestTag = "";

            foreach (var h in hitsAll)
            {
                if (h.collider == null) continue;
                if (h.collider == col) continue; // 자신 콜라이더 무시

                string tag = h.collider.tag;
                if (tag == "Wall" || tag == "Barrier")
                {
                    if (h.distance < nearestDist)
                    {
                        nearestDist = h.distance;
                        nearestPoint = h.point;
                        nearestTag = tag;
                    }
                }
            }

            // (b) “벽까지 충돌 거리” → 스케일 X로 환산 (피벗이 RIGHT이므로 음수 부호)
            float desiredScaleX = -Mathf.Abs(nearestDist / spriteUnitWidth);

            // (c) 보간 (t: 0 → growDuration 구간)
            // t == 0일 때 currentScaleX = -0.01 (초기값),
            // t == growDuration 일 때 currentScaleX = desiredScaleX(벽 위치 딱 맞춤)
            float lerpFactor = Mathf.Clamp01(t / growDuration);
            float currentScaleX = Mathf.Lerp(-0.01f, desiredScaleX, lerpFactor);

            transform.localScale = new Vector3(currentScaleX, scaleY, 1f);

            // (d) Collider 업데이트
            if (col != null)
            {
                // size는 스프라이트 1배 크기(언스케일 상태)로 설정
                col.size = new Vector2(spriteUnitWidth, spriteUnitHeight);

                // pivot=Right이므로, offset.x 를 항상 “spriteUnitWidth / 2” 로 둠
                // => 로컬 스케일이 음수일 때, 실제 Offset 위치는 (spriteUnitWidth/2 * localScale.x) 이 되어
                //    “(spriteUnitWidth/2 * -abs) = -절반길이” 가 됨 → 콜라이더 중심이 스프라이트 중앙으로 이동
                col.offset = new Vector2(spriteUnitWidth / 2f, 0f);
            }

            // (e) 첫 Wall/Barrier 충돌 이펙트(한 번만)
            if (!didHitOnce && nearestTag != "")
            {
                didHitOnce = true;
                EffectManager.Instance.SpawnEffect("barrier_flash", nearestPoint, Quaternion.identity);
            }

            yield return null;
        }

        // ────────────────────────────────────────────────────────
        // (2) 유지 구간: keepLaser == true 면, 매 프레임 충돌거리 기준으로 스케일 즉시 세팅
        didHitOnce = false;
        while (keepLaser)
        {
            Vector2 startPos = transform.position;
            Vector2 laserDir = transform.right; // 여전히 (-1,0)

            float margin = 1.05f;
            Vector2 origin = startPos + (laserDir * -margin);

            RaycastHit2D[] hitsAll = Physics2D.RaycastAll(
                origin,
                laserDir,
                maxLaserLength + margin,
                LayerMask.GetMask("Wall", "Barrier", "Enemy")
            );

            float nearestDist = maxLaserLength + margin;
            Vector2 nearestPoint = origin + laserDir * (maxLaserLength + margin);
            string nearestTag = "";

            foreach (var h in hitsAll)
            {
                if (h.collider == null) continue;
                if (h.collider == col) continue;

                string tag = h.collider.tag;
                if (tag == "Wall" || tag == "Barrier")
                {
                    if (h.distance < nearestDist)
                    {
                        nearestDist = h.distance;
                        nearestPoint = h.point;
                        nearestTag = tag;
                    }
                }
            }

            // (a) 즉시 스케일 적용 (grow 단계와 달리 보간 없이 바로 적용)
            float targetScaleX = -Mathf.Abs(nearestDist / spriteUnitWidth);
            transform.localScale = new Vector3(targetScaleX, scaleY, 1f);

            // (b) Collider 업데이트 (pivot=Right 이므로 offset.x = spriteUnitWidth/2 고정)
            if (col != null)
            {
                col.size = new Vector2(spriteUnitWidth, spriteUnitHeight);
                col.offset = new Vector2(spriteUnitWidth / 2f, 0f);
            }

            // (c) 벽/Barrier에 안 닿았으면 적 관통
            if (nearestTag != "Wall" && nearestTag != "Barrier")
            {
                RaycastHit2D[] enemyHits = Physics2D.RaycastAll(startPos, laserDir, nearestDist);
                foreach (var eh in enemyHits)
                {
                    if (eh.collider == col) continue;

                    if (eh.collider.tag == "Enemy")
                    {
                        var enemy = eh.collider.GetComponent<EnemyController>();
                        if (enemy != null)
                            enemy.TakeDamage(damage * Time.deltaTime);
                    }
                }
            }

            // (d) 한 번만 벽 충돌 이펙트
            if (!didHitOnce && nearestTag != "")
            {
                didHitOnce = true;
                EffectManager.Instance.SpawnEffect("barrier_flash", nearestPoint, Quaternion.identity);
            }

            yield return null;
        }

        // (3) 레이저 종료
        DestroyBullet();
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
