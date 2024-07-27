using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    public GameObject[] portalPoints;
    [SerializeField]
    private int currentPortalPointIndex = 0;
    private GameObject Player;
    private PlayerMovement playerMovement;
    public GameObject defaultPoint;

    public Image fadeImage;
    public float fadeDuration = 1.0f;
    private bool isFading = false;

    private GameManager gameManager;
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

        gameManager.ChangeGameState(GameState.Event);
        switch (portal.portalNumber)
        {
            case 0:
                gameManager.ChangeCameraState(CameraType.All);
                break;
            case 1:
                gameManager.ChangeCameraState(CameraType.Hor, portal.portalNumber);
                break;
           // case 2:
           //     gameManager.ChangeCameraState(CameraType.All);
           //     break;
           // case 3:
           //     gameManager.ChangeCameraState(CameraType.All);
           //     break;
        }
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

        // 시간 정지 및 플레이어 애니메이터 비활성화
        Time.timeScale = 0f;
        playerMovement.SetAnimatorEnabled(false);

        // 페이드 아웃
        yield return StartCoroutine(Fade(1f));

        // 플레이어 이동
        if (point >= 0 && point < portalPoints.Length)
        {
            Player.transform.position = portalPoints[point].transform.position;
        }
        else
        {
            Player.transform.position = defaultPoint.transform.position;
            currentPortalPointIndex = 0;
            Debug.Log("잘못된 텔레포트 지점입니다. 기본 지점으로 이동합니다.");
        }

        // 페이드 인
        yield return StartCoroutine(Fade(0f));

        // 시간 재개 및 플레이어 애니메이터 활성화
        Time.timeScale = 1f;
        playerMovement.SetAnimatorEnabled(true);

        gameManager.ChangeGameState(GameState.None);
        isFading = false;
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
