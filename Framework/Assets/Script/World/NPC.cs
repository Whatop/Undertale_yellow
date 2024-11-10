using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int npcID; // NPC의 ID
    public DialogueManager dialogueManager;
    
    public bool isTalking = false;
    [SerializeField]
    private bool isFirstInteraction = true; // 처음 대화인지 확인
    private GameObject outlineObject; // 외곽선 효과를 위한 오브젝트
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer outlineSpriteRenderer;
    public Material outlineMaterial; // 외곽선 Material
    private Animator animator;
    private Animator TextBar_animator;

    private bool canAdvanceDialogue = false; // 대화 진행 가능 여부를 추적하는 플래그

    public bool isEvent = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        TextBar_animator = UIManager.Instance.npcFaceImage.GetComponent<Animator>();
        CreateOutline(); // 하이라이트용 외곽선 오브젝트 생성
    }

    void Update()
    {
        HandleInteraction();
    }
    private void HandleInteraction()
    {
        bool playerNearby = IsPlayerNearby();

        // 대화 중에는 인벤토리 상호작용 차단
        if (isTalking)
        {
            UIManager.Instance.isInventroy = false;
        }

        if (playerNearby && !isEvent && isFirstInteraction && !UIManager.Instance.isSaveDelay && !UIManager.Instance.isInventroy && 
            !GameManager.Instance.GetPlayerData().isDie && !UIManager.Instance.isUserInterface)
        {
            Highlight(true); // 플레이어가 가까이 있으면 하이라이트 표시

            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTalking)
                {
                    if (dialogueManager.IsEffecting())
                    {
                        UIManager.Instance.isSaveDelay = true; // 상호작용 가능하도록 설정
                        dialogueManager.SkipTypeEffect();
                    }
                    else
                    {
                        dialogueManager.DisplayNextSentence(npcID);
                    }
                }
                else
                {
                    StartDialogue();
                }
            }
        }
        else
        {
            Highlight(false); // 플레이어가 멀어지면 하이라이트 해제
        }

        if(GameManager.Instance.GetPlayerData().isDie)
            HandleOverDialogue();
        else
            HandleEventDialogue();

    }


    public void EndDialogue()
    {
        isTalking = false;
        isEvent = false;
        isFirstInteraction = true;
        // 대화가 끝나면 잠시 상호작용 차단
        UIManager.Instance.isSaveDelay = true; // 상호작용 가능하도록 설정
        StartCoroutine(InteractionDelay());

        // 대화가 끝나면 인벤토리 상호작용 허용
        UIManager.Instance.isInventroy = true;
        UIManager.Instance.ChangeInventroy();
    }

    public IEnumerator InteractionDelay()
    {
        yield return new WaitForSeconds(0.2f); // 0.2초 동안 상호작용 차단
        UIManager.Instance.isSaveDelay = false; // 상호작용 가능하도록 설정
    }
    private void HandleEventDialogue()
    {
        if (isEvent && isFirstInteraction)
        {
            isTalking = true;
            dialogueManager.SetCurrentNPC(this);
            isFirstInteraction = false;
        }

        if (isEvent && (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) && isTalking && !UIManager.Instance.isSaveDelay && !UIManager.Instance.isInventroy)
        {
            // 현재 대사 출력 중인 경우와 완료된 상태를 구분
            if (dialogueManager.IsEffecting())
            {
                UIManager.Instance.isSaveDelay = true; // 상호작용 가능하도록 설정
                dialogueManager.SkipTypeEffect(); // 타이핑 효과를 즉시 완료
            }
            else
            {
                dialogueManager.DisplayNextSentence(npcID); // 다음 문장 표시
            }
        }
    }

    private void HandleOverDialogue()
    {
        // Z 또는 Space 키가 눌렸고, 대화 진행이 가능한 상태에서만 실행
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space)) && canAdvanceDialogue)
        {
            // 현재 대사가 끝난 경우에만 다음 문장으로 넘어감
            dialogueManager.DisplayNextGameOver();
            canAdvanceDialogue = false; // 다음 대사로 넘어간 후에는 다시 false로 설정
        }
    }
    public void OnDialogueEffectComplete()
    {
        canAdvanceDialogue = true; // 대사 타이핑 완료 시 true로 설정하여 키 입력 가능하게 함
    }

    // 표정을 설정하는 메소드
    /// <summary>
    /// Default, Smile,Smiling, Pain, Angry, Sneer, Talking, Surprise, Sad
    /// </summary>
    /// <param name="expression"></param>
    public void SetExpression(string expression)
    {
        if (animator != null)
        {
            animator.ResetTrigger("Talking");
            animator.ResetTrigger("Default");

            animator.SetTrigger(expression);
        }
        if (TextBar_animator != null)
        {
            TextBar_animator.ResetTrigger("Talking");
            TextBar_animator.ResetTrigger("Default");

            TextBar_animator.SetTrigger(expression);
        }
    }

    // 기본 표정으로 돌아가는 메소드
    public void ResetToDefaultExpression()
    {
        if (animator != null)
        {
            animator.SetTrigger("Default");
        }
        if (TextBar_animator != null)
        {
            TextBar_animator.SetTrigger("Default");
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        dialogueManager.SetCurrentNPC(this);
        dialogueManager.StartDialogue(npcID);
    }

    bool IsPlayerNearby()
    {
        float distance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerData().position);
        return distance <= 3f;
    }

    // 외곽선 오브젝트 생성
    void CreateOutline()
    {
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localScale = Vector3.one * 1.1f; // 원래 크기보다 약간 크게 설정

        outlineSpriteRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineSpriteRenderer.sprite = spriteRenderer.sprite;
        outlineSpriteRenderer.material = outlineMaterial; // Material을 외곽선 Material로 설정
        outlineSpriteRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        outlineSpriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // NPC보다 뒤에 표시되도록 정렬 순서 조정

        outlineObject.SetActive(false); // 처음에는 비활성화
    }

    // 하이라이트 효과 관리
    void Highlight(bool isHighlighted)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(isHighlighted);
        }
    }
}
