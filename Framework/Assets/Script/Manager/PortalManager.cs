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
    public int test = 0;
    private GameObject Player;
    public GameObject defaultPoint;

    public Image fadeImage;
    public float fadeDuration = 1.0f;
    private bool isFading = false;

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
        else
        {
           // OnPortalTeleport(test); // 꼭 없엥 
        }

    }

    public void OnPortalEnter(PortalGate portal)
    {
        // 이미 페이드 중이라면 더 이상 진입 이벤트를 처리하지 않음
        if (isFading)
            return;

        // 페이드 아웃 시작
        GameManager.Instance.ChangeGameState(GameState.Event);
        StartCoroutine(FadeOut());

        currentPortalPointIndex = portal.portalNumber;

        if (currentPortalPointIndex >= 0 && currentPortalPointIndex < portalPoints.Length)
        {
            // 페이드 아웃이 완료된 후에 플레이어 이동
            StartCoroutine(MovePlayerAfterFade(portalPoints[currentPortalPointIndex].transform.position));
        }
        else
        {
            // 범위를 벗어난 경우 기본 포인트로 이동
            StartCoroutine(MovePlayerAfterFade(defaultPoint.transform.position));
            currentPortalPointIndex = 0;
            Debug.Log("잘못된 이동");
        }
    }
    public void OnPortalTeleport(int point)
    {
        // 이미 페이드 중이라면 더 이상 진입 이벤트를 처리하지 않음
        if (isFading)
            return;

        // 페이드 아웃 시작
        GameManager.Instance.ChangeGameState(GameState.Event);
        StartCoroutine(FadeOut());

        if (currentPortalPointIndex < portalPoints.Length)
        {
            // 페이드 아웃이 완료된 후에 플레이어 이동
            StartCoroutine(MovePlayerAfterFade(portalPoints[currentPortalPointIndex].transform.position));
        }
        else
        {
            // 범위를 벗어난 경우 기본 포인트로 이동
            StartCoroutine(MovePlayerAfterFade(defaultPoint.transform.position));
            currentPortalPointIndex = 0;
        }
    }
    IEnumerator MovePlayerAfterFade(Vector3 targetPosition)
    {
        yield return new WaitForSeconds(fadeDuration);

        // 플레이어 이동
        Player.transform.position = targetPosition;

        // 페이드 인 시작
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        isFading = true;

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            fadeImage.color = color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isFading = false;
    }

    IEnumerator FadeIn()
    {
        isFading = true;

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        GameManager.Instance.ChangeGameState(GameState.None);

        isFading = false;
    }
}
