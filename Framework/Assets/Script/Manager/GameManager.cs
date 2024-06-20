using UnityEngine;
using System;
public enum GameState
{
    Fight,
    Event,
    NpcTalk,
    None
}

[System.Serializable]
public class PlayerData
{
    public int Maxhealth;
    public int health;
    public Vector3 position;
    public string player_Name;
    public string[] inventory;
    public GameState currentState; // 플레이어의 현재 게임 상태 추가

    public PlayerData()
    {
        // 초기화 로직 추가 (예: 기본값 설정)
        Maxhealth = 10;
        health = 6;
        position = Vector3.zero;
        player_Name = "";
        inventory = new string[10]; // 동적으로 크기를 조절할 수 있도록 고려 가능
        currentState = GameState.None; // 초기 상태 설정
        // 추가 데이터 초기화
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private PlayerData playerData;
    private Weapon weaponData;

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
        // 상태에 따른 추가적인 동작 수행
        switch (newState)
        {
            case GameState.Fight:
                // Fight 상태에 따른 동작 수행
                break;
            case GameState.Event:
                // Event 상태에 따른 동작 수행
                break;
            case GameState.NpcTalk:
                // NpcTalk 상태에 따른 동작 수행
                break;
            default:
                break;
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void OpenUI()
    {
        UIManager.Instance.isUserInterface = true;
    }
}

