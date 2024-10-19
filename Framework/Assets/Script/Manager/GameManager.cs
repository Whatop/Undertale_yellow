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
    Armor,
    None
}
[System.Serializable]
public class PlayerData
{
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

    // 플레이어 착용중인 무기, 장갑
    private Item curWeapon;
    private Item curAmmor;

    public PlayerData()
    {
        // 초기화 로직 추가 (예: 기본값 설정)
        Maxhealth = 6;
        health = 6;
        position = Vector3.zero;
        player_Name = "frisk";
        
        inventory = new List<Item>();// 동적으로 크기를 조절할 수 있도록 고려 가능
        currentState = GameState.None; // 초기 상태 설정
        playerAnimator = null;
        isDie = false;
        // 추가 데이터 초기화
    }

    public void IncreaseHealth(int v)
    {
        if(health < Maxhealth)
            health += v;
        else
            health = Maxhealth;

    }
    public void EquipWeapon(Item item)
    {
        curWeapon = item;
    }
    public void EquipAmmor(Item item)
    {
        curAmmor = item;
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
    private float savedTime;   // 이전에 저장된 시간 (누적 시간)
    private bool isSave;

    string mapName = "페허 - 잎 무더기 ";
    public bool isPortalTransition = false;
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
    }
    private void Start()
    {
        startTime = Time.time;
        isSave = PlayerPrefs.GetInt("MyBoolValue", 0) == 1 ? true : false;

        if (isSave) // 저장되있다면
        {
            Load();
            LoadGameTime();
            //AddItem(0, "괴물 사탕", "감초향은 아니지만 뚜렷한 맛이 난다.", ItemType.HealingItem);
            //AddItem(0, "괴물 사탕", "감초향은 아니지만 뚜렷한 맛이 난다.", ItemType.HealingItem);
            //AddItem(49,"리볼버", "골동품 리볼버다.", ItemType.Weapon);
            //AddItem(48, "카우보이 모자", "전투로 낡은 이 모자엔 턱수염이 딱 어울릴텐데.", ItemType.Armor);
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
    public void AddItem(int id, string name, string description, ItemType itemType = ItemType.None)
    {
        // 인벤토리가 가득 차지 않았는지 확인
        if (GetPlayerData().inventory.Count < 9)
        {
            Item newItem = new Item(id, name, description, itemType);
            GetPlayerData().inventory.Add(newItem);
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            //@@ 추가해
        }
    }
    public void UseItem(int Id)
    {
                SoundManager.Instance.SFXPlay("select_sound", 173);
        // Inventory에 존재하는지 확인하는 검증
        if (Id < 0 || Id >= GetPlayerData().inventory.Count)
        {
            Debug.LogWarning("Invalid item ID.");
            return;
        }

        int itemId = GetPlayerData().inventory[Id].id;
        switch (itemId)
        {
            case 0:
                Debug.Log("Item does nothing.");
                break;
            case 1:
                // 예시: 체력 증가
                GetPlayerData().IncreaseHealth(1);
                DropItem(Id); // 사용 후 삭제
                break;
            case 2:
                // 예시: 무기 착용
                GetPlayerData().EquipWeapon(GetPlayerData().inventory[Id]);
                break;
            // 다른 아이템 효과 추가 가능
            default:
                Debug.Log("Unknown item.");
                break;
        }
    }

  

    public string InfoItem(int Id)
    {
                SoundManager.Instance.SFXPlay("select_sound", 173);
        if (Id < 0 || Id >= GetPlayerData().inventory.Count)
        {
            return "Invalid item ID.";
        }

        string item_Description = GetPlayerData().inventory[Id].description;
        return item_Description;
    }

    public void DropItem(int Id)
    {
                SoundManager.Instance.SFXPlay("select_sound", 173);
        if (Id < 0 || Id >= GetPlayerData().inventory.Count)
        {
            Debug.LogWarning("Invalid item ID.");
            return;
        }

        GetPlayerData().inventory.RemoveAt(Id);
        Debug.Log("Item dropped.");
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
        PlayerPrefs.SetInt("PlayerHealth", playerData.Maxhealth);  // 최대체력으로 회복
        PlayerPrefs.SetInt("PlayerMaxHealth", playerData.Maxhealth);
        PlayerPrefs.SetString("PlayerName", playerData.player_Name);

        PlayerPrefs.SetInt("MyBoolValue", isSave ? 1 : 0);
        // 인벤토리 아이템 저장
        for (int i = 0; i < playerData.inventory.Count; i++)
        {
            PlayerPrefs.SetString("InventoryItem_" + i, playerData.inventory[i].ToString());
        }
        PlayerPrefs.SetInt("InventoryCount", playerData.inventory.Count);
        List<Item> inventory = playerData.inventory;

        // 인벤토리 크기 저장
        PlayerPrefs.SetInt("InventoryCount", inventory.Count);

        for (int i = 0; i < inventory.Count; i++)
        {
            // 각 아이템의 개별 속성 저장 (ID, 이름, 설명, 타입)
            PlayerPrefs.SetInt($"Item_{i}_ID", inventory[i].id);
            PlayerPrefs.SetString($"Item_{i}_Name", inventory[i].itemName);
            PlayerPrefs.SetString($"Item_{i}_Description", inventory[i].description);
            PlayerPrefs.SetInt($"Item_{i}_Type", (int)inventory[i].itemType);
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

        int inventoryCount = PlayerPrefs.GetInt("InventoryCount", 0);
        List<Item> loadedInventory = new List<Item>();

        for (int i = 0; i < inventoryCount; i++)
        {
            // 개별 속성 가져오기
            int id = PlayerPrefs.GetInt($"Item_{i}_ID");
            string name = PlayerPrefs.GetString($"Item_{i}_Name");
            string description = PlayerPrefs.GetString($"Item_{i}_Description");
            ItemType itemType = (ItemType)PlayerPrefs.GetInt($"Item_{i}_Type");

            // 아이템 객체를 조립해서 리스트에 추가
            Item newItem = new Item(id, name, description, itemType);
            loadedInventory.Add(newItem);
        }
        playerData.inventory = loadedInventory;

        Debug.Log("게임이 로드되었습니다.");
    }
}

