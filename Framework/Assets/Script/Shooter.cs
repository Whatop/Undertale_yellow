using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Transform gunTransform; // 총의 Transform 컴포넌트
    public GameObject bulletPrefab; // 총알 프리팹
    public float bulletSpeed = 10f; // 총알 발사 속도

    void Update()
    {
        // 마우스 위치를 기준으로 총의 방향을 설정합니다.


        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - gunTransform.position).normalized;
        gunTransform.up = direction;

        // 마우스 왼쪽 버튼을 클릭하면 총알을 발사합니다.
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // 총알을 생성하고 초기 위치를 총의 위치로 설정합니다.
        GameObject bullet = Instantiate(bulletPrefab, gunTransform.position, gunTransform.rotation);

        // 총알에 속도를 적용하여 발사합니다.
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        bulletRb.velocity = gunTransform.up * bulletSpeed;
    }
    void Reload()
    {
    }
}

