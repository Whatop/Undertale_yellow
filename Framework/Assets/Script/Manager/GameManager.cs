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
[System.Serializable]
public class PlayerData
{
    public int Maxhealth;
    public int health;
    public Vector3 position;
    public string player_Name;
    public List<string> inventory;
    public GameState currentState; // 플레이어의 현재 게임 상태 추가
    public bool isStop = false;
    public Animator playerAnimator;
    public bool isInvincible;
    public bool isDie;

    public PlayerData()
    {
        // 초기화 로직 추가 (예: 기본값 설정)
        Maxhealth = 6;
        health = 6;
        position = Vector3.zero;
        player_Name = "";
        inventory = new List<string>();// 동적으로 크기를 조절할 수 있도록 고려 가능
        currentState = GameState.None; // 초기 상태 설정
        playerAnimator = null;
        isDie = false;
        // 추가 데이터 초기화
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    private PlayerData playerData;
    private Weapon weaponData;

    public Action<GameState> OnGameStateChanged;
    /// <summary>
    /// 전투 확인용
    /// </summary>
    public bool isBattle;
    public int curportalNumber = 0;

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
}

