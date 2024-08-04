using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int npcID; // NPC의 ID
    public DialogueManager dialogueManager;
    private bool isTalking = false; // 대화가 진행 중인지 여부
    private Renderer npcRenderer; // NPC의 Renderer
    private Color originalColor; // NPC의 원래 색상

    void Start()
    {
        npcRenderer = GetComponent<Renderer>();
        originalColor = npcRenderer.material.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsPlayerNearby())
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

    void StartDialogue()
    {
        isTalking = true;
        dialogueManager.SetCurrentNPC(this); // NPC 자신을 DialogueManager에 알림
        dialogueManager.StartDialogue(npcID); // NPC의 대화 시작
    }

    public void EndDialogue()
    {
        isTalking = false; // 대화 종료
    }

    // 플레이어가 가까이 있는지 확인
    bool IsPlayerNearby()
    {
        // 플레이어와의 거리 계산 (예시: 2유닛 이내)
        float distance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerData().position);
        return distance <= 2.0f;
    }

    // 트리거 엔터 이벤트
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Highlight(true);
        }
    }

    // 트리거 엑시트 이벤트
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Highlight(false);
        }
    }

    // 하이라이트 효과 관리
    void Highlight(bool isHighlighted)
    {
        if (isHighlighted)
        {
            npcRenderer.material.color = Color.white; // 하이라이트 색상으로 변경
            npcRenderer.material.SetColor("_OutlineColor", Color.white); // 흰색 테두리 추가
        }
        else
        {
            npcRenderer.material.color = originalColor; // 원래 색상으로 복원
            npcRenderer.material.SetColor("_OutlineColor", Color.clear); // 테두리 제거
        }
    }
}
