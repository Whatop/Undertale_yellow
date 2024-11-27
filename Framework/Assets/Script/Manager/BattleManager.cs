using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class BossDialogue
{
    public string text;
    public string expression;
    public string attack; // 공격 패턴
    public string direction; // 방향 (Left, Right 등)
    public string eventType; // 특수 이벤트
    public string music; // 음악 설정
    public int skipToDialogue; // 특정 대사로 이동
}

[System.Serializable]
public class BossBattleData
{
    public int bossID;
    public string name;
    public List<BossDialogue> dialogues;
}

[System.Serializable]
public class BattleDatabase
{
    public List<BossBattleData> bossBattles;
}

public class BattleManager : MonoBehaviour
{
    public Queue<BossDialogue> boss_sentences;
    [SerializeField]
    private BattleDatabase battleDatabase;
    private static BattleManager instance;
    public GameObject battleObject;
    Animator battleAnimator;
    GameManager gameManager;

    public enum BattleState { None, BasicBattle, BossBattle }  // 전투 상태를 정의
    public BattleState currentState;

    public GameObject[] roomPrefabs;  // 여러 전투 방 프리팹 배열
    public Transform roomSpawnPoint;


    public GameObject[] enemyPrefabs;  // 적 프리팹 배열
    public Room currentRoom;  // 현재 방

    public TextAsset bossDataJson;
    //테스트용 아마도 나중에는 배열로 하든지 보스꺼를 따로 만드는지 할듯
    //일단 이건 튜토 보스용 
    public GameObject Boss_AllObject;
    public GameObject Boss_Face;
    public TextMeshProUGUI Boss_Text;
    public GameObject battlePoint;

    public TypeEffect currentTypeEffect;
    public GameObject Boss_Wall;

    public PlayerMovement player;
    private Vector2 prevPos;

