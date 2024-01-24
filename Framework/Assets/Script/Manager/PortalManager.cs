using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance; // 싱글톤 인스턴스

    public GameObject[] portalPoints; // 포탈 이동포인트 GameObject 배열
    private int currentPortalPointIndex = 0; // 현재 포탈 포인트의 인덱스
    private GameObject Player; // 포탈 포인트 GameObject 배열
    public GameObject defaultPoint;


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

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        if (portalPoints.Length == 0)
        {
            Debug.LogError("Portal points are not assigned. Please assign portal points in the inspector.");
        }
    }

    public void OnPortalEnter(PortalGate portal)
    {
        // 포탈 번호에 따라 이동 방향 결정
        int direction = (portal.portalNumber % 2 == 0) ? 1 : -1;

        // 다음 포인트 인덱스 계산
        currentPortalPointIndex += direction;
        Debug.Log(currentPortalPointIndex + "번 포탈로 이동");
        // 포인트 배열 범위 체크
        if (currentPortalPointIndex >= 0 && currentPortalPointIndex < portalPoints.Length)
        {
            // 플레이어를 해당 포인트로 이동
            Player.transform.position = portalPoints[currentPortalPointIndex].transform.position;
        }
        else
        {
            // 범위를 벗어난 경우 기본 포인트로 이동
            Player.transform.position = defaultPoint.transform.position;
            currentPortalPointIndex = 0;
        }
    }
}
