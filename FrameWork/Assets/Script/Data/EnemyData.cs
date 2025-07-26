using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "GameData/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float minDistance = 3f;  // 플레이어와의 최소 유지 거리
    public float maxDistance = 6f;  // 플레이어와의 최대 유지 거리

    public VirtueType virtue;

    [Tooltip("이 몬스터가 반응하는 감정 키워드 (예: 'Mercy', 'Anger')")]
    public List<string> reactableEmotions = new List<string>();

    [Header("기본 스탯")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public float shootCooldown = 4f;


    [Header("트랩 관련")]
    public bool isTrapActive = false;     // 트랩 활성화 여부
    public float trapShootInterval = 2f; // 트랩 발사 주기
    private float trapTimer = 0f;        // 트랩용 타이머

    [Header("기타")]
    public string bulletPrefabName = "Enemy_None";
}