using UnityEngine;

public class BulletController : MonoBehaviour
{
    public int damage;
    public float speed;
    public float accuracy;
    public float maxrange = 10f;      // 총알의 최대 사정거리

    private Vector2 initialPosition;   // 총알의 초기 위치
    // 총알 초기화 메서드
    public void InitializeBullet(Vector2 direction, float bulletSpeed, float bulletAccuracy, int bulletDamage, float maxRange)
    {
        // 방향에 정확도를 적용하여 조정된 방향 계산
        Vector2 adjustedDirection = ApplyAccuracy(direction);

        // 총알의 속성 설정
        speed = bulletSpeed;
        damage = bulletDamage;
        accuracy = bulletAccuracy;
        maxrange = maxRange;
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
        // 총알을 방향과 속도에 맞게 발사하는 로직을 구현
        // 이 부분에서 Rigidbody 등을 활용하여 총알을 이동시킬 수 있음
        // 예를 들어, GetComponent<Rigidbody2D>().velocity = direction.normalized * speed;
        GetComponent<Rigidbody2D>().velocity = direction * speed;
    }

        void Update()
        {
            // 최대 사정거리를 초과하면 총알을 소멸시킴
            if (Vector2.Distance(initialPosition, transform.position) >= maxrange)
            {
                DestroyBullet();
            }
        }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 다른 오브젝트와 충돌 시 처리할 로직을 추가합니다.
        if (other.CompareTag("Enemy"))
        {
            // 예: 적에게 데미지를 입힙니다.
            other.GetComponent<EnemyController>().TakeDamage(damage);
            Debug.Log("데미지 : " + damage);
           // 총알 소멸 또는 효과 추가 등을 수행합니다.
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        // 총알 소멸 시 처리할 로직을 여기에 추가합니다.
        Destroy(gameObject);
    }
}
