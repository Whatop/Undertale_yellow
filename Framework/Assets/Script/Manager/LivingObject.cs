using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public enum ObjectState
{
    Down,
    Up,
    Side,
    Angle,
    Roll,
    None
}

public class LivingObject : MonoBehaviour
{
    protected float health;
    protected float maxHealth;
    protected float speed;
    protected bool isnpc; // 비전투!
    protected bool isDie = false;

    protected bool isInvincible = false; // 무적 상태 여부
    protected float invincibleDuration = 0.6f; // 무적 지속 시간 (0.6초)
    protected float invincibleTimer = 0f; // 무적 상태의 타이머

    protected Transform healthBarTransform; // 체력바의 Transform
    public GameObject healthBarPrefab; // 체력바 프리팹
    protected GameObject healthBar; // 인스턴스화된 체력바
    protected Slider healthSlider; // 체력바 슬라이더
    protected TextMeshProUGUI healthText; // 체력바 텍스트

    protected GameManager gameManager;
    protected PlayerData playerData;
    protected Rigidbody2D rigid;
    protected Animator animator;

    public Canvas worldCanvas; // 월드 캔버스
    private Camera mainCamera;
    public GameObject hpBarPoint;
    protected SpriteRenderer spriteRenderer; // SpriteRenderer 참조
    private Color originalColor; // 원래 색상 저장

    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = GameManager.Instance;
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        mainCamera = cameraObject.GetComponent<Camera>(); 
        // 자식 중에 "Soul" 이름을 가진 오브젝트에서 SpriteRenderer를 가져옴
        Transform soulTf = transform.Find("Soul");
        if (soulTf != null)
            spriteRenderer = soulTf.GetComponent<SpriteRenderer>();
        else
            Debug.LogWarning("자식에 'Soul' 오브젝트가 없습니다!");
        // 체력바 초기화
        InitializeHealthBar();
    }

    protected void InitializeHealthBar()
    {
        if (healthBarPrefab != null && worldCanvas != null)
        {
            healthBar = Instantiate(healthBarPrefab, worldCanvas.transform);
            healthBarTransform = healthBar.transform;
            healthSlider = healthBar.GetComponentInChildren<Slider>();
            healthText = healthBar.GetComponentInChildren<TextMeshProUGUI>();

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = health;
            }
            if (healthText != null)
            {
                healthText.text = maxHealth + "/" + health;
            }

            healthBarTransform.position = hpBarPoint.transform.position;
        }
    }

    protected virtual void Update()
    {
        // 무적 상태 타이머 처리
        if (isInvincible)
        {
            invincibleTimer += Time.deltaTime;
            if (invincibleTimer >= invincibleDuration)
            {
                isInvincible = false; // 무적 상태 해제
                invincibleTimer = 0f;
            }
        }

        // 체력바 위치 업데이트
        if (healthBar != null)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = health;
            }
            if (healthText != null)
            {
                healthText.text = maxHealth + "/" + health;
            }
            healthBarTransform.position = hpBarPoint.transform.position;
        }

        if (transform.tag == "Player")
        {
            gameManager.SavePlayerData(playerData);
        }
    }

    public void IncreaseHealth(int v)
    {
        if (health + v < maxHealth)
            health += v;
        else
            health = maxHealth;

    }
    public virtual void Die()
    {
        isDie = true;
        Debug.Log("Object died!");
        //Destroy(healthBar); // 체력바 제거

        if (transform.tag == "Player")
        {
            gameManager.Die(); 
        }
        else
        {
            animator.SetBool("isDie",true);
        }
    }

    public virtual void OffHpbar()
    {
        healthBar.SetActive(false);
    }

    public virtual void TakeDamage(float damageAmount)
    {
        if (!isInvincible) // 무적 상태가 아닐 때만 데미지를 받음
        {
            UIManager.Instance.ShowDamageText(transform.position, damageAmount);
            health -= damageAmount;
            //player라면 체력확인하기아..바본가?
            // 체력바 업데이트
            if (healthSlider != null)
            {
                healthSlider.value = health;
            }

            if (health <= 0 && !isDie )
            {
                Die();
            }
            else
            {
                StartCoroutine(StartInvincibility()); // 무적 상태로 전환
            }
            
        }
    }

    public virtual void Heal(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
        // 필요하면 힐 이펙트, 사운드 등 추가
    }
    private IEnumerator StartInvincibility()
    {
        if (transform.tag == "Player")
        {
            if (spriteRenderer == null)
                yield break;

            isInvincible = true; // 무적 상태 활성화
            float elapsed = 0f;

            while (elapsed < invincibleDuration)
            {
                elapsed += Time.deltaTime;

                // 깜빡임 효과
                float alpha = Mathf.Abs(Mathf.Sin(elapsed * 10)); // 10은 깜빡이는 속도
                SetSpriteAlpha(alpha);

                yield return null;
            }

            SetSpriteAlpha(1f); // 알파 값을 원래대로 복원
            isInvincible = false; // 무적 상태 해제
        }
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    public void Move(Vector3 direction)
    {
        if (!isnpc)
        {
            // 적대 관계 움직임 AI 작성
        }
    }

    public void Init()
    {
    }
    public IEnumerator FadeOutSprite(GameObject targetObject, float duration)
    {
        // targetObject에 SpriteRenderer가 붙어있는지 확인
        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            yield break; // 스프라이트가 없으면 종료
        }

        // 현재 알파값(시작값)
        float startAlpha = spriteRenderer.color.a;
        // 목표 알파값
        float endAlpha = 0f;
        // 경과 시간
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            // 0 ~ 1 사이의 보간값 t
            float t = currentTime / duration;
            // 선형 보간을 통해 알파값 계산
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);

            // 새로 계산된 알파값 적용
            Color color = spriteRenderer.color;
            color.a = newAlpha;
            spriteRenderer.color = color;

            yield return null;  // 다음 프레임까지 대기
        }

        // 마지막으로 알파값을 정확히 0으로 보장
        {
            Color finalColor = spriteRenderer.color;
            finalColor.a = 0f;
            spriteRenderer.color = finalColor;
        }
    }

}
