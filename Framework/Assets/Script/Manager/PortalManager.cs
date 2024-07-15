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
    }

    public void OnPortalEnter(PortalGate portal)
    {
        if (isFading)
            return;

        GameManager.Instance.ChangeGameState(GameState.Event);
        StartCoroutine(FadeAndMove(portal.portalNumber));
    }

    public void OnPortalTeleport(int point)
    {
        if (isFading)
            return;

        GameManager.Instance.ChangeGameState(GameState.Event);
        StartCoroutine(FadeAndMove(point));
    }

    IEnumerator FadeAndMove(int point)
    {
        isFading = true;

        // 페이드 아웃
        yield return Fade(1f);

        // 플레이어 이동
        if (point >= 0 && point < portalPoints.Length)
        {
            Player.transform.position = portalPoints[point].transform.position;
        }
        else
        {
            Player.transform.position = defaultPoint.transform.position;
            currentPortalPointIndex = 0;
            Debug.Log("Invalid teleport point. Moving to default point.");
        }

        // 페이드 인
        yield return Fade(0f);

        GameManager.Instance.ChangeGameState(GameState.None);
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

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;
    }
}
