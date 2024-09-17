
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleObject : MonoBehaviour
{
    public int battleNumber = 0;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BattleManager.Instance.BattleStart(battleNumber);
        }
    }
}

