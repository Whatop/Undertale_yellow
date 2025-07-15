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
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            // 인스턴스 생성
            GameObject enemyInstance = Instantiate(
                enemyPrefabs[randomIndex],
                spawnPoint.position,
                Quaternion.identity
            );
            // 인스턴스를 curEnemies에 추가
            BattleManager.Instance.curEnemies.Add(enemyInstance);
        }
    }

    // 모든 적 스폰 포인트를 반환 (필요시 사용)
    public Transform[] GetSpawnPoints()
    {
        return enemySpawnPoints;
    }
}
