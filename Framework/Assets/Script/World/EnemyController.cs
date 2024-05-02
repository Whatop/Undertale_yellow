using UnityEngine;

public class EnemyController : LivingObject
{
    public int maxHealth = 100;
    public GameObject bulletPrefab; // 총알 프리팹
    public float bulletSpeed = 10f; // 총알 발사 속도
    public Weapon weaponData;          // 현재 사용 중인 총의 정보
    public Transform WeaponTransform;  // 총 모델의 Transform
    public Transform hand;  // 총 모델의 Transform 
    public ObjectState objectState;

    float shootCoolTime = 4;
    float curTime = 0;

    private bool undying = false;

    protected override void Awake()
    {
        base.Awake(); // LivingObject의 Awake 메서드 호출
        weaponData = new Weapon();
    }
    void Start()
    {
        health = maxHealth;
    }

    private void Update()
    {
        int curmagazine = weaponData.current_magazine;

        Vector3 playerPosition = gameManager.GetPlayerData().position;
        Vector2 direction = (playerPosition - WeaponTransform.position).normalized;
        hand.up = direction;
        curTime += Time.deltaTime;

        if (curTime > shootCoolTime && bulletPrefab != null && curmagazine > 0)
        {
            Shoot();
            weaponData.current_magazine -= 1;
            weaponData.current_Ammo -= 1;
            gameManager.SaveWeaponData(weaponData);
            curTime = 0;
        }


        // 총알이 없으면 재장전
        if (weaponData.current_Ammo < weaponData.maxAmmo &&
                 weaponData.current_magazine < weaponData.magazine)
        {

            weaponData.current_magazine = weaponData.magazine;
            gameManager.SaveWeaponData(weaponData);
        }

    }

    void Shoot()
    {

        weaponData = gameManager.GetWeaponData();
        weaponData.current_magazine = weaponData.magazine;
        gameManager.SaveWeaponData(weaponData);
        // 총알을 생성하고 초기 위치를 총의 위치로 설정합니다.
        GameObject bullet = Instantiate(bulletPrefab, WeaponTransform.position, WeaponTransform.rotation);


        // 총알에 속도를 적용하여 발사합니다.
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = hand.up * bulletSpeed;
    }
}
