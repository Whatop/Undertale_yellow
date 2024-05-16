using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    protected float speed;
    protected bool isnpc; // 비전투!

    protected bool isInvincible = false; // 무적 상태인지 여부
    protected float invincibleDuration = 1.5f; // 무적 지속 시간
    protected float invincibleTimer = 0f; // 무적 타이머


    protected GameManager gameManager;

    protected PlayerData playerData;
    protected Rigidbody2D rigid;
    protected Animator animator;
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = GameManager.Instance;
        // 게임 매니저에서 플레이어 데이터를 가져와서 적용
        playerData = gameManager.GetPlayerData();
        health = playerData.health;
        // 나머지 필요한 데이터도 가져와서 적용
    }

    private void Update()
    {
        gameManager.SavePlayerData(playerData);
    }
    public virtual void Die()
    {
        Debug.Log("Object died!");
        // 여기에 죽을 때의 동작을 구현합니다.
    }

    public void TakeDamage(int damageAmount) //가만히 ?
    {
        if (!isInvincible) // 무적 상태가 아닐 때만 데미지를 받음
        {
            UIManager.Instance.ShowDamageText(transform.position, damageAmount);
            health -= damageAmount;
            if (health <= 0)
            {
                Die();
            }
        }
    }
    public void TakeDamage(int damageAmount, Vector3 position)// 움직이는?
    {
        if (!isInvincible) // 무적 상태가 아닐 때만 데미지를 받음
        {
            UIManager.Instance.ShowDamageText(position, damageAmount);
            health -= damageAmount;
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
            //적대관게 움직임 AI 작성
        }
        

    }

    public void Init()
    {
    }
}
