using UnityEngine;
using System.Collections;

public class ShellBehavior : MonoBehaviour
{
    // 탄피 이동 관련
    public Vector2 velocity;       // 초기 속도 (Eject 시 세팅)
    public float drag = 3f;       // 마찰(감속) 계수
    public float rotationSpeed = 360f; // 탄피 회전 속도 (도/초)

    // 착지 판정
    public float minVelocity = 0.1f;  // 이 이하 속도면 착지로 간주
    public float maxLifetime = 2f;    // 최대 이동 시간
    private float elapsedTime = 0f;
    private bool hasLanded = false;   // 착지 여부

    // 착지 후 페이드 아웃
    public float fadeDuration = 1f;
    private bool isFading = false;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    // 사운드
    public string landSoundName = "shell_land_01";
    public int landSoundIndex = 300;
    public float landVolume = 0.7f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (!hasLanded)
        {
            // 1) 탄피 이동 (단순 x,y 좌표 이동)
            transform.Translate(velocity * Time.deltaTime, Space.World);

            // 2) 탄피 회전 (원한다면)
            float rotateAngle = rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotateAngle);

            // 3) 마찰로 속도 서서히 감소
            //    속도 크기가 drag*dt 만큼 줄어듦 (단순 선형감속 예시)
            float speed = velocity.magnitude;
            speed -= drag * Time.deltaTime;
            if (speed < 0f) speed = 0f;
            velocity = velocity.normalized * speed;

            // 4) 착지 판정 : 일정 시간 경과 or 속도 너무 작아짐
            if (elapsedTime >= maxLifetime || velocity.magnitude < minVelocity)
            {
                Land();
            }
        }
        else
        {
            // 이미 착지했으면, 페이드 중인지 확인만
            // 착지한 뒤에는 위치 이동 등 안 함
            rb.gravityScale = 0;
        }
    }

    void Land()
    {
        hasLanded = true;
        // 움직임 정지
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;  // 회전도 멈춤
        SoundManager.Instance.SFXPlay("shotgun_reload_01", 224); // 재장전 사운드

        // 착지 후 페이드 아웃 시작
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        if (isFading) yield break;
        isFading = true;

        Color originColor = sr.color;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = new Color(originColor.r, originColor.g, originColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
}
