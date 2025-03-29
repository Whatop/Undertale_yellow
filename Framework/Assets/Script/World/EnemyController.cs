using UnityEngine;

public class EnemyController : LivingObject
{
    public GameObject bulletPrefab; // 총알 프리팹
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

    private bool undying = false;

    protected override void Awake()
    {
        base.Awake(); // LivingObject의 Awake 메서드 호출
        weaponData = new Weapon();
        //animator.GetComponent<Animator>();
    }

     void Start()
    {
        maxHealth = 10;
        health = maxHealth;
        speed = 2;
    }

    protected override void Update()
    {
        base.Update();
        if (!isDie)
        {
            float distanceToPlayer = Vector2.Distance(gameManager.GetPlayerData().position, transform.position);

            if (distanceToPlayer > maxDistance && isMove)
            {
                ChasePlayer();
            }
            else if (distanceToPlayer < minDistance)
            {
                MoveAwayFromPlayer();
            }
            else
            {
                StopMoving();
            }

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
                curTime = 0;
                SoundManager.Instance.SFXPlay("shotgun_shot_01", 218); // 총 사운드

            }

            // 총알이 없으면 재장전
            if (weaponData.current_Ammo < weaponData.maxAmmo &&
                weaponData.current_magazine < weaponData.magazine)
            {
                weaponData.current_magazine = weaponData.magazine;
            }
        }
        else
            StopMoving();

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
       weaponData.current_magazine = weaponData.magazine;

        BattleManager.Instance.SpawnBulletAtPosition(
      BulletType.Normal,
      WeaponTransform.position,
      WeaponTransform.rotation,
      hand.up,
      "Enemy_None"
      ,0,0,false
  );

        // weaponData.current_magazine = weaponData.magazine;
        //
        // // 총알을 생성하고 초기 위치를 총의 위치로 설정합니다.
        // GameObject bullet = Instantiate(bulletPrefab, WeaponTransform.position, WeaponTransform.rotation);
        //
        // // 총알에 속도를 적용하여 발사합니다.
        // Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        // bulletRb.velocity = hand.up * bulletSpeed;
    }
}
