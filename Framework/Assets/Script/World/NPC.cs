using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int npcID; // NPC의 ID
    public DialogueManager dialogueManager;
    private bool isTalking = false; // 대화가 진행 중인지 여부

    void Start()
    {
        StartDialogue(); // NPC의 대화 시작
    }

    void Update()
    {
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

    void StartDialogue()
    {
        isTalking = true;
        dialogueManager.StartDialogue(npcID); // NPC의 대화 시작
    }

    public void EndDialogue()
    {
        isTalking = false; // 대화 종료
    }
}
