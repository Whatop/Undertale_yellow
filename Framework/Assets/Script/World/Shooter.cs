using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform WeaponTransform; // 총의 Transform 컴포넌트
    public GameObject bulletPrefab; // 총알 프리팹
    public float bulletSpeed = 10f; // 총알 발사 속도
    Weapon weaponData;
    GameManager gameManager;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        weaponData = new Weapon();
    }

    void Update()
    {
        // 마우스 위치를 기준으로 총의 방향을 설정합니다.

        Weapon weapon = gameManager.GetWeaponData();
        int currentAmmo= weapon.currentAmmo;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - WeaponTransform.position).normalized;
        WeaponTransform.up = direction;

        // 마우스 왼쪽 버튼을 클릭하면 총알을 발사합니다.
        if (Input.GetMouseButtonDown(0) && currentAmmo > 0)
        {
            Shoot();
            weapon.currentAmmo -= 1;
            gameManager.SaveWeaponData(weapon);
        }

        if(weapon.currentAmmo < weapon.maxAmmo &&Input.GetKeyDown(KeyCode.R))
        {

            weapon.currentAmmo = weapon.magazine;
            gameManager.SaveWeaponData(weapon);
        }
    }

    void Shoot()
    {
        // 총알을 생성하고 초기 위치를 총의 위치로 설정합니다.
        GameObject bullet = Instantiate(bulletPrefab, WeaponTransform.position, WeaponTransform.rotation);
        
        
        // 총알에 속도를 적용하여 발사합니다.
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = WeaponTransform.up * bulletSpeed;
    }
    void Reload()
    {
    }
}

