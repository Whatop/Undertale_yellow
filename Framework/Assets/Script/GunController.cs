using UnityEngine;

// 각 총의 특성을 나타내는 클래스
[System.Serializable]
public class Gun
{
    public int id;             // 총의 고유한 ID
    public string gunName;     // 총의 이름
    public int damage;         // 총의 공격력
    public int currentAmmo;    // 현재 탄알집에 남아있는 총알 수
    public int maxAmmo;        // 탄알집의 최대 총알 수
    public int maxRange;
    public float bulletSpeed;  // 총알 속도
    public float accuracy;     // 총의 정확도
    public Transform firePoint; // 총알이 발사될 위치
}

public class GunController : MonoBehaviour
{
    public Transform gunTransform;  // 총 모델의 Transform
    public GameObject bulletPrefab; // 총알 프리팹
    public Gun currentGun;          // 현재 사용 중인 총의 정보

    public float rotationSpeed = 10f; // 플레이어 회전 속도

    void Update()
    {
        // 마우스 방향 얻기
        Vector3 direction = GetDirectionToMouse();

        // 플레이어 회전
        RotatePlayer(direction);

        // 마우스 왼쪽 버튼 클릭 시 총알 발사
        if (Input.GetMouseButtonDown(0))
        {
            ShootBullet(direction);
        }
    }

    // 마우스 위치로부터 방향 벡터 계산
    Vector3 GetDirectionToMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return (targetPosition - transform.position).normalized;
    }

    // 플레이어 회전
    void RotatePlayer(Vector3 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        gunTransform.rotation = Quaternion.Slerp(gunTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // 총알 발사
    void ShootBullet(Vector3 direction)
    {
        // 총알이 남아 있을 경우 발사
        if (currentGun.currentAmmo > 0)
        {
            // 총알 생성 및 초기화
            GameObject bullet = Instantiate(bulletPrefab, currentGun.firePoint.position, Quaternion.identity);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.InitializeBullet(direction, currentGun.bulletSpeed, currentGun.accuracy, currentGun.damage, currentGun.maxRange);

            // 현재 탄알집에서 총알 감소
            currentGun.currentAmmo--;

            // 로그 출력
            Debug.Log($"Shot {currentGun.gunName}! Ammo: {currentGun.currentAmmo}/{currentGun.maxAmmo}");
        }
        else
        {
            // 총알이 부족한 경우 로그 출력
            Debug.Log("Out of ammo!");
        }
    }
}
