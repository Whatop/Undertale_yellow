using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int npcID; // NPC의 ID
    public DialogueManager dialogueManager;
    private bool isTalking = false; // 대화가 진행 중인지 여부
    private GameObject outlineObject; // 외곽선 효과를 위한 오브젝트
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer outlineSpriteRenderer;
    public Material outlineMaterial; // 외곽선 Material

    bool isFirst = true;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateOutline();
    }

    void Update()
    {
        if (IsPlayerNearby() && !EventManager.Instance.isEvent) // 이벤트 중이 아닐 때만 상호작용 가능
        {
            Highlight(true);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isTalking)
                {
                    dialogueManager.DisplayNextSentence(); // 대화 진행
                }
                else
                {
                    StartDialogue(); // 대화 시작
                }
            }
        }
        else
        {
            Highlight(false);
        }
        if (EventManager.Instance.isEvent) // 강제 이벤트
        {
            if (isFirst)
            {
                StartDialogue(); // 대화 시작
                isFirst = false;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isTalking)
                {
                    dialogueManager.DisplayNextSentence(); // 대화 진행
                }
                
            }
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        UIManager.Instance.TextUpdate();
        dialogueManager.SetCurrentNPC(this); // NPC 자신을 DialogueManager에 알림
        dialogueManager.StartDialogue(npcID); // NPC의 대화 시작
    }

    public void EndDialogue()
    {
        isTalking = false; // 대화 종료
        isFirst = false; // 처음
    }

    // 플레이어가 가까이 있는지 확인
    bool IsPlayerNearby()
    {
        // 플레이어와의 거리 계산 (예시: 3유닛 이내)
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
        outlineSpriteRenderer.sortingOrder = 2; // 정렬 순서 2로 설정

        outlineObject.SetActive(false);
    }

    // 하이라이트 효과 관리
    void Highlight(bool isHighlighted)
    {
        outlineObject.SetActive(isHighlighted);
    }
}
