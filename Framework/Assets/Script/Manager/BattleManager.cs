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

    public Transform[] bulletPoints;
    public Transform nonePoint;
    public GameObject floweybulletprefab;

    public TextAsset bossDataJson;
    //테스트용 아마도 나중에는 배열로 하든지 보스꺼를 따로 만드는지 할듯
    //일단 이건 튜토 보스용 
    public GameObject Boss_AllObject;
    public GameObject Boss_Face;
    public GameObject Boss_Face_UI;
    public GameObject Boss_Textbar;
    public TextMeshProUGUI Boss_Text;
    public GameObject battlePoint;
    public int test_curboss = 0;
    private List<GameObject> activeBullets = new List<GameObject>(); // 현재 활성화된 총알 목록

    public TypeEffect currentTypeEffect;
    

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
        test_curboss = 0;
        LoadBossData();
    }
    public void BattleSetting()
    {
        Boss_AllObject.SetActive(true);
        prevPos = player.transform.position;
        player.TeleportPlayer(battlePoint.transform.position);
        Boss_Face.gameObject.SetActive(true);
        Boss_Text.gameObject.SetActive(true);
        gameManager.ChangeGameState(GameState.Fight);
        isTalking = true;


    }
    public void BattleReSetting()
    {
        Boss_AllObject.SetActive(false);
       // player.TeleportPlayer(prevPos);
        Boss_Text.gameObject.SetActive(false);
        gameManager.ChangeGameState(GameState.None);
        Boss_Textbar.SetActive(false);
        EndDialogue();
    }

    void Update()
    {
        HandleInteraction();
    }
    public void BattleStart(int eventNumber)
    {

        gameManager.isBattle = true;
        // 사운드 재생
        SoundManager.Instance.SFXPlay("BattleStart", 0);
        // 전투 애니메이션 시작
        battleAnimator.SetTrigger("BattleStart");

        // 코루틴으로 전투를 지연시켜 시작
        prevPos = player.transform.position;
        player.TeleportPlayer(battlePoint.transform.position);
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

        if (isTalking && !UIManager.Instance.isInventroy)
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
        isTalking = true;

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
        SetBossExpression("Restare");
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
  
    private void SetBossExpression(string expression) //@@@ test 용
    {
        Debug.Log($"보스 표정 : {expression}");
        // 애니메이션 트리거 설정
        if (!string.IsNullOrEmpty(expression))
        {
            Animator animator = null; // 지역 변수 선언
            switch (test_curboss)
            {
                case 0:
                    animator = Boss_Face.GetComponent<Animator>();
                    animator?.SetTrigger(expression);
                    break;
                case 1:
                default:
                    animator = Boss_Face_UI.GetComponent<Animator>();
                    animator?.SetTrigger(expression);
                    break;

            }
        }
    }
    private void ExecuteAttack(string attack)
    {
        switch (attack)
        {
            case "Attack1":
                Debug.Log("Executing Attack 1");
                SpawnAndMoveBullets();
                break;

            case "Attack2":
                Debug.Log("Executing Attack 2");

                MoveBulletsToPlayer();
                break;

            default:
                Debug.LogWarning($"Unknown attack pattern: {attack}");
                break;
        }
    }

    // 총알을 생성하고 각 위치로 이동시키는 메서드 (Attack1)
    private void SpawnAndMoveBullets()
    {
        if (bulletPoints == null || bulletPoints.Length == 0)
        {
            Debug.LogError("Bullet points not assigned!");
            return;
        }

        for (int i = 0; i < bulletPoints.Length; i++)
        {
            // 총알 생성
            GameObject bullet = Instantiate(floweybulletprefab, nonePoint.position, Quaternion.identity);

            // 총알 리스트에 추가
            activeBullets.Add(bullet);

            // 생성된 총알을 목표 위치로 이동
            StartCoroutine(MoveBulletToTarget(bullet, bulletPoints[i].position));
        }
    }
    // 총알을 목표 위치로 이동시키는 코루틴
    private IEnumerator MoveBulletToTarget(GameObject bullet, Vector3 targetPosition)
    {
        float speed = 4f; // 총알 이동 속도

        while (bullet != null && Vector3.Distance(bullet.transform.position, targetPosition) > 0.1f)
        {
            bullet.transform.position = Vector3.MoveTowards(bullet.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null; // 다음 프레임까지 대기
        }

    }
    // Attack2에서 플레이어 방향으로 총알 이동
    private void MoveBulletsToPlayer()
    {
        if (activeBullets.Count == 0)
        {
            Debug.LogWarning("No active bullets to move.");
            return;
        }

        Vector3 playerPosition = gameManager.GetPlayerData().position;

        foreach (var bullet in activeBullets)
        {
            if (bullet != null)
            {
                // BulletController를 이용하여 방향 재설정
                BulletController bulletController = bullet.GetComponent<BulletController>();
                if (bulletController != null)
                {
                    Vector2 direction = (playerPosition - bullet.transform.position).normalized;
                    bulletController.InitializeBullet(direction, bulletController.speed, bulletController.accuracy, bulletController.damage, bulletController.maxrange);
                }
            }
        }

        // 총알을 이동 후 리스트 초기화 (필요하면 유지 가능)
        activeBullets.Clear();
    }
    private void HandleSpecialEvent(string eventType)
    {
        switch (eventType)
        {
            case "ChangeSoul":
                Debug.Log("작동왜 안함?");
                gameManager.GetPlayerData().player.GetComponent<PlayerMovement>().EnableSoul();
                SetBossExpression("Sink");  // 보스의 애니메이션 'Sink'로 설정
                test_curboss = 1;
                Boss_Face_UI.SetActive(true);
                SetBossExpression("Appear");
                //ingame sink 키고, ui 키기

                break;

            case "tutorialShot":

                //@@@수정할거
                gameManager.GetPlayerData().player.GetComponent<PlayerMovement>().tutorialDontShot = false;
                break;

            case "EndDialogue":
                // 문장이 끝나면 표정 Oh로 표정변경 0.3초뒤 "총알"이라는 대사를 "친절"로 변경 
                Debug.Log("Ending dialogue.");

                break;

            case "LowerTone":
                // 음악을 낮은 톤으로 변경
                Debug.Log("Changing music to lower tone.");
                SoundManager.Instance.PlayMusic("LowerTone");  // 음악을 변경하는 예시
                break;

            case "AttackWithBullet":
                // 대사 후 총알로 공격하는 이벤트 처리
                Debug.Log("Attacking with bullet after dialogue.");
                ExecuteAttack("Attack1");  // 예시로 Attack1을 실행
                break;

            case "CreepFace":
                // 소리나 애니메이션을 점점 느리게, creep_face로 전환
                Debug.Log("Slowing down sound and switching to creep face.");
                // 음성을 느리게 하거나 애니메이션을 변경하는 로직 추가
                SoundManager.Instance.SlowDownMusic();  // 음악을 느리게 조절하는 예시
                SetBossExpression("Smile");  // 보스의 표정을 'CreepFace'로 설정
                Debug.Log("Revelation after 1 second.");
                StartCoroutine(HandleFinalRevelation());
                break;

            default:
                Debug.LogWarning($"Unknown special event: {eventType}");
                break;
        }
    }

    private IEnumerator HandleFinalRevelation()
    {
        yield return new WaitForSeconds(1f);  // 1초 기다리기
        Debug.Log("눈치챘다는 대사: 이제 네가 뭐 하는지 알겠다.");
        // 추가적인 대사나 애니메이션 처리
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
            StartCoroutine(DelayDialogue(2, 1f));
        }
        UIManager.Instance.OnPlayerUI(); // 전투 상태에서는 UI를 보여줌
    }

    private IEnumerator DelayDialogue(int eventNumber, float delay)
    {
        yield return new WaitForSeconds(delay);
        Boss_Textbar.SetActive(true);
        StartDialogue(eventNumber);
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
