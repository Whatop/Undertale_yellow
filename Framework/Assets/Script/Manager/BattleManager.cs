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
[System.Serializable]
public class BulletPool
{
    public string bulletName;
    public GameObject prefab;
    public int poolSize;
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
    [SerializeField]
    private List<GameObject> activeBullets = new List<GameObject>(); // 현재 활성화된 총알 목록

    public TypeEffect currentTypeEffect;
    
    //플레이어
    public PlayerMovement player;
    //현재 적
    // 전투 중 등장하는 "현재 활성화된 적" 리스트
    public List<GameObject> curEnemies = new List<GameObject>();
    [SerializeField]
    private List<Transform> bulletSpawnTransforms = new List<Transform>();

    private Vector2 prevPos;

    private BossBattleData currentBoss;
    private int currentDialogueIndex = 0;

    public GameObject bulletPointPrefab; // 생성할 프리팹 (유니티 인스펙터에서 지정)
    public Transform[] bulletspawnPoint; // 총알 생성위치
    public Transform spawnParent;   // 생성된 오브젝트를 담을 부모 오브젝트 (정리용)


    [SerializeField]
    private List<BulletPool> bulletPools;

    private Dictionary<string, List<GameObject>> bulletPoolDict;

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

        InitializeBulletPools();
        // 필수 상태 초기화
        isTalking = false;
        isFirstInteraction = true;
        isEvent = false;
        test_curboss = 0;
        GenerateGrid(40.60f, 59.54f, 5, 6.00f, -4.05f, 12);
        LoadBossData();
    }

    #region ObjectPool
    private void InitializeBulletPools()
    {
        bulletPoolDict = new Dictionary<string, List<GameObject>>();

        foreach (var pool in bulletPools)
        {
            List<GameObject> objectList = new List<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectList.Add(obj);
            }

            bulletPoolDict.Add(pool.bulletName, objectList);
        }
    }
    private GameObject GetBulletFromPool(string key)
    {
        if (!bulletPoolDict.ContainsKey(key))
        {
            Debug.LogWarning($"총알 풀 '{key}'이 존재하지 않습니다.");
            return null;
        }

        var poolList = bulletPoolDict[key];
        foreach (var obj in poolList)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }

        // 모두 사용 중이면 새로 생성
        GameObject prefab = bulletPools.Find(p => p.bulletName == key)?.prefab;
        if (prefab == null)
        {
            Debug.LogError($"'{key}'에 대한 프리팹을 찾을 수 없습니다.");
            return null;
        }

        GameObject newObj = Instantiate(prefab);
        newObj.SetActive(false);
        poolList.Add(newObj);
        return newObj;
    }


    #endregion
    #region BulletPoints
    private void GenerateGrid(float xStart, float xEnd, int xCount, float yStart, float yEnd, int yCount)
    {
        float[] xPositions = GeneratePositions(xStart, xEnd, xCount);
        float[] yPositions = GeneratePositions(yStart, yEnd, yCount);

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                Vector3 spawnPosition = new Vector3(xPositions[i], yPositions[j], 0);
                GameObject obj = Instantiate(bulletPointPrefab, Vector3.zero, Quaternion.identity, spawnParent);
                obj.transform.position = spawnPosition;  // 위치 설정
                obj.name = $"Bullet_{i}_{j}"; // 이름 설정

                bulletSpawnTransforms.Add(obj.transform); // Transform 저장
            }
        }
    }

    private float[] GeneratePositions(float start, float end, int count)
    {
        float[] positions = new float[count];

        for (int i = 0; i < count; i++)
        {
            positions[i] = Mathf.Lerp(start, end, (float)i / (count - 1));
        }

        return positions;
    }
    #endregion
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
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            //SetAttack("Split", 0, 1f);//분열
            BattleManager.Instance.SpawnBulletAtPosition(
            BulletType.Laser,
            bulletPoints[28].position,
            Quaternion.identity,
            Vector2.right, // 또는 원하는 방향
            "Gaster_Laser",
            0,
            0f,
        false
    );

        }
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    SetAttack("Spiral_S", 10, 1f);//회오리
        //    SetAttack("Spiral_S", 24, 1f);//회오리
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    SetAttack("Homing", 0, 1f);//유도
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    for(int i =0; 12>i; i++)
        //        SetAttack("Right", i, 1f);

        //    for (int i = 48; 60 > i; i++)
        //        SetAttack("Left", i, 1f);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha8))
        //{
        //    for (int i = 0; 12 > i; i++)
        //        SetAttack("None", i, 1f);

        //    for (int i = 48; 60 > i; i++)
        //        SetAttack("None", i, 1f);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    SetAttack("Normal", 29, 1f);
        //    SetAttack("Normal", 30, 1f);
        //    SetAttack("Normal", 31, 1f);
        //    SetAttack("Normal", 32, 1f);
        //    SetAttack("Normal", 33, 1f);
        //    SetAttack("Normal", 34, 1f);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    SetAttack("Speed", 0, 1f);
        //    SetAttack("Speed", 1, 1f);
        //    SetAttack("Speed", 2, 1f);
        //    SetAttack("Speed", 3, 1f);
        //    SetAttack("Speed", 4, 1f);
        //    SetAttack("Speed", 5, 1f);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha6))
        //{
        //    SetAttack("Spiral_M", 10, 1f);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha7))
        //{
        //    SetAttack("Spiral_R", 24, 1f);
        //}
    }
    private void LateUpdate()
    {
        // 비활성화된 총알 제거
        activeBullets.RemoveAll(b => b == null || !b.activeInHierarchy);
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
        curEnemies.Add(enemyPrefabs[0]);
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
        // 특수 이벤트 처리
        if (!string.IsNullOrEmpty(dialogue.eventType))
        {
            HandleSpecialEvent(dialogue.eventType, dialogue.text);
        }
        else
        {
            currentTypeEffect.SetMsg(dialogue.text, OnSentenceComplete, 100, dialogue.expression);

            // 표정 설정
            SetBossExpression(dialogue.expression);
        }
        // 공격 패턴 실행
        if (!string.IsNullOrEmpty(dialogue.attack))
        {
            ExecuteAttack(dialogue.attack);
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
    #region Attack
    //MoveBulletsInDirection
    //HomingBullets
    //SpiralBullets
    //SplitBullets
    public void ChangeAllBulletTypes(BulletType newType)
    {
        foreach (var bullet in activeBullets)
        {
            if (bullet != null)
            {
                BulletController bulletController = bullet.GetComponent<BulletController>();
                if (bulletController != null)
                {
                    bulletController.ChangeBulletType(newType);
                }
            }
        }
    }

    private void ExecuteAttack(string attack)
    {
        switch (attack)
        {
            case "Attack1":
                Debug.Log("Executing Attack 1");
                // SpawnAndMoveBullets();
                SetAttack("Directional");//벙형
                SetAttack("Spiral");//회오리
                SetAttack("Split");//분열
                SetAttack("Homing");//유도
                SetAttack("Homing");
                break;

            case "Attack2":
                Debug.Log("Executing Attack 2");

              //  MoveBulletsToPlayer(true);  // accelerate = true
                break;

            case "Attack3":
                Debug.Log("왼쪽 공격");
                //SpawnAndMoveBullets();
               // MoveBulletsInDirection(Vector2.left,10);
                break;


            case "Attack4":
                Debug.Log("유도");
               // SpawnAndMoveBullets();
                //StartCoroutine(HomingBullets(10.5f, 8f));

                break;

            case "Attack5":
                Debug.Log("회오리");
               // SpawnAndMoveBullets();
               // StartCoroutine(SpiralBullets(120, 2.5f));

                break;

            case "Attack6":
                Debug.Log("분열");
               // SpawnAndMoveBullets();
               // StartCoroutine(SplitBullets(10, 8f));

                break;

            default:
                Debug.LogWarning($"Unknown attack pattern: {attack}");
                break;
        }
    }

    void SetAttack(string attack,int bulletpoint = 0, float delay = 0f)
    {
        switch (attack)
        {

            case "Left":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, Vector2.left.normalized);
                break;
            case "Right":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, Vector2.right.normalized);
                break;
            case "Up":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, Vector2.up.normalized);
                break;
            case "Down":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, Vector2.down.normalized);
                break;
            case "UpLeft":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, new Vector2(-1, 1).normalized);
                break;
            case "UpRight":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, new Vector2(1, 1).normalized);
                break;
            case "DownLeft":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, new Vector2(-1, -1).normalized);
                break;
            case "DownRight":
                SpawnBullets(BulletType.Directional, bulletpoint, delay, new Vector2(1, -1).normalized);
                break;
            case "FixedPoint":
                SpawnBullets(BulletType.FixedPoint, bulletpoint, delay);
                break;
            case "Normal":
                SpawnBullets(BulletType.Normal, bulletpoint, delay);
                break;
            case "Homing":
                SpawnBullets(BulletType.Homing, bulletpoint, delay);
                break;
            case "Spiral_S":
                SpawnBullets(BulletType.Spiral, bulletpoint, delay, default, 0);
                break;
            case "Spiral_M":
                SpawnBullets(BulletType.Spiral, bulletpoint, delay, default, 1);
                break;
            case "Spiral_R":
                SpawnBullets(BulletType.Spiral, bulletpoint, delay, default, 2);
                break;
            case "Split":
                SpawnBullets(BulletType.Split, bulletpoint, delay);
                break;
            case "Speed":
                SpawnBullets(BulletType.Speed, bulletpoint, delay);
                break;
            case "None":
                SpawnBullets(BulletType.None, bulletpoint, delay);
                break;
            default:
                Debug.LogWarning($"Unknown attack pattern: {attack}");
                break;
        }
    }
    // 총알을 스폰하고 특정 타입의 패턴을 적용하는 메서드
    public void SpawnBullets(
       BulletType bulletType,
       int bulletpoint = -1,
       float delay = 0f,
       Vector2 dir = default,
       int size = 0,
       string prefab = "Flowey_Normal"
   )
    {
        if (prefab == "None") return;

        Transform spawnPoint = bulletpoint == -1
            ? bulletspawnPoint[0]
            : (bulletpoint < 30 ? bulletspawnPoint[0] : bulletspawnPoint[1]);

        Vector3 spawnPos = spawnPoint.position;

        GameObject bullet = GetBulletFromPool(prefab);
        if (bullet == null) return;

        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.identity;
        bullet.SetActive(true);

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            Transform target = bulletpoint != -1 ? bulletSpawnTransforms[bulletpoint] : null;
            bc.InitializeBullet(dir, 5f, 0f, 1, 15f, delay, bulletType, target, size);
            activeBullets.Add(bullet);
        }
    }

    public void SpawnBulletAtPosition(
    BulletType type,
    Vector2 position,
    Quaternion rotation,
    Vector2 dir,
    string prefab = "None",
    int size = 0,
    float delay = 0f, 
    bool isfriends = false
)
    {
        if (prefab == "None") return;

        GameObject bullet = GetBulletFromPool(prefab);
        if (bullet == null) return;

        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.SetActive(true);

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
        {
            bc.InitializeBullet(dir, 5f, 0f, 1, 15f, delay, type, null, size, isfriends);
            activeBullets.Add(bullet);
        }
    }



    #endregion
    private void HandleSpecialEvent(string eventType,string dialogue)
    {
        switch (eventType)
        {
            case "ChangeSoul":
                currentTypeEffect.Clear();
                gameManager.GetPlayerData().player.GetComponent<PlayerMovement>().EnableSoul(0.7f);
                gameManager.GetPlayerData().player.GetComponent<PlayerMovement>().MakePlayerTransparent();
                SetBossExpression("Sink");  // 보스의 애니메이션 'Sink'로 설정
                test_curboss = 1;
                Boss_Face_UI.SetActive(true);
                SetBossExpression("Appear");
                //ingame sink 키고, ui 키기
                StartCoroutine(FloweyAnimationThenNextDialogue(dialogue,1.5f));

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
    // 플라위 애니메이션이 끝난 후 다음 대사를 진행하는 코루틴
    private IEnumerator FloweyAnimationThenNextDialogue(string dialogue, float waitTime)
    {
        // waitTime 동안 대기(애니메이션이 끝날 때까지 혹은 넉넉히 잡아둔 시간)
        yield return new WaitForSeconds(waitTime);
        SetBossExpression("Talking");
        currentTypeEffect.SetMsg(dialogue, OnSentenceComplete, 100);
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
            //GameObject spawnedEnemy = Instantiate(enemyPrefabs[0], somePosition, Quaternion.identity);
            //curEnemies.Add(spawnedEnemy);
        }
        else
        {
            Debug.LogWarning("Current Room is not set.");
        }
    }
    // 적이 사망하거나 사라졌을 때 리스트에서 제거
    public void RemoveEnemy(GameObject enemy)
    {
        //BattleManager.Instance.RemoveEnemy(this.gameObject)
        if (curEnemies.Contains(enemy))
        {
            curEnemies.Remove(enemy);
        }
    }
}
