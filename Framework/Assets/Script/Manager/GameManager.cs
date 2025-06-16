using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public enum GameState
{
    Fight,
    Event,
    NpcTalk,
    None
}
public enum ItemType
{
    HealingItem,
    Weapon,
    Ammor,
    None
}
[System.Serializable]
public class PlayerData
{
    public GameObject player;
    public float Maxhealth;
    public float health;
    public Vector3 position;
    public string player_Name;
    public List<Item> inventory;
    public GameState currentState; // 플레이어의 현재 게임 상태 추가
    public bool isStop = false;
    public Animator playerAnimator;
    public bool isInvincible;
    public bool isDie;
    public bool isPhone; 


    public int LEVEL = 1;
    public int AT = 0;
    public int DF = 0;
    public int AT_level = 0;
    public int DF_level = 0;
    public int EXP = 10;
    public int NextEXP = 0;
    public int GOLD = 0;

    // 플레이어 착용중인 무기, 장갑
    public Item curWeapon;
    public Item curAmmor;

    public PlayerData()
    {
        // 초기화 로직 추가 (예: 기본값 설정)
        Maxhealth = 20;
        health = 20;
        position = Vector3.zero;
        player_Name = "frisk";
        LEVEL = 1;

        inventory = new List<Item>();// 동적으로 크기를 조절할 수 있도록 고려 가능
        currentState = GameState.None; // 초기 상태 설정
        playerAnimator = null;
        isDie = false;
        isPhone = false; 

        // 추가 데이터 초기화
    }

    public void LevelUp()
    {
        LEVEL++;
        player.GetComponent<LivingObject>().IncreaseHealth(1);

    }
    public void EquipWeapon(Item item)
    {
        curWeapon = item;
    }
    public void EquipAmmor(Item item)
    {
        curAmmor = item;
    }
    public Item GetEquippedWeapon()
    {
        return curWeapon;
    }
    public Item GetEquippedAmmor()
    {
        return curAmmor;
    }
}
[System.Serializable]
public class Item
{
    public int id;          // 아이템 고유 ID
    public string itemName; // 아이템 이름
    public string description; // 아이템 설명
    public ItemType itemType = ItemType.None;
    public Item(int id, string name, string description)
    {
        this.id = id;
        this.itemName = name;
        this.description = description;
    }
    public Item(int id, string name, string description,ItemType itemType)
    {
        this.id = id;
        this.itemName = name;
        this.description = description;
        this.itemType = itemType;
    }
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerDataSO playerDataSO; // ScriptableObject 사용
    private PlayerData runtimePlayerData; // 런타임 중 관리할 데이터

    [SerializeField] private GameConfigSO gameConfig;

    private static GameManager instance;

    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private Weapon weaponData;

    public Action<GameState> OnGameStateChanged;
    public GameObject savePrefab; // SavePoint Prefab
    public Transform[] savePointTransforms; // SavePoint 위치 배열
    private List<GameObject> instantiatedSavePoints = new List<GameObject>(); // 생성된 SavePoint 리스트


    // 감정 표현이 해금되었는지 확인할 리스트
    private List<string> unlockedEmotions = new List<string>();  // 예: "기쁨", "슬픔", "분노" 등



    /// <summary>
    /// 전투 확인용
    /// </summary>
    public bool isBattle;
    public int curportalNumber = 0;
    private float startTime;   // 게임 시작 시간
    public float savedTime;   // 이전에 저장된 시간 (누적 시간)
    private bool isSave;
    public GameObject gameoverSoul;
    public Canvas canvas;          // UI가 포함된 Canvas

    string mapName = "페허 - 잎 무더기 ";
    public bool isPortalTransition = false;

