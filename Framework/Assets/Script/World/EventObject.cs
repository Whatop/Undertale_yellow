using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObject : MonoBehaviour
{
    public NPC eventNPC;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (EventManager.Instance != null)
            {
                DialogueManager.Instance.SetCurrentNPC(eventNPC);
                EventManager.Instance.TriggerEvent(eventNPC.npcID);
            }
            else
            {
                Debug.LogWarning("EventManager instance is null.");
            }

            Destroy(gameObject);
        }
    }
}
