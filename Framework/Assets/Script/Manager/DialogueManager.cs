using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences;
    private NPC currentNPC;
    public TypeEffect typeEffect;
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(int npcID)
    {
        sentences.Clear();
        switch (npcID)
        {
            case 0:
                sentences.Enqueue("안녕, 나는 테스트 NPC " + npcID);
                sentences.Enqueue("오늘 하루는 어때?");
                sentences.Enqueue("잘가~");
                break;
        }

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
        typeEffect.SetMsg(sentence, OnSentenceComplete);
    }

    private void OnSentenceComplete()
    {
        // 문장이 완전히 표시된 후 추가로 실행할 로직을 여기에 추가할 수 있습니다.
        Debug.Log("문장이 완료되었습니다.");
    }

    void EndDialogue()
    {
        if (currentNPC != null)
        {
            currentNPC.EndDialogue();
            UIManager.Instance.CloseTextbar();
            GameManager.Instance.GetPlayerData().isStop = false;

        }
    }

    public void SetCurrentNPC(NPC npc)
    {
        currentNPC = npc;
    }
}