    private BossBattleData currentBoss;
    private int currentDialogueIndex = 0;
    public static BattleManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BattleManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("BattleManager");
                    instance = obj.AddComponent<BattleManager>();
                }
            }
            return instance;
        }
    }
    public bool isTalking = false;
    private bool isFirstInteraction = true; // 처음 대화인지 확인
    public bool isEvent = false;
    public int boss_id;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        battleAnimator = battleObject.GetComponent<Animator>();
        battleObject.SetActive(true);
        boss_sentences = new Queue<BossDialogue>();

        // 필수 상태 초기화
        isTalking = false;
        isFirstInteraction = true;
        isEvent = false;
        LoadBossData();
    }
    public void BattleSetting()
    {
        Boss_AllObject.SetActive(true);
        StartDialogue(1);
        prevPos = player.transform.position;
        player.TeleportPlayer(battlePoint.transform.position);
        Boss_Text.gameObject.SetActive(true);
        Boss_Wall.gameObject.SetActive(true);
        gameManager.ChangeGameState(GameState.Fight);
        isTalking = true;

    }
    void BattleReSetting()
    {
        Boss_AllObject.SetActive(false);
        player.TeleportPlayer(prevPos);
        Boss_Text.gameObject.SetActive(false);
        Boss_Wall.gameObject.SetActive(false);
        gameManager.ChangeGameState(GameState.None);

    }

    void Update()
    {
        HandleInteraction();
    }
    public void BattleStart(int eventNumber)
    {
        gameManager.GetPlayerData().player.GetComponent<PlayerMovement>().EnableSoul();
        gameManager.isBattle = true;
        // 사운드 재생
        SoundManager.Instance.SFXPlay("BattleStart", 0);
        // 전투 애니메이션 시작
        battleAnimator.SetTrigger("BattleStart");

        // 코루틴으로 전투를 지연시켜 시작
        StartCoroutine(StartBattleAfterDelay(eventNumber, 1.5f));
    }
    private void LoadBossData()
    {
        if (bossDataJson != null)
        {
            battleDatabase = JsonUtility.FromJson<BattleDatabase>(bossDataJson.text);
        }
        else
        {
            Debug.LogError("Boss data JSON is not assigned.");
        }
    }

    public void StartBossBattle(int bossID)
    {
        if (battleDatabase == null || battleDatabase.bossBattles == null)
        {
            Debug.LogError("Battle database is empty.");
            return;
        }

        currentBoss = battleDatabase.bossBattles.Find(boss => boss.bossID == bossID);
        if (currentBoss == null)
        {
            Debug.LogError($"Boss with ID {bossID} not found.");
            return;
        }

        currentDialogueIndex = 0;
        DisplayNextDialogue();
    }
    
    private void HandleInteraction()
    {

        if (isTalking)
        {
            // 대화 중에는 상호작용 제한
            UIManager.Instance.isInventroy = false;

            // 타이핑 효과 중인 경우
            if (IsEffecting())
            {
                if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
                {
                    SkipTypeEffect();
                    UIManager.Instance.isSaveDelay = true;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space))
            {
                DisplayNextDialogue();
            }

            return; // 대화 중이므로 나머지 상호작용은 처리하지 않음
        }

      
    }
    public void StartDialogue(int bossID)
    {
        boss_sentences.Clear();

        // 보스 데이터 검색
        currentBoss = FindBossBattle(bossID);
        if (currentBoss == null)
        {
            Debug.LogError($"Boss with ID {bossID} not found.");
            return;
        }

        // 대사 데이터를 큐에 추가
        foreach (var dialogue in currentBoss.dialogues)
        {
            boss_sentences.Enqueue(new BossDialogue
            {
                text = dialogue.text,
                expression = dialogue.expression,
                attack = dialogue.attack,
                direction = dialogue.direction,
                eventType = dialogue.eventType,
                music = dialogue.music,
                skipToDialogue = dialogue.skipToDialogue,
            });
        }

        // 첫 번째 대사 출력
        DisplayNextDialogue();
    }

    private void DisplayNextDialogue()
    {
        if (boss_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        var dialogue = boss_sentences.Dequeue();
        currentTypeEffect.SetMsg(dialogue.text, OnSentenceComplete, 100, dialogue.expression);

        // 표정 설정
        SetBossExpression(dialogue.expression);

        // 공격 패턴 실행
        if (!string.IsNullOrEmpty(dialogue.attack))
        {
            ExecuteAttack(dialogue.attack);
        }

        // 특수 이벤트 처리
        if (!string.IsNullOrEmpty(dialogue.eventType))
        {
            HandleSpecialEvent(dialogue.eventType);
        }

        // 특정 대사로 건너뛰기 처리
        if (currentBoss.dialogues[currentDialogueIndex].skipToDialogue > 0)
        {
            currentDialogueIndex = currentBoss.dialogues[currentDialogueIndex].skipToDialogue - 1;
            DisplayNextDialogue(); // 건너뛴 대사로 즉시 이동
        }
        else
        {
            currentDialogueIndex++;
        }
    }


    private BossBattleData FindBossBattle(int bossID)
    {
        if (battleDatabase == null || battleDatabase.bossBattles == null)
        {
            Debug.LogError("Battle database is not loaded or empty.");
            return null;
        }

        foreach (var bossBattle in battleDatabase.bossBattles)
        {
            if (bossBattle.bossID == bossID)
            {
                return bossBattle;
            }
        }

        Debug.LogWarning($"No boss battle found for Boss ID: {bossID}");
        return null;
    }

    private void OnSentenceComplete()
    {
        SetBossExpression("Default");
        Debug.Log("보스 문장이 완료되었습니다.");
    }

    public void EndDialogue()
    {
        isTalking = false;
        isEvent = false;
        isFirstInteraction = true;
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
        private void SetBossExpression(string expression)
    {
        Debug.Log($"Setting boss expression to: {expression}");
        // 애니메이션 트리거 설정
        if (!string.IsNullOrEmpty(expression))
        {
            var animator = Boss_Face.GetComponent<Animator>();
            animator?.SetTrigger(expression);
        }
    }

    private void ExecuteAttack(string attack)
    {
        switch (attack)
        {
            case "Attack1":
                Debug.Log("Executing Attack 1");
                // Attack 1에 대한 구체적인 로직 추가
                break;

            case "Attack2":
                Debug.Log("Executing Attack 2");
                // Attack 2에 대한 구체적인 로직 추가
                break;

            default:
                Debug.LogWarning($"Unknown attack pattern: {attack}");
                break;
        }
    }

    private void HandleSpecialEvent(string eventType)
    {
        switch (eventType)
        {
            case "MoveToReceiveFriendly":
                Debug.Log("Moving to receive friendly items.");
                // 플레이어 이동 관련 처리 로직
                break;

            case "EndDialogue":
                Debug.Log("Ending dialogue.");
                currentDialogueIndex = currentBoss.dialogues.Count; // 대화 종료
                break;

            default:
                Debug.LogWarning($"Unknown special event: {eventType}");
                break;
        }
    }

    private IEnumerator StartBattleAfterDelay(int eventNumber, float delay)
    {
        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(delay);

        // 기본 전투 혹은 보스 전투 시작
        if (eventNumber == 1)
        {
            StartBasicBattle();
        }
        else if (eventNumber == 2)
        {
            StartBossBattle();
            BattleSetting();
        }
        UIManager.Instance.OnPlayerUI(); // 전투 상태에서는 UI를 보여줌

    }


    // 기본 전투 시작
    void StartBasicBattle()
    {
        currentState = BattleState.BasicBattle;
        gameManager.ChangeGameState(GameState.Fight);

        // 랜덤으로 방을 생성
        //int randomRoomIndex = Random.Range(0, roomPrefabs.Length);
        //currentRoom = Instantiate(roomPrefabs[randomRoomIndex], roomSpawnPoint.position, Quaternion.identity).GetComponent<Room>();

        // 적을 스폰
        SpawnEnemies();
    }

    // 보스 전투 시작
    void StartBossBattle()
    {
        currentState = BattleState.BossBattle;
        gameManager.ChangeGameState(GameState.Fight);
        // 보스 방 생성, 이동?
        // 카메라 변경



        // 보스 적을 스폰
        NextPatturn();
    }
    
    void NextPatturn()
    {

    }
    // 적 스폰 로직
    void SpawnEnemies()
    {
        if (currentRoom != null)
        {
            currentRoom.SpawnEnemies(enemyPrefabs);
        }
        else
        {
            Debug.LogWarning("Current Room is not set.");
        }
    }
}
