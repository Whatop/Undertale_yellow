using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed;
    Animator animator;
    Rigidbody2D rigid;
    public float h;
    public float v;
    Vector3 dirVec;

    GameObject scanObject;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            MoveSpeed = 6f;
        }
        else
        {
            MoveSpeed = 4f;
        }
        if (GameManager.Instance.GetPlayerData().currentState == GameState.Event)
        {
            animator.SetBool("isChange", false);
            MoveSpeed = 0f;
        }


    }
    void Move()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if (h != animator.GetInteger("h"))
        {
            animator.SetInteger("h", (int)h);

            animator.SetBool("isChange", true);
        }
        else if (v != animator.GetInteger("v"))
        {
            animator.SetInteger("v", (int)v);

            animator.SetBool("isChange", true);
        }
        else
            animator.SetBool("isChange", false);
        rigid.velocity = new Vector2(h, v) * MoveSpeed;
    }
}
