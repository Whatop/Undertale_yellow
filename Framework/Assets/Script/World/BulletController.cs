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
    public float speed;
    public float accuracy;
    public float maxrange = 10f;
    public bool isFreind = false;

    private float gravityEffect = 0.3f;  // 포물선 중력 효과
    private float maxTurnAngle = 150f;  // 최대 회전 각도 제한
    private float homingDuration = 2f;  // 유도 지속 시간

    private float maxSpeed = 16f; // 최대 속도 제한
    private float speedIncreaseRate = 4f; // 초당 속도 증가량
    private bool isAccelerating = false;
    private bool isSplitted = false; // 분열 여부 확인

    private Vector2 initialPosition; // 총알의 초기 위치
    private Vector2 targetPosition;  // 특정 위치로 이동할 경우 사용
    private Transform target; // 유도 탄환의 타겟
    private bool hasTarget = false; // 목표 위치 여부
    private Rigidbody2D rb;

    private static readonly Dictionary<BulletType, Color> bulletColors = new Dictionary<BulletType, Color>
    {
        { BulletType.Normal, Color.white },
        { BulletType.Homing, Color.red },
        { BulletType.Spiral, Color.yellow },
        { BulletType.Split, Color.green },
        { BulletType.Directional, Color.white },
        { BulletType.Speed, Color.white },
        { BulletType.FixedPoint, Color.cyan }
    };

    private void Start()
    {
        initialPosition = transform.position;

        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().color = bulletColors[bulletType];
        }
    }

    public void InitializeBullet(Vector2 direction, float bulletSpeed, float bulletAccuracy, int bulletDamage, float maxRange,
                                 BulletType type = default, bool accelerate = false, Transform target = null)
    {
        Vector2 adjustedDirection = ApplyAccuracy(direction);
        speed = bulletSpeed;
        damage = bulletDamage;
        accuracy = bulletAccuracy;
        maxrange = maxRange;
        bulletType = type;
        isAccelerating = accelerate;

        if (target != null)
        {
            targetPosition = target.position;
            hasTarget = true;
            StartCoroutine(MoveAndNext(type));
            //스폰 위치로 이동후
        }

    }

    void Update()
    {
        switch (bulletType)
        {
            case BulletType.Homing:
                StartCoroutine(HomingMove());
              
                break;
            case BulletType.Spiral:
                SpiralMove();
                break;
            case BulletType.Split:
                if (!isSplitted) StartCoroutine(SplitBullets(3));
                break;
            case BulletType.Directional:
                DirectionalMove();
                break;
            case BulletType.Normal:
                StartCoroutine(MoveTargetPlayer());
                break;
            case BulletType.Speed:
                StartCoroutine(MoveTargetPlayer());
                StartCoroutine(IncreaseSpeedOverTime());
                break;
        }

        if (Vector2.Distance(initialPosition, transform.position) >= maxrange)
        {
            DestroyBullet();
        }
    }

    // 정확도를 적용하여 방향을 조정하는 메서드
    private Vector2 ApplyAccuracy(Vector2 direction)
    {
        float randomAngle = Random.Range(-accuracy, accuracy);
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        return rotation * direction;
    }

    // 특정 위치로 이동하는 총알
    private IEnumerator MoveAndNext(BulletType type = default)
    {
        float speed = this.speed;
        while (hasTarget && Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            yield return null;
        }
        if (type != default)
        {
            switch (bulletType)
            {
                case BulletType.Homing:
                    HomingMove();
                    break;
                case BulletType.Spiral:
                    SpiralMove();
                    break;
                case BulletType.Split:
                    if (!isSplitted) StartCoroutine(SplitBullets(3));
                    break;
                case BulletType.Directional:
                    DirectionalMove();
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
        DestroyBullet();
    }
    // 처음 플레이어 방향으로 일정 시간 동안 이동한 후, 해당 방향 유지
    private IEnumerator MoveTargetPlayer()
    {
        if (target == null)
        {
            target = GameManager.Instance.GetPlayerData().player.transform; // 플레이어를 타겟으로 설정
        }

        if (target != null)
        {
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized; // 플레이어 방향 계산
            GetComponent<Rigidbody2D>().velocity = direction * speed; // 처음 속도 설정

            yield return new WaitForSeconds(0.5f); // 0.5초 동안 플레이어 방향 유지

            // 이후에는 해당 방향을 유지하면서 직진
            GetComponent<Rigidbody2D>().velocity = direction * speed;
        }
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

    // 상하좌우 및 대각선 방향 이동
    void DirectionalMove()
    {
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;
    }

    // 유도 탄환 동작
    private IEnumerator HomingMove()
    {
        float timer = 0f;

        while (timer < homingDuration && target != null)
        {
            if (rb != null)
            {
                Vector2 currentVelocity = rb.velocity;
                Vector2 targetDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

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
            yield return null;
        }

        // 유도 종료 후 마지막 방향으로 직진
        if (rb != null)
        {
            rb.velocity = rb.velocity.normalized * speed;
        }
    }

    // 회오리 패턴
    void SpiralMove()
    {
        float angle = Time.time * 200f;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        GetComponent<Rigidbody2D>().velocity = direction * speed;


    }

    // 일정 거리 이동 후 분열
    private IEnumerator SplitBullets(int splitCount)
    {
        isSplitted = true;
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < splitCount; i++)
        {
            float angle = (360f / splitCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            GameObject newBullet = Instantiate(gameObject, transform.position, Quaternion.identity);
            newBullet.GetComponent<BulletController>().InitializeBullet(direction, speed, accuracy, damage, maxrange, BulletType.Directional);
        }
        Destroy(gameObject);
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
