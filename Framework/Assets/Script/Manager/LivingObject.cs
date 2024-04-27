using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingObject : MonoBehaviour
{
    protected int health;
    protected float speed;
    protected bool isnpc; // 비전투!
    public virtual void Die()
    {
        Debug.Log("Object died!");
        // 여기에 죽을 때의 동작을 구현합니다.
    }

    public virtual void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Move(Vector3 direction)
    {
        if (!isnpc)
        {
            //적대관게 움직임 AI 작성
        }
        

    }

    public virtual void Init(int startingHealth, float startingSpeed)
    {
        health = startingHealth;
        speed = startingSpeed;
    }
}
