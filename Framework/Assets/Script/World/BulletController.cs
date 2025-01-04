using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage;
    public float speed;
    public float accuracy;
    public float maxrange = 10f;      // 총알의 최대 사정거리
    public bool isFreind = false;

    private Vector2 initialPosition;  // 총알의 초기 위치
    private Vector2 targetPosition;   // 총알의 목표 위치
    private bool hasTarget = false;   // 목표 위치가 설정되었는지 여부

    // Start에서 초기 위치 설정
    private void Start()
    {
        initialPosition = transform.position;  // 총알의 초기 위치 설정
    }

    // 총알 초기화 메서드
    public void InitializeBullet(Vector2 direction, float bulletSpeed, float bulletAccuracy, int bulletDamage, float maxRange, Transform target = null)
    {
        // 방향에 정확도를 적용하여 조정된 방향 계산
        Vector2 adjustedDirection = ApplyAccuracy(direction);

        // 총알의 속성 설정
        speed = bulletSpeed;
        damage = bulletDamage;
        accuracy = bulletAccuracy;
        maxrange = maxRange;

        if (target != null)
        {
            targetPosition = target.position; // 목표 위치 설정
            hasTarget = true; // 목표가 설정됨
        }

        // 총알 발사
        Shoot(adjustedDirection);
    }

    // 정확도를 적용하여 방향을 조정하는 메서드
    private Vector2 ApplyAccuracy(Vector2 direction)
    {
        float randomAngle = Random.Range(-accuracy, accuracy);
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        return rotation * direction;
    }

    // 총알을 발사하는 메서드
    private void Shoot(Vector2 direction)
    {
        if (!hasTarget)
        {
            GetComponent<Rigidbody2D>().velocity = direction * speed;
        }
    }

    void Update()
    {
        // 목표 위치가 설정된 경우 해당 위치로 이동
        if (hasTarget)
        {
            Vector2 currentPosition = transform.position;
            Vector2 direction = (targetPosition - currentPosition).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            // 목표 위치에 도달하면 소멸
            if (Vector2.Distance(currentPosition, targetPosition) <= 0.1f)
            {
                DestroyBullet();
            }
        }
        else
        {
            // 총알이 최대 사정거리를 초과하면 소멸
            if (Vector2.Distance(initialPosition, transform.position) >= maxrange)
            {
                DestroyBullet();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 적과 충돌 시 처리
        if (other.CompareTag("Enemy") && isFreind && other.GetComponent<EnemyController>().objectState != ObjectState.Roll)
        {
            other.GetComponent<EnemyController>().TakeDamage(damage, other.transform.position);
            DestroyBullet();
        }
        // 플레이어와 충돌 시 처리
       // else if (other.CompareTag("Player") && !isFreind && other.GetComponent<PlayerMovement>().objectState != ObjectState.Roll)
       // {
       //     other.GetComponent<PlayerMovement>().TakeDamage(damage, other.transform.position);
       //     DestroyBullet();
       // }
        else if (other.CompareTag("Soul") && !isFreind && GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().objectState != ObjectState.Roll)
        {
            GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().TakeDamage(damage, GameManager.Instance.GetPlayerData().player.transform.position);
            DestroyBullet();
        }
       //else if (other.CompareTag("Wall"))
       //{
       //    DestroyBullet();
       //}
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
