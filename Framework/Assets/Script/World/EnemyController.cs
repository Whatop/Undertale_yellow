using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        // 적이 데미지를 받았을 때 호출되는 함수
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 적이 죽었을 때 호출되는 함수
        // 적 캐릭터의 사망 효과, 드롭 아이템 등을 처리합니다.
        Destroy(gameObject);
    }
}
