using UnityEngine;

public class PortalGate : MonoBehaviour
{
    public int portalNumber; // 포탈의 번호

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PortalManager.Instance.OnPortalEnter(this); // 포탈 진입 이벤트 호출
        }
    }
}
