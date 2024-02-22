using System;
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

    bool isMove = false;

    public float cooldownTime = 0.5f;
    private bool isCooldown = false;

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
        if (Input.GetMouseButtonDown(1) && !isCooldown && isMove)
        {
            // 우클릭 입력이 감지되면 처리하고 쿨타임 시작
            playerState = PlayerState.Roll;
            StartRoll(v,h);
            StartCooldown();
        }

        // 쿨타임 감소
        if (isCooldown)
        {
            cooldownTime -= Time.deltaTime;

            // 쿨타임이 끝나면 상태 초기화
            if (cooldownTime <= 0)
            {
                isCooldown = false;
                cooldownTime = 0.5f; // 원하는 쿨타임 값으로 설정
                playerState = PlayerState.None;
            }
        }

        if (playerState != PlayerState.Roll)//구르기는 각도처리를 따로해서
        {
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
            else if (angle < 45f && angle >= 15f)
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
    }

    private void StartRoll(int vir, int hir) //Roll은 각도처리 따로 함 이동값으로 판별해서 
    {
        if (vir != 0 && hir == 0) // 왼쪽 또는 오른쪽으로 이동할 때
        {
            Vector3 currentScale = transform.localScale;
            if (hir < 0) // 왼쪽 이동
            {
                currentScale.x = Mathf.Abs(currentScale.x) * -1;// 스케일 반대로 설정
            }
            else // 오른쪽 이동
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1;//  스케일 그대로 설정
            }
            transform.localScale = currentScale;
            animator.SetBool("IsSide", true);
        }
        else if (vir == 0 && hir != 0) // 위 또는 아래로 이동할 때
        {
            animator.SetBool("IsUp", v > 0);
            animator.SetBool("IsDown", v < 0);
        }
        else // 대각선 이동일 때
        {
            Vector3 currentScale = transform.localScale;
            if (hir < 0 && vir > 0) // 왼쪽 위 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * -1;// 스케일 반대로 설정
                animator.SetBool("IsAngle", true);
            }
            else if (hir > 0 && vir > 0) // 오른쪽 위 대각선
            {
                currentScale.x = Mathf.Abs(currentScale.x) * 1;//  스케일 그대로 설정
                animator.SetBool("IsAngle", true);
            }
            else if (hir < 0 && vir < 0) // 왼쪽 아래 대각선
            {
                animator.SetBool("IsSide", true);
                currentScale.x = Mathf.Abs(currentScale.x) * -1;// 스케일 반대로 설정
            }
            else if (hir > 0 && vir < 0) // 오른쪽 아래 대각선
            {
                animator.SetBool("IsSide", true);
                currentScale.x = Mathf.Abs(currentScale.x) * 1;//  스케일 그대로 설정
            }
            transform.localScale = currentScale;

            //왼쪽 대각선 아래면 스케일반대로, 오른쪽 아래 대각선이면 그대로 
        }
            animator.SetTrigger("IsRoll");
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
            isMove = true;
            animator.SetInteger("h", (int)h);

        }
        else if (v != animator.GetInteger("v"))
        {
            isMove = true;
            animator.SetInteger("v", (int)v);
        }

        if (v == 0 && h == 0)
        {
            isMove = false;
        }
        animator.SetBool("isMove", isMove);

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
    void StartCooldown()
    {
        // 쿨타임 시작
        isCooldown = true;
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
