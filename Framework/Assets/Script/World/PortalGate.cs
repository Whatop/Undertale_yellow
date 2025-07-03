using UnityEngine;

public class PortalGate : MonoBehaviour
{
    public int portalNumber; // Æ÷Å»ÀÇ ¹øÈ£

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PortalManager.Instance.OnPortalTeleport(portalNumber);
        }
    }
}
