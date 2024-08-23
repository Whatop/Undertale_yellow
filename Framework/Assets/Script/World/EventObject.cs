using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObject : MonoBehaviour
{
    public int EventNumber; // Æ÷Å»ÀÇ ¹øÈ£


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.Instance.TriggerEvent(EventNumber);
        }
    }
}