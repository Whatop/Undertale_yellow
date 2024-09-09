using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; // Cinemachine 네임스페이스 추가
using UnityEngine.UI; // Cinemachine 네임스페이스 추가

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    public GameObject[] portalPoints;
    private int currentPortalPointIndex = 0;
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
        SwitchCamera(-1);
    }

    public void OnPortalEnter(PortalGate portal)
    {
        if (isFading)
            return;

        StartCoroutine(FadeAndMove(portal.portalNumber));
    }

    public void OnPortalTeleport(int point)
    {
        if (isFading)
            return;

        gameManager.ChangeGameState(GameState.Event);
        StartCoroutine(FadeAndMove(point));
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
        // 모든 가상 카메라 비활성화
        foreach (var cam in virtualCameras)
        {
            cam.gameObject.SetActive(false);
            defaultvirtualCamera.gameObject.SetActive(false);
        }

        // 포인트에 해당하는 가상 카메라 활성화
        switch (point)
        {
            case 0:
                virtualCameras[1].transform.position = portalPoints[1].transform.position;
                mainsCamera.transform.position = virtualCameras[1].transform.position;
                virtualCameras[1].gameObject.SetActive(true);
                break;

            case 1:
                virtualCameras[0].transform.position = portalPoints[0].transform.position;
                mainsCamera.transform.position = virtualCameras[0].transform.position;
                virtualCameras[0].gameObject.SetActive(true);
                break;

            case 2:
                virtualCameras[2].transform.position = portalPoints[2].transform.position;
                mainsCamera.transform.position = virtualCameras[2].transform.position;
                virtualCameras[2].gameObject.SetActive(true);
                break;

            case 3:
                virtualCameras[1].transform.position = portalPoints[2].transform.position;
                mainsCamera.transform.position = virtualCameras[2].transform.position;
                virtualCameras[1].gameObject.SetActive(true);
                break;

            case 4:
                virtualCameras[3].transform.position = portalPoints[2].transform.position;
                mainsCamera.transform.position = virtualCameras[2].transform.position;
                virtualCameras[3].gameObject.SetActive(true);
                break;
            default:
                defaultvirtualCamera.transform.position = defaultPoint.transform.position;
                mainsCamera.transform.position = defaultvirtualCamera.transform.position;
                defaultvirtualCamera.gameObject.SetActive(true);
                break;
        }

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

            elapsedTime += Time.unscaledDeltaTime; // 시간 정지 상태에서 페이드가 올바르게 진행되도록 unscaledDeltaTime 사용
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
    }
}
