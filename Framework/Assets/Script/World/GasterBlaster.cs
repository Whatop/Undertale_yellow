using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasterBlaster : MonoBehaviour
{
    private Animator animator;
    public GameObject laserPrefab;               // 레이저 프리팹
    public Transform laserSpawnPoint;            // 레이저 시작 위치
    private bool laserFired = false;
    public Vector2 targetDirection = Vector2.down; // BattleManager에서 설정
    public bool trackPlayer = true; // true면 플레이어 방향, false면 targetDirection 사용

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    private void OnEnable()
    {
        laserFired = false;
        animator.Play("Idle");
    }
    private void OnDisable()
    {
        CancelInvoke();
    }

    public void Shot()
    {
        animator.SetTrigger("OpenMouth");
        Vector2 targetPos = GameManager.Instance.GetPlayerData().player.transform.position;
        Vector2 myPos = transform.position;

        Vector2 dir = targetPos - myPos; // 나 → 플레이어 방향
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle+90); // Sprite의 위쪽이 forward면 -90 보정

        SoundManager.Instance.SFXPlay("gasterblaster", 225); // 충전/발사 사운드
    }

    // 애니메이션에서 호출
    public void FireLaser()
    {
        if (laserFired) return;
        laserFired = true;

        GameObject laser = Instantiate(laserPrefab, laserSpawnPoint.position, laserSpawnPoint.rotation);
        laser.transform.SetParent(transform); // 부모로 붙여 연동
        CameraController.Instance.ShakeCamera();
        EndAttack();
    }
    public void EndAttack()
    {
        StartCoroutine(MoveBackAndDisable());
    }

    IEnumerator MoveBackAndDisable()
    {
        Vector3 backDir = transform.up;
        Vector3 start = transform.position;
        Vector3 end = start + backDir * 40f; // 거리 2배로 증가

        float duration = 5f; // 기존 2.5f의 2배로 증가
        float t = 0f;

        while (t < duration)
        {
            transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false); // 필요하다면 종료 처리
    }


    void DeactiveDelay()
    {
        gameObject.SetActive(false);
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Soul"))
        {
            GameObject player = other.gameObject;

            GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().TakeDamage(1, GameManager.Instance.GetPlayerData().player.transform.position);


        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Soul"))
        {
            GameObject player = other.gameObject;

            GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().TakeDamage(1, GameManager.Instance.GetPlayerData().player.transform.position);


        }
    }
}
