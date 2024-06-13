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
    protected int health;
    protected int maxHealth;
    protected float speed;
    protected bool isnpc; // 비전투!
    protected bool isDie = false;
 
    protected bool isInvincible = false; // 무적 상태인지 여부
    protected float invincibleDuration = 1.5f; // 무적 지속 시간
    protected float invincibleTimer = 0f; // 무적 타이머

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
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = GameManager.Instance;
        playerData = gameManager.GetPlayerData();
        GameObject cameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        mainCamera = cameraObject.GetComponent<Camera>();

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

            if (transform.tag == "Pleyer")
                gameManager.SavePlayerData(playerData);
    }

    public virtual void Die()
    {
        isDie = true;
        Debug.Log("Object died!");
        Destroy(healthBar); // 체력바 제거

    }

    public void TakeDamage(int damageAmount)
    {
        if (!isInvincible) // 무적 상태가 아닐 때만 데미지를 받음
        {
            UIManager.Instance.ShowDamageText(transform.position, damageAmount);
            health -= damageAmount;

            // 체력바 업데이트
            if (healthSlider != null)
            {
                healthSlider.value = health;
            }

            if (health <= 0)
            {
                Die();
            }
        }
    }

    public void TakeDamage(int damageAmount, Vector3 position)
    {
        if (!isInvincible) // 무적 상태가 아닐 때만 데미지를 받음
        {
            UIManager.Instance.ShowDamageText(position, damageAmount);
            health -= damageAmount;

            // 체력바 업데이트
            if (healthSlider != null)
            {
                healthSlider.value = health;
            }

            if (health <= 0)
            {
                Die();
            }
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
}
