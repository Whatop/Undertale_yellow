using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences; // 대사 큐
    private NPC currentNPC; // 현재 대화 중인 NPC

    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(int npcID)
    {
        // 대사를 초기화하고, 대사를 큐에 추가하는 코드
        // 예시:
        sentences.Clear();
        sentences.Enqueue("Hello, I am NPC " + npcID);
        sentences.Enqueue("How are you today?");
        sentences.Enqueue("Goodbye!");

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        Debug.Log(sentence); // 실제 게임에서는 대사를 UI에 표시하는 코드가 필요합니다.
    }

    void EndDialogue()
    {
        if (currentNPC != null)
        {
            currentNPC.EndDialogue(); // NPC에게 대화 종료를 알림
        }
    }

    public void SetCurrentNPC(NPC npc)
    {
        currentNPC = npc;
    }
}
