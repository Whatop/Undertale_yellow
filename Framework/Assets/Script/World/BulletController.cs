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
    private float homingDuration = 2f;  // 유도 지속 시간

    private float maxSpeed = 16f; // 최대 속도 제한
    private float speedIncreaseRate = 4f; // 초당 속도 증가량
    private bool isActivated = false;
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
        rb = GetComponent<Rigidbody2D>();

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
    public void InitializeBullet(Vector2 direction, float bulletSpeed, float bulletAccuracy, int bulletDamage, float maxRange,
                                 float delay = 0,BulletType type = default, Transform target = null)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        accuracy = bulletAccuracy;
        maxrange = maxRange;
        bulletType = type;

        if (target != null)
        {
            targetPosition = target.position;
            hasTarget = true;
            StartCoroutine(MoveAndNext(type, delay, direction));
            //스폰 위치로 이동후
        }
    }

    void Update()
    {
        //if (Vector2.Distance(initialPosition, transform.position) >= maxrange)
        //{
        //    DestroyBullet();
        //}
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
    private IEnumerator MoveAndNext(BulletType type = default, float delay = 0,Vector2 dir = default)
    {
        float speed = this.speed;
        bulletType = type;
        //  목표 위치까지 이동
        while (hasTarget && Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            yield return null;
        }

        //  이동 후, 패턴 실행 전에 대기 (필요할 경우)
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        //  이동 완료 후, 패턴 실행
        ExecuteBulletPattern(type, dir);
    }

    // 패턴 실행을 위한 분리된 메서드
    private void ExecuteBulletPattern(BulletType type,Vector2 dir = default)
    {
        Debug.Log("패턴실행");
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

    //  해당 방향 유지
    private IEnumerator DirectionalMove(Vector2 moveDirection = default)
    {
        if (rb == null) yield break;

        // 기본 방향이 설정되지 않았다면 기존 속도를 유지
        if (moveDirection == Vector2.zero)
            moveDirection = rb.velocity.normalized; // 기존 속도 유지

        moveDirection = moveDirection.normalized;
        rb.velocity = moveDirection * speed;

        yield return new WaitForSeconds(0.5f); // 일정 시간 동안 이동 유지 (필요 시 조정 가능)

    }


    // 개별 유도 탄환 동작
    private IEnumerator HomingMove()
    {
        float timer = 0f;
        float gravityEffect = 0.3f;  // 포물선을 만들 중력 효과
        float maxTurnAngle = 50f;     // 한 프레임당 최대 회전 각도 제한
        Vector2 lastVelocity = rb.velocity;

        while (timer < homingDuration)
        {
            if (rb != null)
            {
                Vector2 targetDirection = ((Vector2)GameManager.Instance.GetPlayerData().position - (Vector2)transform.position).normalized;

                // 1. 포물선 효과 적용
                rb.velocity += new Vector2(0, -gravityEffect * Time.deltaTime);

                // 2. 목표 방향으로 부드럽게 회전
                float currentAngle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
                float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxTurnAngle * Time.deltaTime);

                // 3. 새로운 방향으로 속도 재설정
                Vector2 newVelocity = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)) * speed;
                rb.velocity = newVelocity;

                lastVelocity = newVelocity; // 마지막 속도 저장
            }
            timer += Time.deltaTime;
            yield return null;
        }

        //  유도 종료 후 마지막 방향 유지
        if (rb != null)
        {
            rb.velocity = lastVelocity;
        }
    }

    private IEnumerator SpiralBullets(Vector2 moveDirection)
    {
        float angle = 0;

        // 초기 이동 방향을 설정
        if (moveDirection == Vector2.zero)
            moveDirection = Vector2.right; // 기본적으로 오른쪽으로 이동

        while (true)
        {
            angle += 300 * Time.deltaTime; //  더 빠르게 회전 (값 조정 가능)

            // 회전 벡터 계산 (원 운동)
            Vector2 spiralDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // 기존 이동 방향에 회전 벡터를 더해서 나선형 이동
            Vector2 finalDirection = (moveDirection + spiralDirection).normalized;

            rb.velocity = finalDirection * speed;

            yield return null;
        }
    }

    private IEnumerator SplitBullets(int splitCount)
    {
        if (isSplitted) yield break;
        isSplitted = true;
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < splitCount; i++)
        {
            float angle = (360f / splitCount) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject newBullet = Instantiate(gameObject, transform.position, Quaternion.identity);
            BulletController bulletController = newBullet.GetComponent<BulletController>();

            if (bulletController != null)
            {
                // 분열된 총알도 즉시 이동하도록 속도 적용
                bulletController.InitializeBullet(direction, speed, accuracy, damage, maxrange, 0, bulletType);
                bulletController.rb.velocity = direction * speed; //즉시 속도 설정
            }
        }
    }
    void DestroyBullet()
    {
        Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"총알이 {other.gameObject.name}과 충돌");
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
