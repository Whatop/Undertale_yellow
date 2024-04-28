using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingObject : MonoBehaviour
{
    protected int health;
    protected float speed;
    protected bool isnpc; // 비전투!

    protected GameManager gameManager;

    protected Rigidbody2D rigid;
    protected Animator animator;
    protected virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = GameManager.Instance;
        // 게임 매니저에서 플레이어 데이터를 가져와서 적용
        PlayerData playerData = gameManager.GetPlayerData();
        health = playerData.health;
        // 나머지 필요한 데이터도 가져와서 적용
    }

    public virtual void Die()
    {
        Debug.Log("Object died!");
        // 여기에 죽을 때의 동작을 구현합니다.
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
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
