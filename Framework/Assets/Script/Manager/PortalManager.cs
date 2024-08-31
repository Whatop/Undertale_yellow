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
        // 시간 정지 및 플레이어 애니메이터 비활성화
        Time.timeScale = 0f;
        playerMovement.SetAnimatorEnabled(false);

        // 페이드 아웃
        yield return StartCoroutine(Fade(1f));

        currentPortalPointIndex = point;
        // 플레이어 이동
        if (currentPortalPointIndex >= 0 && currentPortalPointIndex < portalPoints.Length)
        {
            Player.transform.position = portalPoints[currentPortalPointIndex].transform.position;
        }
        else
        {
            Player.transform.position = defaultPoint.transform.position;
            currentPortalPointIndex = 0;
            SwitchCamera(-1);
            Debug.Log("잘못된 텔레포트 지점입니다. 기본 지점으로 이동합니다.");
        }

        // 카메라 이동
        SwitchCamera(point);
        // 페이드 인
        yield return StartCoroutine(Fade(0f));

        gameManager.ChangeGameState(GameState.None);

        // 시간 재개 및 플레이어 애니메이터 활성화
        Time.timeScale = 1f;
        playerMovement.SetAnimatorEnabled(true);
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
        if (point >= 0 && point < virtualCameras.Length)
        {
            virtualCameras[point].transform.position = portalPoints[point].transform.position;
            virtualCameras[point].gameObject.SetActive(true);
        }
        else
        {
            defaultvirtualCamera.transform.position = defaultPoint.transform.position;
            defaultvirtualCamera.gameObject.SetActive(true);
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
