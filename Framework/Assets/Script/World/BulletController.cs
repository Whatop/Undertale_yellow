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
    Speed // 점점 빨라지는
}

public class BulletController : MonoBehaviour
{
    public BulletType bulletType = BulletType.Normal; // 총알의 타입
    public int damage;
    public float speed = 5;
    public float accuracy;
    public float maxrange = 10f;
    public bool isFreind = false;

    private float gravityEffect = 0.3f;  // 포물선 중력 효과
    private float maxTurnAngle = 150f;  // 최대 회전 각도 제한
    private float homingDuration = 5f;  // 유도 지속 시간

    private float maxSpeed = 16f; // 최대 속도 제한
    private float speedIncreaseRate = 4f; // 초당 속도 증가량
    private bool isActivated = false;
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


    private static readonly Dictionary<BulletType, Color> bulletColors = new Dictionary<BulletType, Color>
    {
        { BulletType.Normal, Color.white },
        { BulletType.Homing, Color.red },
        { BulletType.Spiral, Color.yellow },
        { BulletType.Split, Color.green },
        { BulletType.Directional, Color.white },
        { BulletType.Speed, Color.white },
        { BulletType.FixedPoint, Color.cyan }
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

        StartPattern(); // 처음 시작할 때 실행
    }
    // 처음 한 번만 패턴을 실행
    private void StartPattern()
    {
        if (isActivated) return; // 한 번만 실행되도록 제한
        isActivated = true;

        switch (bulletType)
        {
            case BulletType.Homing:
                StartCoroutine(HomingMove());
                break;
            case BulletType.Spiral:
                StartCoroutine(MoveTargetPlayer());
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
    public void InitializeBullet(Vector2 fireDirection, float bulletSpeed, float bulletAccuracy, int bulletDamage, float maxRange,
                                 float delay = 0,BulletType type = default, Transform target = null)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        accuracy = bulletAccuracy;
        maxrange = maxRange;
        bulletType = type;
        storedFireDirection = fireDirection;

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
    if (!isActivated) return;

    switch (bulletType)
    {
        case BulletType.Homing:
                if(isHoming)
            UpdateHoming(); // ← 매 프레임 동작
            break;

        case BulletType.Spiral:
                if(isSpiral)
            UpdateSpiral(); // ← 회전 반경 커지도록 설정
                break;
    }
}
    // Homing 탄환 매 프레임 추적
    private void UpdateHoming()
    {
        float gravityEffect = 0.3f;  // 포물선을 만들 중력 효과
        float maxTurnAngle = 50f;     // 한 프레임당 최대 회전 각도 제한 (값이 크면 급격히 회전)
        float timer = 0f;

        if (timer < homingDuration)
        {
            if (rb != null && GameManager.Instance.GetPlayerData().player != null)
            {
                Vector2 currentVelocity = rb.velocity;
                Vector2 targetDirection = ((Vector2)GameManager.Instance.GetPlayerData().position - (Vector2)transform.position).normalized;

                // 1. 포물선 효과: Y축 속도에 중력 적용
                Vector2 gravity = new Vector2(0, -gravityEffect * Time.deltaTime);
                currentVelocity += gravity;

                // 2. 방향 제한을 두면서 부드럽게 회전
                float currentAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
                float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                // 현재 각도를 목표 각도 방향으로 부드럽게 회전 (각도 제한)
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxTurnAngle * Time.deltaTime);

                // 새로운 방향으로 속도 재설정
                Vector2 newVelocity = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)) * speed;
                rb.velocity = newVelocity;
            }
            timer += Time.deltaTime;
        }
        else
        {
            rb.velocity = rb.velocity.normalized * speed; // 마지막 방향으로 유지
            isHoming = false;
        }
    }
        private void UpdateSpiral()
    {
        if (rb == null) return;

        spiralAngle += 300 * Time.deltaTime;
        spiralRadius += 0.2f * Time.deltaTime; // ← 점점 커지는 반경

        Vector2 spiral = new Vector2(Mathf.Cos(spiralAngle), Mathf.Sin(spiralAngle)) * spiralRadius;
        Vector2 direction = spiral.normalized;

        rb.velocity = direction * speed;
    }

    public void ChangeBulletType(BulletType newType)
    {
        bulletType = newType;
        isActivated = false; // 새로운 타입이 설정되었으므로 다시 실행 가능하도록 변경

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
        float elapsed = 0f;
        float timeout = 3f; // 최대 3초 동안만 이동 시도

        while (hasTarget && Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 newPos = rb.position + direction * speed * 5f * Time.deltaTime;
            rb.MovePosition(newPos); // ← 이걸로 이동
            yield return null;
        }


        // 유도 실패 상황 예외 처리
        if (elapsed >= timeout)
        {
            Debug.LogWarning("MoveAndNext 타임아웃! 위치로 도달하지 못했어요.");
        }

        if (delay > 0)
            yield return new WaitForSeconds(delay);

        ExecuteBulletPattern(type, storedFireDirection);
    }


    // 패턴 실행을 위한 분리된 메서드
    private void ExecuteBulletPattern(BulletType type,Vector2 dir = default)
    {
        switch (type)
        {
            case BulletType.Homing:
                StartCoroutine(HomingMove());
                break;
            case BulletType.Spiral:
                StartCoroutine(SpiralBullets(dir));
                break;
            case BulletType.Split:
                if (!isSplitted) StartCoroutine(SplitBullets(3));
                break;
            case BulletType.Directional:
                StartCoroutine(DirectionalMove(dir));
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

    private IEnumerator SpiralBullets(Vector2 moveDirection)
    {
        isSpiral = true;
        float angle = 0f;
        if (moveDirection == Vector2.zero)
                moveDirection = ((Vector2)GameManager.Instance.GetPlayerData().player.transform.position - (Vector2)transform.position).normalized;
       
        while (true)
        {
            angle += 300 * Time.deltaTime;
            Vector2 spiral = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 finalDir = (moveDirection + spiral).normalized;
            rb.velocity = finalDir * speed;
            yield return null;
        }
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
    void DestroyBullet()
    {
        Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"총알이 {other.gameObject.name}과 충돌");
        if (other.CompareTag("Enemy") && isFreind && other.GetComponent<EnemyController>().objectState != ObjectState.Roll)
        {
            other.GetComponent<EnemyController>().TakeDamage(damage, other.transform.position);
            DestroyBullet();
        }
        else if (other.CompareTag("Soul") && !isFreind && GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().objectState != ObjectState.Roll)
        {
            GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().TakeDamage(damage, GameManager.Instance.GetPlayerData().player.transform.position);
            DestroyBullet();
        }
    }

}