    private DialogueManager dialogueManager;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 삭제되지 않음
            InitializePlayerData(); // 런타임 데이터 초기화
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadGameConfig(); // PlayerPrefs에서 게임 설정 로드
        // 플레이어 데이터 초기화
        playerData = new PlayerData();
        weaponData = new Weapon();
    }
    private void Start()
    {
        dialogueManager = DialogueManager.Instance;

        savePrefab = gameConfig.savePrefab;
        // GameConfigSO의 위치 데이터로 SavePointTransforms 초기화
        if (gameConfig != null && gameConfig.savePointPositions != null)
        {
            savePointTransforms = new Transform[gameConfig.savePointPositions.Length];
            for (int i = 0; i < gameConfig.savePointPositions.Length; i++)
            {
                GameObject tempObj = new GameObject($"SavePointTransform_{i}");
                tempObj.transform.position = gameConfig.savePointPositions[i];
                savePointTransforms[i] = tempObj.transform;
            }
        }

        InitializeSavePoints();
        startTime = Time.time;
        isSave = PlayerPrefs.GetInt("MyBoolValue", 0) == 1 ? true : false;

        if (isSave) // 저장되있다면
        {
            Load();
            LoadGameTime();
            PortalManager.Instance.LoadLastCamera();
            //UIManager.Instance.ResetSettings();
            Item fristWaepon = new Item(49, "리볼버", "* 골동품 리볼버다.", ItemType.Weapon);
           Item fristIAmmor = new Item(48, "카우보이 모자", "* 전투로 낡은 이 모자엔 턱수염이 딱 어울릴텐데.", ItemType.Ammor);
           GetPlayerData().EquipWeapon(fristWaepon);
           GetPlayerData().EquipAmmor(fristIAmmor);
      
        }
        else
        {
            //Item fristWaepon = new Item(51, "리볼버", "골동품 리볼버다.", ItemType.Weapon);
            //Item fristIAmmor = new Item(48,"카우보이 모자", "* 전투로 낡은 이 모자엔 \n    * 턱수염이 딱 어울릴텐데.", ItemType.Ammor);
            //GetPlayerData().EquipWeapon(fristWaepon);
            //GetPlayerData().EquipAmmor(fristIAmmor);
        }
    }
    private void Update()
    {
   
        RectTransform gameoverSoulRect = gameoverSoul.GetComponent<RectTransform>();
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(GetPlayerData().position);
        if (!isBattle)
        {
            UIManager.Instance.OffPlayerUI();
        }
        else
        {
           UIManager.Instance.OnPlayerUI();
        }
        // Canvas가 Screen Space - Overlay 모드인지 확인
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // 화면 좌표를 그대로 UI의 localPosition으로 변환
            gameoverSoul.transform.position = screenPosition;
        }
        else if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            // Screen Space - Camera 또는 World Space 모드에서는 RectTransformUtility를 사용
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPosition
            );

            // 변환된 좌표를 gameoverSoul의 localPosition으로 설정
            gameoverSoul.GetComponent<RectTransform>().localPosition = localPosition;
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
               AddItem(0);
           AddItem(0);
           AddItem(52);
           AddItem(61);
        }
}
    #region RadialMenuType_method

    /// 아이템 보유 여부 확인 (Linq 없이 수동 루프)
    public bool HasItem(string itemName)
    {
        foreach (Item item in playerData.inventory)
        {
            if (item.itemName == itemName)
                return true;
        }
        return false;
    }

    /// 영혼 보유 여부 확인 - PlayerMovement에서 curWeaponType과 비교
    public bool HasSoul(string soulName)
    {
        // 문자열 → WeaponType 변환 시도
       //if (Enum.TryParse<WeaponType>(soulName, out var parsed))
       //{
       //    var current = playerData.player.GetComponent<PlayerMovement>().playerWeapons;
       //    return current == parsed;
       //}
       //
        return false;
    }

    /// 감정 표현 해금 여부 확인
    public bool CheckEmotionUnlocked(string emotionName)
    {
        return unlockedEmotions.Contains(emotionName);
    }

    /// 감정 해금 함수 (예: 이벤트 클리어 시 호출)
    public void UnlockEmotion(string emotionName)
    {
        if (!unlockedEmotions.Contains(emotionName))
            unlockedEmotions.Add(emotionName);
    }


    #endregion
    #region Savepoint
    void InitializePlayerData()
    {
        if (playerDataSO != null)
        {
            runtimePlayerData = new PlayerData
            {
                player = playerDataSO.player,
                Maxhealth = playerDataSO.Maxhealth,
                health = playerDataSO.health,
                position = playerDataSO.position,
                player_Name = playerDataSO.player_Name,
                inventory = new List<Item>(playerDataSO.inventory), // 깊은 복사
                currentState = playerDataSO.currentState,
                isStop = playerDataSO.isStop,
                playerAnimator = playerDataSO.playerAnimator,
                isInvincible = playerDataSO.isInvincible,
                isDie = playerDataSO.isDie,
                isPhone = playerDataSO.isPhone,
                LEVEL = playerDataSO.LEVEL,
                AT = playerDataSO.AT,
                DF = playerDataSO.DF,
                AT_level = playerDataSO.AT_level,
                DF_level = playerDataSO.DF_level,
                EXP = playerDataSO.EXP,
                NextEXP = playerDataSO.NextEXP,
                GOLD = playerDataSO.GOLD,
                curWeapon = playerDataSO.curWeapon,
                curAmmor = playerDataSO.curAmmor
            };
        }
        else
        {
            Debug.LogError("PlayerDataSO가 설정되지 않았습니다.");
        }
    }

    void SaveGameConfig()
    {
        if (savePointTransforms != null)
        {
            PlayerPrefs.SetInt("SavePointCount", savePointTransforms.Length);

            for (int i = 0; i < savePointTransforms.Length; i++)
            {
                PlayerPrefs.SetFloat($"SavePoint_{i}_X", savePointTransforms[i].position.x);
                PlayerPrefs.SetFloat($"SavePoint_{i}_Y", savePointTransforms[i].position.y);
                PlayerPrefs.SetFloat($"SavePoint_{i}_Z", savePointTransforms[i].position.z);
            }
        }

        // SavePrefab 경로 저장
        if (savePrefab != null)
        {
            PlayerPrefs.SetString("SavePrefabPath", savePrefab.name);
        }

        PlayerPrefs.Save();
        Debug.Log("SaveGameConfig: 게임 설정이 저장되었습니다.");
    }
    void LoadGameConfig()
    {
        int savePointCount = PlayerPrefs.GetInt("SavePointCount", 0);

        if (savePointCount > 0)
        {
            savePointTransforms = new Transform[savePointCount];

            for (int i = 0; i < savePointCount; i++)
            {
                float x = PlayerPrefs.GetFloat($"SavePoint_{i}_X", 0f);
                float y = PlayerPrefs.GetFloat($"SavePoint_{i}_Y", 0f);
                float z = PlayerPrefs.GetFloat($"SavePoint_{i}_Z", 0f);

                // Transform 생성 및 위치 설정 (임시 GameObject)
                GameObject tempObj = new GameObject($"SavePoint_{i}");
                tempObj.transform.position = new Vector3(x, y, z);
                savePointTransforms[i] = tempObj.transform;
            }
        }

          

        Debug.Log("LoadGameConfig: 게임 설정이 로드되었습니다.");
    }
    private void OnApplicationQuit()
    {
        SaveGameConfig(); // 게임 종료 시 게임 설정 저장
    }
    void InitializeSavePoints()
    {
        if (gameConfig.savePrefab == null || gameConfig.savePointPositions == null)
        {
            Debug.LogError("SavePrefab or SavePointPositions are not configured in GameConfigSO!");
            return;
        }
        for (int i = 0; i < savePointTransforms.Length; i++)
        {
            CreateSavePoint(savePointTransforms[i].position, 1000 + i);
        }
    }
    // SavePoint 생성 메서드
    // SavePoint 생성 메서드
    public void CreateSavePoint(Vector3 position, int id)
    {
        // SavePoint 인스턴스 생성
        GameObject savePoint = Instantiate(savePrefab, position, Quaternion.identity);
        instantiatedSavePoints.Add(savePoint);

        // SavePoint 초기 설정
        NPC savePointNPC = savePoint.GetComponent<NPC>();
        if (savePointNPC != null)
        {
            savePointNPC.npcID = id;
        }

        Debug.Log($"SavePoint 생성: ID={id}, 위치={position}");
    }

    // 모든 SavePoint 삭제 (필요할 경우)
    public void ClearSavePoints()
    {
        foreach (var savePoint in instantiatedSavePoints)
        {
            Destroy(savePoint);
        }
        instantiatedSavePoints.Clear();
    }


    #endregion
    public void SaveGameTime()
    {
        // 현재까지의 경과 시간을 저장 (초 단위)
        savedTime += Time.time - startTime;

        // 저장을 원한다면 PlayerPrefs 사용 (간단한 예로)
        PlayerPrefs.SetFloat("SavedGameTime", savedTime);

        // 게임을 다시 시작할 때 시간을 재설정
        startTime = Time.time;
    }
    private void LoadGameTime()
    {
        // 저장된 시간이 있으면 로드, 없으면 0으로 설정
        savedTime = PlayerPrefs.GetFloat("SavedGameTime", 0f);
    }
    public string GetElapsedTimeInMinutes()
    { // 현재 경과된 시간 계산 (초 단위)
        float elapsedTime = (Time.time - startTime) + savedTime;

        // 분과 초를 분리하여 두 자리로 표시
        string minutes = Mathf.Floor(elapsedTime / 60).ToString("00");
        string seconds = (elapsedTime % 60).ToString("00");
        return  (minutes+":"+seconds); // 분 단위로 반환
    }
    public string GetMapName()
    {
        return mapName;
    }
    public PlayerData GetPlayerData()
    {
        return playerData;

    }

    public void SavePlayerData(PlayerData newData)
    {
        // 플레이어 데이터 저장
        playerData = newData;
    }

    public Weapon GetWeaponData()
    {
        return weaponData;
    }

    public void SaveWeaponData(Weapon newData)
    {
        // 무기 데이터 저장, 일부 무기는 사용할수도?
        weaponData = newData;
    }

    public void ChangeGameState(GameState newState)
    {
        playerData.currentState = newState;
        isBattle = (newState == GameState.Fight); // 전투 상태와 연동
        OnGameStateChanged?.Invoke(newState);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void OpenUI()
    {
        UIManager.Instance.isUserInterface = true;
    }

    public void Die()
    {
        playerData.isDie = true;
        playerData.playerAnimator.SetBool("isDie",true);
        UIManager.Instance.playGameover();
        BattleManager.Instance.BattleReSetting();
        DestroyAllEnemies();

    }
    public void DestroyAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }
    public void AddItem(int id)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager.Instance가 null입니다.");
            return;
        }
        if (DialogueManager.Instance.itemDatabase == null)
        {
            Debug.LogError("itemDatabase가 null입니다.");
            return;
        }
        if (DialogueManager.Instance.itemDatabase.items == null)
        {
            Debug.LogError("itemDatabase.items가 null입니다.");
            return;
        }
        // 인벤토리가 가득 차지 않았는지 확인
        if (GetPlayerData().inventory.Count >= 9)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return;
        }

        // JSON 데이터에서 해당 id의 아이템을 찾기
        Item originItem = DialogueManager.Instance.itemDatabase.items.Find(item => item.id == id);
        if (originItem != null)
        {
            // ★ 여기 추가 ★ 복제본 생성
            Item copiedItem = new Item(originItem.id, originItem.itemName, originItem.description);

            // 타입 설정
            switch (originItem.itemType)
            {
                case ItemType.HealingItem:
                    copiedItem.itemType = ItemType.HealingItem;
                    break;
                case ItemType.Weapon:
                    copiedItem.itemType = ItemType.Weapon;
                    break;
                case ItemType.Ammor:
                    copiedItem.itemType = ItemType.Ammor;
                    break;
                default:
                    copiedItem.itemType = ItemType.None;
                    break;
            }

            GetPlayerData().inventory.Add(copiedItem);
        }
        else
        {
            Debug.LogWarning($"ID {id}에 해당하는 아이템을 찾지 못했습니다.");
        }
    }

    public void UseItem(int Id)
    {
        SoundManager.Instance.SFXPlay("select_sound", 173);

        // 인벤토리에서 유효한 아이템 ID인지 확인
        if (Id < 0 || Id >= GetPlayerData().inventory.Count)
        {
            Debug.LogWarning("Invalid item ID.");
            return;
        }

        Item itemToEquip = GetPlayerData().inventory[Id];
        ItemType itemType = itemToEquip.itemType;
        dialogueManager.StartItemDialogue(itemToEquip); // 이벤트 대사처럼 처리

        switch (itemType)
        {
            case ItemType.None:
                Debug.Log("Item does nothing.");
                break;

            case ItemType.HealingItem:
                // 체력 증가 예시
                GetPlayerData().player.GetComponent<LivingObject>().IncreaseHealth(1); 
                        
                        
                GetPlayerData().inventory.RemoveAt(Id); 
                break;

            case ItemType.Weapon:
                // 무기 착용 및 교체
                Item currentWeapon = GetPlayerData().GetEquippedWeapon();
                if (currentWeapon != null)
                {
                    // 기존에 착용한 무기를 인벤토리에 다시 추가
                    GetPlayerData().inventory.Add(currentWeapon);
                }
                // 새로운 무기 착용
                GetPlayerData().EquipWeapon(itemToEquip);
                GetPlayerData().inventory.RemoveAt(Id); 
                break;

            case ItemType.Ammor:
                // 방어구 착용 및 교체
                Item currentAmmor = GetPlayerData().GetEquippedAmmor();
                if (currentAmmor != null)
                {
                    // 기존에 착용한 방어구를 인벤토리에 다시 추가
                    GetPlayerData().inventory.Add(currentAmmor);
                }
                // 새로운 방어구 착용
                GetPlayerData().EquipAmmor(itemToEquip);
                GetPlayerData().inventory.RemoveAt(Id); 
                // 새롭게 착용된 방어구를 인벤토리에서 제거
                break;

            // 추가적인 아이템 효과는 여기서 추가 가능
            default:
                Debug.Log("Unknown item.");
                break;
        }
    }



    public void InfoItem(int Id)
    {
                SoundManager.Instance.SFXPlay("select_sound", 173);
        if (Id < 0 || Id >= GetPlayerData().inventory.Count)
        {
            Debug.LogWarning("Invalid item ID.");
            return;
        }
        Item itemToEquip = GetPlayerData().inventory[Id];

        dialogueManager.StartInfoDialogue(itemToEquip); // 이벤트 대사처럼 처리

    }

    public void DropItem(int Id)
    {
        SoundManager.Instance.SFXPlay("select_sound", 173);
        if (Id < 0 || Id >= GetPlayerData().inventory.Count)
        {
            Debug.LogWarning("Invalid item ID.");
            return;
        }

        GetPlayerData().inventory.RemoveAt(Id); // 인덱스를 그대로 사용

        // 아이템 버림 대사
                dialogueManager.SetUINPC(); // 이벤트 대사처럼 처리
        Item itemToEquip = GetPlayerData().inventory[Id];
        dialogueManager.StartDropDialogue(itemToEquip); // 이벤트 대사처럼 처리
    }


    // Save Method
    public void Save()
    {
        GetPlayerData().player.GetComponent<LivingObject>().IncreaseHealth(99);
        isSave = true;

        // 플레이어 위치 저장
        PlayerPrefs.SetFloat("PlayerPosX", playerData.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", playerData.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", playerData.position.z);

        // 체력 및 기타 플레이어 데이터 저장
        PlayerPrefs.SetFloat("PlayerHealth", playerData.Maxhealth);  // 최대 체력으로 회복
        PlayerPrefs.SetFloat("PlayerMaxHealth", playerData.Maxhealth);
        PlayerPrefs.SetString("PlayerName", playerData.player_Name);

        PlayerPrefs.SetInt("MyBoolValue", isSave ? 1 : 0);

        // 인벤토리 아이템 저장
        List<Item> inventory = playerData.inventory;
        PlayerPrefs.SetInt("InventoryCount", inventory.Count);

        for (int i = 0; i < inventory.Count; i++)
        {
            // 각 아이템의 개별 속성 저장 (ID, 이름, 설명, 타입)
            PlayerPrefs.SetInt($"Item_{i}_ID", inventory[i].id);
            PlayerPrefs.SetString($"Item_{i}_Name", inventory[i].itemName);
            PlayerPrefs.SetString($"Item_{i}_Description", inventory[i].description);
            PlayerPrefs.SetInt($"Item_{i}_Type", (int)inventory[i].itemType);
        }

        // 현재 무기 저장
        if (playerData.curWeapon != null)
        {
            PlayerPrefs.SetInt("CurWeapon_ID", playerData.curWeapon.id);
            PlayerPrefs.SetString("CurWeapon_Name", playerData.curWeapon.itemName);
            PlayerPrefs.SetString("CurWeapon_Description", playerData.curWeapon.description);
            PlayerPrefs.SetInt("CurWeapon_Type", (int)playerData.curWeapon.itemType);
        }

        // 현재 갑옷 저장
        if (playerData.curAmmor != null)
        {
            PlayerPrefs.SetInt("CurAmmor_ID", playerData.curAmmor.id);
            PlayerPrefs.SetString("CurAmmor_Name", playerData.curAmmor.itemName);
            PlayerPrefs.SetString("CurAmmor_Description", playerData.curAmmor.description);
            PlayerPrefs.SetInt("CurAmmor_Type", (int)playerData.curAmmor.itemType);
        }

        // 마지막 포탈 번호
        PlayerPrefs.SetInt("LastPortalNumber", PortalManager.Instance.lastPortalNumber);

        PlayerPrefs.Save();
        Debug.Log("게임이 저장되었습니다.");
    }



    // Load Method
    public void Load()
    {
        // 플레이어 위치 로드
        float posX = PlayerPrefs.GetFloat("PlayerPosX", 0f);
        float posY = PlayerPrefs.GetFloat("PlayerPosY", 0f);
        float posZ = PlayerPrefs.GetFloat("PlayerPosZ", 0f);
        playerData.position = new Vector3(posX, posY, posZ);

        // 체력 및 기타 플레이어 데이터 로드
        playerData.health = PlayerPrefs.GetInt("PlayerHealth", 20); // 기본 값 6
        playerData.Maxhealth = PlayerPrefs.GetInt("PlayerMaxHealth", 20);
        playerData.player_Name = PlayerPrefs.GetString("PlayerName", playerData.player_Name);

        // 인벤토리 아이템 로드
        int inventoryCount = PlayerPrefs.GetInt("InventoryCount", 0);
        List<Item> loadedInventory = new List<Item>();

        for (int i = 0; i < inventoryCount; i++)
        {
            // 개별 속성 가져오기
            int id = PlayerPrefs.GetInt($"Item_{i}_ID");
            string name = PlayerPrefs.GetString($"Item_{i}_Name");
            string description = PlayerPrefs.GetString($"Item_{i}_Description");
            ItemType itemType = (ItemType)PlayerPrefs.GetInt($"Item_{i}_Type");

            // 아이템 객체를 생성하여 리스트에 추가
            Item newItem = new Item(id, name, description, itemType);
            loadedInventory.Add(newItem);
        }
        playerData.inventory = loadedInventory;

        // 현재 무기 로드
        if (PlayerPrefs.HasKey("CurWeapon_ID"))
        {
            int weaponId = PlayerPrefs.GetInt("CurWeapon_ID");
            string weaponName = PlayerPrefs.GetString("CurWeapon_Name");
            string weaponDescription = PlayerPrefs.GetString("CurWeapon_Description");
            ItemType weaponType = (ItemType)PlayerPrefs.GetInt("CurWeapon_Type");

            // 현재 무기 아이템 생성
            playerData.curWeapon = new Item(weaponId, weaponName, weaponDescription, weaponType);
        }

        // 현재 갑옷 로드
        if (PlayerPrefs.HasKey("CurAmmor_ID"))
        {
            int armorId = PlayerPrefs.GetInt("CurAmmor_ID");
            string armorName = PlayerPrefs.GetString("CurAmmor_Name");
            string armorDescription = PlayerPrefs.GetString("CurAmmor_Description");
            ItemType armorType = (ItemType)PlayerPrefs.GetInt("CurAmmor_Type");

            // 현재 갑옷 아이템 생성
            playerData.curAmmor = new Item(armorId, armorName, armorDescription, armorType);
        }
        // 마지막 포탈 벊로 로드됨
        PortalManager.Instance.lastPortalNumber = PlayerPrefs.GetInt("LastPortalNumber", -1); // 기본값 -1

        Debug.Log("게임이 로드되었습니다.");
        if (GetPlayerData().player !=null)
        GetPlayerData().player.GetComponent<PlayerMovement>().updateLoad();
    }
    // 버튼 클릭 이벤트에 연결할 메서드
    public void ClearPlayerPrefs()
    {
        // PlayerPrefs 전체 삭제
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); // 변경 사항 저장
        Debug.Log("모든 PlayerPrefs 데이터가 삭제되었습니다.");
    }
}

