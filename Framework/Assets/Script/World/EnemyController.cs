using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public GameObject bulletPrefab; // 총알 프리팹
    public float bulletSpeed = 10f; // 총알 발사 속도
    public Weapon EnemyWeapon;          // 현재 사용 중인 총의 정보
    public Transform WeaponTransform;  // 총 모델의 Transform
    GameManager gameManager;
    float shootCoolTime = 4;
    float curTime = 0;


    private void Awake()
    {
        gameManager = GameManager.Instance;
    }
    void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        curTime += Time.deltaTime;

        if(curTime > shootCoolTime && bulletPrefab != null)
        {
            Shoot();
            curTime = 0;
        }
    }

    public void TakeDamage(int damage)
    {
        // 적이 데미지를 받았을 때 호출되는 함수
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    void Shoot()
    {

        EnemyWeapon = gameManager.GetWeaponData();
        EnemyWeapon.current_magazine = EnemyWeapon.magazine;
        gameManager.SaveWeaponData(EnemyWeapon);
        // 총알을 생성하고 초기 위치를 총의 위치로 설정합니다.
        GameObject bullet = Instantiate(bulletPrefab, WeaponTransform.position, WeaponTransform.rotation);


        // 총알에 속도를 적용하여 발사합니다.
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = WeaponTransform.up * bulletSpeed;
    }
    void Die()
    {
        // 적이 죽었을 때 호출되는 함수
        // 적 캐릭터의 사망 효과, 드롭 아이템 등을 처리합니다.
        Destroy(gameObject);
    }
}
