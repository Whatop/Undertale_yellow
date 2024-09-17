using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    // 적이 스폰될 위치를 담는 배열
    public Transform[] enemySpawnPoints;

    // 해당 방에서 적을 스폰하는 메서드
    public void SpawnEnemies(GameObject[] enemyPrefabs)
    {
        foreach (var spawnPoint in enemySpawnPoints)
        {
            // 적 프리팹을 랜덤으로 선택
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            // 선택된 프리팹을 스폰 위치에 생성
            Instantiate(enemyPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
        }
    }

    // 모든 적 스폰 포인트를 반환 (필요시 사용)
    public Transform[] GetSpawnPoints()
    {
        return enemySpawnPoints;
    }
}
