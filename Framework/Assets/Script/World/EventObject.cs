using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObject : MonoBehaviour
{
    public int EvnetNumber; // 포탈의 번호

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.Instance.OnEventEnter(this); // 포탈 진입 이벤트 호출
        }
    }
}