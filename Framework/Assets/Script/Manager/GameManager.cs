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
    public float health;
    public Vector3 position;
    public string player_Name;
    public string[] inventory;
    public GameState currentState; // 플레이어의 현재 게임 상태 추가

    public PlayerData()
    {
        // 초기화 로직 추가 (예: 기본값 설정)
        health = 100f;
        position = Vector3.zero;
        player_Name = "";
        inventory = new string[10];
        currentState = GameState.None; // 초기 상태 설정
        // 추가 데이터 초기화
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private PlayerData playerData;

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
}
