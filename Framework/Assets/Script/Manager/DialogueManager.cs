using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SentenceData
{
    public string text;
    public string expression;
}

[System.Serializable]
public class DialogueData
{
    public int npcID;
    public bool isEvent;
    public SentenceData[] sentences;
}
[System.Serializable]
public class GameOverDialogueData
{
    public int npcID;
    public SentenceData[] sentences;
}
[System.Serializable]
public class ItemDatabase
{
    public List<Item> items;
}

[System.Serializable]
public class DialogueDatabase
{
    public DialogueData[] dialogues;
    public GameOverDialogueData[] gameOverDialogues;
}

public class DialogueManager : MonoBehaviour
{
    public Queue<SentenceData> sentences;
    public Queue<SentenceData> gameover_sentences;
    public ItemDatabase itemDatabase { get; private set; }

    public NPC currentNPC;
    public TypeEffect typeEffect;
    public TypeEffect gameOvertypeEffect;

    [SerializeField]
    private DialogueDatabase dialogueDatabase;
    public static DialogueManager Instance { get; private set; }
    public Sprite[] npcFaces;
    private int npcID;
    [SerializeField]
    private NPC uiNpc;
    [SerializeField]
    private NPC gameoverNpc;
    public TypeEffect currentTypeEffect;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        sentences = new Queue<SentenceData>();
        gameover_sentences = new Queue<SentenceData>();
        itemDatabase = new ItemDatabase();
        LoadDialogueData();
        LoadItemData();
    }

    private void LoadDialogueData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("dialogues");
        if (jsonFile != null)
        {
            dialogueDatabase = JsonUtility.FromJson<DialogueDatabase>(jsonFile.text);
        }
        else
        {
            Debug.LogError("Failed to load dialogue data.");
        }
    }
    public void LoadItemData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("items"); // 'items.json' 파일을 불러옴
        if (jsonFile != null)
        {
            itemDatabase = JsonUtility.FromJson<ItemDatabase>(jsonFile.text);

            if (itemDatabase != null && itemDatabase.items != null)
            {
                Debug.Log($"[LoadItemData] 아이템 {itemDatabase.items.Count}개 로드 완료");

                foreach (var item in itemDatabase.items)
                {
                    Debug.Log($"[LoadItemData] id: {item.id}, 이름: {item.itemName}, 타입: {item.itemType}");
                }
            }
            else
            {
                Debug.LogError("[LoadItemData] itemDatabase나 itemDatabase.items가 null입니다.");
            }
        }
        else
        {
            Debug.LogError("[LoadItemData] items.json 파일 로드 실패");
        }
    }

    private GameOverDialogueData FindGameOverDialogue(int id)
    {
        foreach (var dialogue in dialogueDatabase.gameOverDialogues)
        {
            if (dialogue.npcID == id)
            {
                return dialogue;
            }
        }
        return null;
    }

    public bool IsEffecting()
    {
        return currentTypeEffect != null && currentTypeEffect.IsEffecting();
    }

    public void SkipTypeEffect()
    {
        if (currentTypeEffect != null && currentTypeEffect.IsEffecting())
        {
            currentTypeEffect.Skip();
        }
    }
    private void ConfigureDialogueUI(bool isEvent = false, int eventID = -1)
    {
        bool showFace = isEvent; // isEvent가 true일 때만 얼굴 이미지를 보이게 함
        int faceIndex = -1;
        Vector2 textPosition = new Vector2(isEvent ? 160f : -160f, UIManager.Instance.text.gameObject.transform.localPosition.y);

        // 얼굴 이미지를 보이게 할지 여부를 설정
        UIManager.Instance.npcFaceImage.gameObject.SetActive(showFace);

        // isEvent가 true일 때만 eventID에 따라 얼굴 이미지 설정
        if (isEvent)
        {
            switch (eventID)
            {
                case 100:
                    faceIndex = 0; // 예: eventID가 100일 때 0번 얼굴 이미지 사용
                    SoundManager.Instance.StopBGSound();
                    SoundManager.Instance.BGSoundPlay(3);
                    break;
                case 101:
                    faceIndex = 1; // 예: eventID가 101일 때 1번 얼굴 이미지 사용
                    break;
                case 1000:
                case 1001:
                case 1002:
                    faceIndex = -1; // 예: eventID가 1001일 때 1번 얼굴 이미지 사용
                    SoundManager.Instance.SFXPlay("heal_sound", 123);
                    break;
                default:
                    faceIndex = -1; // 기본값 (얼굴 이미지를 설정하지 않음)
                    break;
            }
        }

        // 얼굴 이미지를 보이게 설정하고, 유효한 인덱스일 때만 설정
        if (showFace && faceIndex >= 0 && faceIndex < npcFaces.Length)
        {
            UIManager.Instance.npcFaceImage.sprite = npcFaces[faceIndex];
        }

        UIManager.Instance.text.gameObject.transform.localPosition = textPosition;
    }



    public void StartDialogue(int id, bool isEvent = false)
    {
        npcID = id;
        sentences.Clear();
        GameManager.Instance.GetPlayerData().isStop = true;

        currentNPC.isEvent = isEvent;
        DialogueData dialogue = FindDialogue(npcID, isEvent);

        // 대화 데이터가 없는 경우 로그 출력
        if (dialogue == null)
        {
            Debug.LogWarning($"No dialogue found for NPC ID: {npcID}, isEvent: {isEvent}");
            return;
        }

        ConfigureDialogueUI(isEvent, id);

        foreach (var sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        UIManager.Instance.TextBarOpen();
        DisplayNextSentence(id);
    }

    public void StartItemDialogue(Item item)
    {
            case ItemType.Armor:
        sentences.Clear();
        GameManager.Instance.GetPlayerData().isStop = true;
        SetUINPC(); // 이벤트 대사처럼 처리

        currentNPC.isEvent = true;
        string message = "";
        ConfigureDialogueUI(false, -1);

        // 아이템 타입에 따라 메시지 설정
        switch (item.itemType)
        {
            case ItemType.HealingItem:
                message = $"* {item.itemName}을 먹었다.";

                // 회복 아이템 사용 시 체력 체크
                if (GameManager.Instance.GetPlayerData().health == GameManager.Instance.GetPlayerData().Maxhealth)
                {
                    message += "\n* 당신의 HP가 가득 찼다.";
                }
                break;

            case ItemType.Weapon:
            case ItemType.Ammor:
                message = $"* {item.itemName}을(를) 장착했다.";
                break;

            default:
                message = "* 이 아이템은 사용해도 아무 일도 일어나지 않았다.";
                break;
        }

        // 대사를 큐에 추가
        sentences.Enqueue(new SentenceData
        {
            text = message,
            expression = "Default"
        });

        // UI 열기 및 첫 번째 대사 출력
        UIManager.Instance.TextBarOpen();
        DisplayNextSentence();
    }
    public void StartInfoDialogue(Item item)
    {
        // 대화 큐 초기화
        sentences.Clear();
        GameManager.Instance.GetPlayerData().isStop = true;
        SetUINPC(); // 이벤트 대사처럼 처리

        currentNPC.isEvent = true;
        string message = "";
        ConfigureDialogueUI(false, -1);
        string item_Description = item.description;
        message = item_Description;

        // 대사를 큐에 추가
        sentences.Enqueue(new SentenceData
        {
            text = message,
            expression = "Default"
        });

        // UI 열기 및 첫 번째 대사 출력
        UIManager.Instance.TextBarOpen();
        DisplayNextSentence();
    }public void StartDropDialogue(Item item)
    {
        // 대화 큐 초기화
        sentences.Clear();
        GameManager.Instance.GetPlayerData().isStop = true;
        SetUINPC(); // 이벤트 대사처럼 처리
        string message = $"* {item.itemName} 은(는)\n    버려졌다.";

        currentNPC.isEvent = true;
        ConfigureDialogueUI(false, -1);

        // 대사를 큐에 추가
        sentences.Enqueue(new SentenceData
        {
            text = message,
            expression = "Default"
        });

        // UI 열기 및 첫 번째 대사 출력
        UIManager.Instance.TextBarOpen();
        DisplayNextSentence();
    }

    private DialogueData FindDialogue(int id, bool isEvent)
    {
        foreach (var dialogue in dialogueDatabase.dialogues)
        {
            if (dialogue.npcID == id && dialogue.isEvent == isEvent)
            {
                return dialogue;
            }
        }
        return null;
    }


    #region Game Over
    public void StartGameOverDialogue(int npcID)
    {
        gameover_sentences.Clear();
        SetOverNPC();
        GameOverDialogueData gameOverDialogue = FindGameOverDialogue(npcID);
        if (gameOverDialogue != null)
        {
            foreach (var sentence in gameOverDialogue.sentences)
            {
                // 게임 오버 대사에 대해 텍스트와 표정을 모두 큐에 추가
                gameover_sentences.Enqueue(new SentenceData
                {
                    text = sentence.text.Replace("[PLAYER_NAME]", GameManager.Instance.GetPlayerData().player_Name),
                    expression = sentence.expression
                });
            }
        }
        else
        {
            Debug.LogWarning("No game over dialogue found for NPC ID: " + npcID);
        }

        DisplayNextGameOver();
    }

    public void DisplayNextGameOver()
    {
        if (gameover_sentences.Count == 0)
        {
            End_And_LoadComplete();
            return;
        }

        SentenceData sentence = gameover_sentences.Dequeue();
        gameOvertypeEffect.SetMsg(sentence.text, OnGameOverComplete, 0);
    }


    private void OnGameOverComplete()
    {
        currentNPC.OnDialogueEffectComplete(); // 현재 NPC의 대화 완료 처리 호출

        Debug.Log(" 게임오버 대화");
    }

    #endregion

    public void DisplayNextSentence(int eventNumber = -1)
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        SentenceData sentenceData = sentences.Dequeue();

        // 텍스트 출력
        typeEffect.SetMsg(sentenceData.text, OnSentenceComplete, eventNumber, sentenceData.expression);
    }

    private void OnSentenceComplete()
    {
        Debug.Log("문장이 완료되었습니다.");
    }

    private void End_And_LoadComplete()
    {
        Debug.Log("게임오버 끝 -> Save로 넘어감");
        UIManager.Instance.End_And_Load();
    }

    private void EndDialogue(int eventNumber = 0)
    {
        if (currentNPC != null)
        {
            currentNPC.ResetToDefaultExpression(); // 기본 표정으로 복원
            currentNPC.EndDialogue();
        }
        GameManager.Instance.GetPlayerData().isStop = false;
        UIManager.Instance.CloseTextbar();

        switch (npcID)
        {
            case 1000:
            case 1001:
            case 1002:
                UIManager.Instance.SaveOpen();
                break;
        }

        npcID = -1; // npcID 초기화
    }


    public void SetCurrentNPC(NPC npc)
    {
        currentNPC = npc;
    }
    public void SetUINPC()
    {
        currentNPC = uiNpc;
    }
    public void SetOverNPC()
    {
        currentNPC = gameoverNpc;
    }
}
