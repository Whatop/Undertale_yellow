using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; // Cinemachine 네임스페이스 추가
using UnityEngine.UI; // Cinemachine 네임스페이스 추가

[System.Serializable]
public class PortalData
{
    public int portalNumber;
    public GameObject portalPoint;
    public CinemachineVirtualCamera virtualCamera;
}



public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    public GameObject[] portalPoints;
    private int currentPortalPointIndex = 0;
    public int lastPortalNumber = -1; // 초기값은 기본 상태를 의미 (-1)

    private GameObject Player;
    private PlayerMovement playerMovement;
    public GameObject defaultPoint;
    public CinemachineVirtualCamera defaultvirtualCamera;  // 각 포탈 지점에 대응하는 가상 카메라 배열

    public Image fadeImage;
    public float fadeDuration = 1.0f;
    private bool isFading = false;

    private GameManager gameManager;

    public CinemachineVirtualCamera[] virtualCameras;  // 각 포탈 지점에 대응하는 가상 카메라 배열
    public Camera mainsCamera;
    // 인스펙터에서 설정
    public List<PortalData> portalDataList;

    public GameObject[] Rooms;

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
        gameManager = GameManager.Instance;
    }

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = Player.GetComponent<PlayerMovement>();

        if (portalPoints.Length == 0)
        {
            Debug.LogError("포탈 지점이 설정되지 않았습니다. 인스펙터에서 포탈 지점을 설정해주세요.");
        }
        //SwitchCamera();
    }
    public void HandlePortal(int point)
    {
        if (isFading)
            return;

        StartCoroutine(FadeAndMove(point));
    }

    public void OnPortalEnter(PortalGate portal)
    {
        HandlePortal(portal.portalNumber);
    }

    public void OnPortalTeleport(int point)
    {
        gameManager.ChangeGameState(GameState.Event);
        HandlePortal(point);
    }

    IEnumerator FadeAndMove(int point)
    {
        isFading = true;
        gameManager.isPortalTransition = true;

        // 플레이어 이동과 관련된 모든 입력 차단
        playerMovement.enabled = false; // 입력을 비활성화
        playerMovement.SetAnimatorEnabled(false); // 애니메이터 비활성화

        // 시간 정지
        Time.timeScale = 0f;

        // 페이드 아웃
        yield return StartCoroutine(Fade(1f));

        // 플레이어 이동
        currentPortalPointIndex = point;
        if (currentPortalPointIndex >= 0 && currentPortalPointIndex < portalPoints.Length)
        {
            Player.transform.position = portalPoints[currentPortalPointIndex].transform.position;
            SwitchCamera(point);
        }
        else if (point == 999)
        {
            Player.transform.position = portalPoints[5].transform.position;
            SwitchCamera(point);
        }
        else
        {
            Player.transform.position = defaultPoint.transform.position;
            currentPortalPointIndex = 0;
            SwitchCamera(-1);
            Debug.Log("잘못된 텔레포트 지점입니다. 기본 지점으로 이동합니다.");
        }

        // 페이드 인
        yield return StartCoroutine(Fade(0f));

        // 상태 초기화
        gameManager.ChangeGameState(GameState.None);

        // 시간 재개
        Time.timeScale = 1f;

        // 플레이어 입력 다시 활성화
        playerMovement.enabled = true; // 입력 다시 활성화
        playerMovement.SetAnimatorEnabled(true); // 애니메이터 다시 활성화
        gameManager.isPortalTransition = false;

        isFading = false;
    }

    void SwitchCamera(int point)
    {
        lastPortalNumber = point; // 현재 활성화된 포탈 번호 저장

        foreach (var data in portalDataList)
        {
            data.virtualCamera.gameObject.SetActive(data.portalNumber == point);
        }
        defaultvirtualCamera.gameObject.SetActive(!portalDataList.Exists(data => data.portalNumber == point));

        Debug.Log(lastPortalNumber + " : 번호로 이동함");
    
    }




    IEnumerator Fade(float targetAlpha)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        float startAlpha = fadeImage.color.a;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            fadeImage.color = color;

            elapsedTime += isFading ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
    }
}
