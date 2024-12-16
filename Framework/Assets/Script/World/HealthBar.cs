using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Text poisonText; // 독 상태 표시 텍스트
    private float currentHealth;
    private float maxHealth;

    // 독 상태 변수
    private bool isPoisoned = false;
    private float poisonDamage = 5f;
    private float poisonInterval = 1f;
    private float poisonDuration = 5f;

    private float poisonTimer;

    void Start()
    {
        maxHealth = GameManager.Instance.GetPlayerData().Maxhealth;
        currentHealth = GameManager.Instance.GetPlayerData().health;
        UpdateHealthBar();
    }

    void Update()
    {
        
        if (isPoisoned)
        {
            poisonTimer -= Time.deltaTime;
            if (poisonTimer <= 0)
            {
                ApplyPoisonDamage();
                poisonTimer = poisonInterval;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBar();
    }

    public void ApplyPoison(float duration, float damage, float interval)
    {
        poisonDuration = duration;
        poisonDamage = damage;
        poisonInterval = interval;
        isPoisoned = true;
        poisonTimer = poisonInterval;

        // 독 상태 텍스트 업데이트
        if (poisonText != null)
        {
            poisonText.gameObject.SetActive(true);
            poisonText.text = "Poisoned!";
        }

        Invoke(nameof(RemovePoison), poisonDuration); // 독 상태 종료 타이머
    }

    private void ApplyPoisonDamage()
    {
        TakeDamage(poisonDamage);
        Debug.Log($"Poison damage applied: {poisonDamage}");
    }

    private void RemovePoison()
    {
        isPoisoned = false;

        // 독 상태 텍스트 제거
        if (poisonText != null)
        {
            poisonText.gameObject.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        fillImage.fillAmount = currentHealth / maxHealth;
        fillImage.color = Color.Lerp(Color.red, Color.green, currentHealth / maxHealth);
    }
}
