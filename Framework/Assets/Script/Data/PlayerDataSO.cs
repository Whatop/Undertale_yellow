using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "GameData/PlayerDataSO", order = 1)]
public class PlayerDataSO : ScriptableObject
{
    public Item curArmor; //  
        curArmor = null;
    public int health = 6; // 현재 체력
    public Vector3 position = Vector3.zero; // 초기 위치
    public string player_Name = "frisk"; // 플레이어 이름

    public List<Item> inventory = new List<Item>(); // 초기 인벤토리 설정
    public GameState currentState = GameState.None; // 초기 상태
    public bool isStop = false; // 플레이어 동작 제어
    public Animator playerAnimator; // 플레이어 애니메이터
    public bool isInvincible = false; // 무적 여부
    public bool isDie = false; // 사망 여부
    public bool isPhone = false; // 특정 상태(폰 사용 여부 등)

    public int LEVEL = 1; // 초기 레벨
    public int AT = 0; // 공격력
    public int DF = 0; // 방어력
    public int AT_level = 0; // 공격력 레벨
    public int DF_level = 0; // 방어력 레벨
    public int EXP = 10; // 경험치
    public int NextEXP = 0; // 다음 레벨까지 경험치
    public int GOLD = 0; // 초기 골드

    public Item curWeapon; // 착용 무기
    public Item curAmmor; // 착용 방어구

    /// <summary>
    /// 초기 데이터를 설정하는 메서드 (에디터에서 호출 가능)
    /// </summary>
    public void ResetData()
    {
        Maxhealth = 6;
        health = 6;
        position = Vector3.zero;
        player_Name = "frisk";
        inventory.Clear();
        currentState = GameState.None;
        isStop = false;
        playerAnimator = null;
        isInvincible = false;
        isDie = false;
        isPhone = false;
        LEVEL = 1;
        AT = 0;
        DF = 0;
        AT_level = 0;
        DF_level = 0;
        EXP = 10;
        NextEXP = 0;
        GOLD = 0;
        curWeapon = null;
        curAmmor = null;
    }
}
