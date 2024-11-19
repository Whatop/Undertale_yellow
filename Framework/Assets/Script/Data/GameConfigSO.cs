using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigSO", menuName = "GameData/GameConfigSO", order = 1)]
public class GameConfigSO : ScriptableObject
{
    [Header("SavePoint Settings")]
    public GameObject savePrefab; // SavePoint 프리팹
    public Vector3[] savePointPositions; // SavePoint 위치 리스트

    [Header("Map Settings")]
    public string mapName; // 맵 이름

    [Header("Game Time Settings")]
    public float savedTime; // 저장된 게임 시간

    [Header("Sound Settings")]
    public AudioClip backgroundMusic; // 배경음악

    [Header("Other Settings")]
    public bool isDebugMode; // 디버그 모드 여부
}
