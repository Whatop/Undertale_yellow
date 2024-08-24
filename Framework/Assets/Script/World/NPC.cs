using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int npcID; // NPC의 ID
    public DialogueManager dialogueManager;
    [SerializeField]
    private bool isTalking = false;
    [SerializeField]
    private bool isFirstInteraction = true; // 처음 대화인지 확인
    private GameObject outlineObject; // 외곽선 효과를 위한 오브젝트
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer outlineSpriteRenderer;
    public Material outlineMaterial; // 외곽선 Material

    public bool isEvent = false;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateOutline(); // 하이라이트용 외곽선 오브젝트 생성
    }

    void Update()
    {
        if (IsPlayerNearby() && !isEvent) // 이벤트 중이 아닐 때 상호작용 가능
        {
            Highlight(true); // 플레이어가 가까이 있으면 하이라이트 표시

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isTalking)
                {
                    dialogueManager.DisplayNextSentence();
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

        // 강제 이벤트 중일 때 대화 처리
        if (isEvent && isFirstInteraction)
        {
            StartDialogue();
            isFirstInteraction = false;
        }

        if (isEvent && Input.GetKeyDown(KeyCode.Space) && isTalking)
        {
            dialogueManager.DisplayNextSentence();
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        dialogueManager.SetCurrentNPC(this);
        dialogueManager.StartDialogue(npcID);
    }

    public void EndDialogue()
    {
        isTalking = false;
        isEvent = false;
        isFirstInteraction = true;
    }

    bool IsPlayerNearby()
    {
        float distance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerData().position);
        return distance <= 3f; // 원하는 거리로 조정
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
