using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance; // 싱글톤 인스턴스

    private GameObject[] portals; // 포탈 포인트 GameObject 배열
    public GameObject[] portalPoints; // 포탈 포인트 GameObject 배열
    private int currentPortalPointIndex = 0; // 현재 포탈 포인트의 인덱스
    public GameObject Player; // 포탈 포인트 GameObject 배열

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
        portals = GameObject.FindGameObjectsWithTag("Portal");
        Player = GameObject.FindGameObjectWithTag("Player");

        if (portalPoints.Length == 0)
        {
            Debug.LogError("Portal points are not assigned. Please assign portal points in the inspector.");
        }
    }

    public void OnPortalEnter(PortalPoint portal)
    {
        Debug.Log("작동은 되는감?");

        // 현재 포탈의 번호와 비교하여 다음 포인트로 이동
        int currentPortalNumber = portals[currentPortalPointIndex].GetComponent<PortalPoint>().portalNumber;

        // 다음 포인트로 이동 (순환)
        currentPortalPointIndex = (currentPortalPointIndex + 1) % portals.Length;

        // 플레이어를 해당 포인트로 이동시키거나 다른 동작 수행
        int nextPortalNumber = portals[currentPortalPointIndex].GetComponent<PortalPoint>().portalNumber;

        if (nextPortalNumber % 2 == 0)
        {
            // 짝수인 경우 앞으로 이동 로직 추가
            // 예시: MovePlayerForward();
            Player.transform.position = portalPoints[nextPortalNumber + 1].transform.position;
        }
        else
        {
            Player.transform.position = portalPoints[nextPortalNumber - 1].transform.position;
            // 홀수인 경우 뒤로 이동 로직 추가
            // 예시: MovePlayerBackward();
        }
    }
}
