using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewEnemyData", menuName = "GameData/EnemyData")]
public class EnemyData : ScriptableObject
{
    public float minDistance = 3f;  // 플레이어와의 최소 유지 거리
    public float maxDistance = 6f;  // 플레이어와의 최대 유지 거리

    public VirtueType virtue;
    public enum ReactionType
    {
        SpeedUp, SlowDown,
        BulletSpeedUp, BulletSpeedDown,
        Heal, TakeDamage,
        Stun, StopAttack, Invincible, Undying,
        Flee,                       // 도망/전투 이탈 등
        PlayAnim, PlayEffect,       // 연출용
        Custom                      // 커스텀 훅
    }
    [System.Serializable]
    public class EmotionReaction
    {
        public string emotionKey;        // 반응한 내용
        public ReactionType action;     // 위의 ReactionType
        public float amount;            // 수치(속도배율, 회복량 등)
        public float duration;          // 지속시간(초) 필요 시
        public string reactionText;        // 반응한 내용
        public string animTrigger;      // 애니메이터 트리거(선택)
        public string effectName;       // 이펙트 풀 키(선택)
        public bool onlyOnce;           // 1회만 반응할지
    }
    public string GetReaction(string emotion)
    {
        foreach (var r in emotionKeys)
        {
            if (r == emotion)
            {
                foreach (var i in reactableEmotions)
                {
                    if(i.emotionKey == emotion)
                    {
                        return i.reactionText;
                    }
                }

            }
        }
        return string.Empty;
    }
    [Tooltip("이 몬스터가 반응하는 감정 키워드 (예: 'Mercy', 'Anger')")]
    // 감정과 반응 텍스트를 매핑한 리스트입니다.
    public List<EmotionReaction> reactableEmotions = new List<EmotionReaction>();

    //감정표현 세팅용
    public List<string> emotionKeys = new List<string>();

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