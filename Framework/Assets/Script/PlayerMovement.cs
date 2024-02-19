using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Down,
    Up,
    Side,
    Angle,
    Roll,
    None
}
public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed;
    Animator animator;
    Rigidbody2D rigid;
    public float h;
    public float v;
    Vector3 dirVec;

    GameObject scanObject;
    public PlayerState playerState;
    GameManager gameManager;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        gameManager = GameManager.Instance;
    }

    
    void Update()
    {
        float angle = CalculateMouseAngle();


        Debug.Log("현재 각도: " + angle);


        animator.SetBool("IsUp", false);
        animator.SetBool("IsSide", false);
        animator.SetBool("IsDown", false);
        animator.SetBool("IsAngle", false);

        if (angle > -45f && angle <= 15f)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * 1;
            transform.localScale = currentScale;

            SetPlayerState(PlayerState.Side);
        }
        else if (angle > 45f && angle <= 135f)
        {
            SetPlayerState(PlayerState.Up);
        }
        else if ((angle > 165f && angle <= 180f) || (angle >= -180f && angle < -135f))
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * -1;
            transform.localScale = currentScale;

            SetPlayerState(PlayerState.Side);
        }
        else if (angle >= -135f && angle < -45f)
        {
            SetPlayerState(PlayerState.Down);
        }
        else if(angle < 45f && angle >= 15f)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * 1;
            transform.localScale = currentScale;

            SetPlayerState(PlayerState.Angle);
        }
        else
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * -1;
            transform.localScale = currentScale;

            SetPlayerState(PlayerState.Angle);
        }

    }
    void SetPlayerState(PlayerState newState)
    {

        // 각 상태에 따른 초기화 등의 로직을 추가할 수 있습니다.
        switch (newState)
        {
            case PlayerState.Angle:
                // Walk 상태에 대한 초기화 로직
                animator.SetBool("IsAngle", true);
                break;
            case PlayerState.Down:
                // Idle 상태에 대한 초기화 로직
                animator.SetBool("IsDown", true);
                break;
            case PlayerState.Side:
                // Roll 상태에 대한 초기화 로직
                animator.SetBool("IsSide", true);
                break;
            case PlayerState.Up:
                // None 상태에 대한 초기화 로직
                animator.SetBool("IsUp", true);
                break;
            default:
                break;
        }
    }

    float CalculateMouseAngle()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.z;
        Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3 direction = targetPosition - transform.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    void FixedUpdate()
    {
        Move();
    }
    void Move()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        if (h != animator.GetInteger("h"))
        {
            animator.SetInteger("h", (int)h);

            animator.SetBool("isMove", true);
        }
        else if (v != animator.GetInteger("v"))
        {
            animator.SetInteger("v", (int)v);

            animator.SetBool("isMove", true);
        }

        if (v == 0 && h == 0)
        {
            animator.SetBool("isMove", false);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            MoveSpeed = 6f;
        }
        else
        {
            MoveSpeed = 4f;
        }
        if (gameManager.GetPlayerData().currentState == GameState.Event)
        {
            animator.SetInteger("v", 0);
            animator.SetInteger("h", 0);
            MoveSpeed = 0f;
        }
        rigid.velocity = new Vector2(h, v) * MoveSpeed;
    }
    void UpdateAnimation(Vector2 moveInput)
    {
        // moveInput에 따라 애니메이션을 변경합니다.
        if (moveInput.magnitude > 0)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
        }
        else
        {
            animator.SetFloat("Horizontal", 0);
            animator.SetFloat("Vertical", 0);
        }
    }
}
