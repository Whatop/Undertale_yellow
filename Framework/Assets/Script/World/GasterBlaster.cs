using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasterBlaster : MonoBehaviour
{
    private Animator animator;
    public GameObject laserPrefab;               // 레이저 프리팹
    public Transform laserSpawnPoint;            // 레이저 시작 위치
    private bool laserFired = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        laserFired = false;
        animator.Play("Idle");       // 초기 상태
        Invoke(nameof(Shot), 0.3f);  // 등장 후 약간의 텀을 두고 공격 시작
        Invoke(nameof(DeactiveDelay), 10f); // 혹시 못 사라질 경우 대비
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    void Shot()
    {
        animator.SetTrigger("OpenMouth");
        SoundManager.Instance.SFXPlay("gasterblaster", 225); // 충전/발사 사운드
        CameraController.Instance.ShakeCamera();
    }

    // 애니메이션에서 호출
    public void FireLaser()
    {
        if (laserFired) return;
        laserFired = true;

        GameObject laser = Instantiate(laserPrefab, laserSpawnPoint.position, laserSpawnPoint.rotation);
        laser.transform.SetParent(transform); // 부모로 붙여 연동
    }

    // 애니메이션에서 호출
    public void EndAttack()
    {
        StartCoroutine(MoveBackAndDisable());
    }

    IEnumerator MoveBackAndDisable()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.down * 3f;
        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
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

            player.GetComponent<EnemyController>().TakeDamage(1);
          
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Soul"))
        {
            GameObject player = other.gameObject;

            player.GetComponent<EnemyController>().TakeDamage(1);
             
        }
    }
}
