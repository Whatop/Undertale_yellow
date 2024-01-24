using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance; // ΩÃ±€≈Ê ¿ŒΩ∫≈œΩ∫

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnEventEnter(EventObject eventObject)
    {
      
    }
}
