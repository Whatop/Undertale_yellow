using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    [Header("Portal Settings")]
    [SerializeField] private GameObject[] portalPoints;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private GameManager gameManager;
    private GameObject player;
    private PlayerMovement playerMovement;
    private bool isFading = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    /// <summary>
    /// 외부에서 호출: 해당 포탈로 순간이동
    /// </summary>
    public void OnPortalTeleport(int portalIndex)
    {
        if (!isFading)
            StartCoroutine(FadeAndTeleport(portalIndex));
    }

    private IEnumerator FadeAndTeleport(int portalIndex)
    {
        isFading = true;
        gameManager.isPortalTransition = true;

        playerMovement.enabled = false;
        playerMovement.SetAnimatorEnabled(false);
        Time.timeScale = 0f;

        yield return StartCoroutine(Fade(1f));

        // 이동 처리
        if (portalIndex >= 0 && portalIndex < portalPoints.Length)
        {
            player.transform.position = portalPoints[portalIndex].transform.position;
            CameraController.Instance.SwitchRoomConfiner(portalIndex);
        }
        else if(portalIndex==999)
        {
            player.transform.position = portalPoints[portalPoints.Length-1].transform.position;
            CameraController.Instance.SwitchRoomConfiner(portalIndex);
        }
        else
        {
            Debug.LogWarning($"PortalManager: 잘못된 포탈 인덱스 {portalIndex}");
        }

        yield return StartCoroutine(Fade(0f));

        gameManager.ChangeGameState(GameState.None);
        Time.timeScale = 1f;
        playerMovement.enabled = true;
        playerMovement.SetAnimatorEnabled(true);
        gameManager.isPortalTransition = false;
        isFading = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        float startAlpha = c.a;

        while (elapsed < fadeDuration)
        {
            elapsed += isFading ? Time.unscaledDeltaTime : Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        fadeImage.color = c;
    }
    public void LoadLastPortal()
    {
        int last = PlayerPrefs.GetInt("LastPortalNumber", 0);
        // 위치 복원
        player.transform.position = portalPoints[last].transform.position;
        // 카메라 경계 복원
        CameraController.Instance.SwitchRoomConfiner(last);
        // (페이드+이동까지 원하면 OnPortalTeleport(last) 호출)
    }

}
