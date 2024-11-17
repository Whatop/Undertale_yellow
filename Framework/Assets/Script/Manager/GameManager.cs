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
    public int Maxhealth;
    public int health;
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
        Maxhealth = 6;
        health = 6;
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

    public void IncreaseHealth(int v)
    {
        if(health < Maxhealth)
            health += v;
        else
            health = Maxhealth;

    }
    public void LevelUp()
    {
        LEVEL++;
        IncreaseHealth(10);
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
    private static GameManager instance;

    [SerializeField]
    private PlayerData playerData;
    [SerializeField]
    private Weapon weaponData;

    public Action<GameState> OnGameStateChanged;
    /// <summary>
    /// 전투 확인용
    /// </summary>
    public bool isBattle;
    public int curportalNumber = 0;
    private float startTime;   // 게임 시작 시간
    public float savedTime;   // 이전에 저장된 시간 (누적 시간)
    private bool isSave;

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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 플레이어 데이터 초기화
        playerData = new PlayerData();
        weaponData = new Weapon();
        dialogueManager = DialogueManager.Instance;
    }
    private void Start()
    {
        startTime = Time.time;
        isSave = PlayerPrefs.GetInt("MyBoolValue", 0) == 1 ? true : false;

        if (isSave) // 저장되있다면
        {
            Load();
            LoadGameTime();
            //UIManager.Instance.ResetSettings();
         
            Item fristWaepon = new Item(49, "리볼버", "* 골동품 리볼버다.", ItemType.Weapon);
            Item fristIAmmor = new Item(48, "카우보이 모자", "* 전투로 낡은 이 모자엔 턱수염이 딱 어울릴텐데.", ItemType.Ammor);
            GetPlayerData().EquipWeapon(fristWaepon);
            GetPlayerData().EquipAmmor(fristIAmmor);

        }
        else
        {
            //AddItem(0);
            //AddItem(0);
            //AddItem(52);
            //AddItem(61);
            //Item fristWaepon = new Item(51, "리볼버", "골동품 리볼버다.", ItemType.Weapon);
            //Item fristIAmmor = new Item(48,"카우보이 모자", "* 전투로 낡은 이 모자엔 \n    * 턱수염이 딱 어울릴텐데.", ItemType.Ammor);
            //GetPlayerData().EquipWeapon(fristWaepon);
            //GetPlayerData().EquipAmmor(fristIAmmor);
        }
    }
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
        // 인벤토리가 가득 차지 않았는지 확인
        if (GetPlayerData().inventory.Count >= 9)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return;
        }

        // JSON 데이터에서 해당 id의 아이템을 찾기
        Item itemToAdd = dialogueManager.itemDatabase.items.Find(item => item.id == id);
        if (itemToAdd != null)
        {
            // ID 범위에 따라 아이템 타입 설정
            if (itemToAdd.id >= 0 && itemToAdd.id <= 50)
            {
                itemToAdd.itemType = ItemType.HealingItem;
            }
            else if (itemToAdd.id >= 51 && itemToAdd.id <= 60)
            {
                itemToAdd.itemType = ItemType.Ammor;
            }
            else if (itemToAdd.id >= 61 && itemToAdd.id <= 70)
            {
                itemToAdd.itemType = ItemType.Weapon;
            }
            else
            {
                itemToAdd.itemType = ItemType.None;
            }

            // 설정된 타입과 함께 아이템을 인벤토리에 추가
            GetPlayerData().inventory.Add(itemToAdd);
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
                GetPlayerData().IncreaseHealth(1); 
                        
                        
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
        isSave = true;

        // 플레이어 위치 저장
        PlayerPrefs.SetFloat("PlayerPosX", playerData.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", playerData.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", playerData.position.z);

        // 체력 및 기타 플레이어 데이터 저장
        PlayerPrefs.SetInt("PlayerHealth", playerData.Maxhealth);  // 최대 체력으로 회복
        PlayerPrefs.SetInt("PlayerMaxHealth", playerData.Maxhealth);
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
        playerData.health = PlayerPrefs.GetInt("PlayerHealth", 6); // 기본 값 6
        playerData.Maxhealth = PlayerPrefs.GetInt("PlayerMaxHealth", 6);
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

        Debug.Log("게임이 로드되었습니다.");
        if (GetPlayerData().player !=null)
        GetPlayerData().player.GetComponent<PlayerMovement>().updateLoad();
    }

}

