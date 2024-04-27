using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int npcID; // NPC의 ID
    public DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager.StartDialogue(npcID); // NPC의 대화 시작
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogueManager.StartDialogue(npcID); // 스페이스 바를 누르면 대화 진행
        }
    }
}
