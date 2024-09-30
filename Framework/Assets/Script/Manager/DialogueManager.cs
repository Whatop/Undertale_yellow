using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences;
    private Queue<string> gameover_sentences;
    private NPC currentNPC;
    public TypeEffect typeEffect;

    public static DialogueManager instance;
    public Sprite[] npcFaces;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static DialogueManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DialogueManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("DialogueManager");
                    instance = obj.AddComponent<DialogueManager>();
                }
            }
            return instance;
        }
    }

    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(int npcID)
    {
        sentences.Clear();
        GameManager.Instance.GetPlayerData().isStop = true;
        switch (npcID)
        {
            case 0:
                sentences.Enqueue("안녕, 나는 테스트 NPC " + npcID);
                sentences.Enqueue("오늘 하루는 어때?");
                sentences.Enqueue("잘가~");
                break;

            case 1001:
                sentences.Enqueue("어라?");
                sentences.Enqueue("Don't say that.");
                break;
        }

        UIManager.Instance.TextBarOpen();
        DisplayNextSentence();
    }
    public void StartEventDialogue(int npcID)
    {
        sentences.Clear();
        GameManager.Instance.GetPlayerData().isStop = true;
        switch (npcID)
        {
            case 0:
                sentences.Enqueue("안녕, 나는 테스트 NPC " + npcID);
                sentences.Enqueue("오늘 하루는 어때?");
                sentences.Enqueue("잘가~");
                break;

            case 1001:
                UIManager.Instance.npcFaceImage.sprite = npcFaces[0];
                sentences.Enqueue(".  .  .");
                sentences.Enqueue("안녕, 내 이름은 플라위");
                sentences.Enqueue("이런, 길을 잃은 것 같네");
                sentences.Enqueue("내가 조금 도와줄게.");
                SoundManager.Instance.StopBGSound();
                SoundManager.Instance.BGSoundPlay(3);
                break;
        }
        currentNPC.isEvent = true;
        UIManager.Instance.TextBarOpen();
        UIManager.Instance.OffPlayerUI();
        DisplayNextSentence(npcID);
    }

    public void StartGameOverDialogue(int npcID)
    {
        sentences.Clear();
        GameManager.Instance.GetPlayerData().isStop = true;
        switch (npcID)
        {
            case 0:
                sentences.Enqueue("벌써부터 포기해선 안된다 . . .");
                sentences.Enqueue(GameManager.Instance.GetPlayerData().player_Name + "!" + "의지를 가지거라 . . .");
                break;

        }
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
    public void DisplayNextSentence(int eventNumber)
    {
        if (sentences.Count == 0)
        {
            EndDialogue(eventNumber);
            return;
        }

        string sentence = sentences.Dequeue();
        typeEffect.SetMsg(sentence, OnSentenceComplete, eventNumber);
    }
    private void OnSentenceComplete()
    {
        Debug.Log("문장이 완료되었습니다.");
    }

    void EndDialogue()
    {
        if (currentNPC != null)
        {
            currentNPC.EndDialogue();
            UIManager.Instance.CloseTextbar();
            GameManager.Instance.GetPlayerData().isStop = false;
            UIManager.Instance.OnPlayerUI();
        }
    }
    void EndDialogue(int eventNumber)
    {
        if (currentNPC != null)
        {
            currentNPC.EndDialogue();
            UIManager.Instance.CloseTextbar();
            GameManager.Instance.GetPlayerData().isStop = false;
            UIManager.Instance.OnPlayerUI();
            switch (eventNumber)
            {
                case 1001:
                    BattleManager.Instance.BattleStart(eventNumber);
                    break;
            }
        }
    }
    public void SetCurrentNPC(NPC npc)
    {
        currentNPC = npc;
    }
}
