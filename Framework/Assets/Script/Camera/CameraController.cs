using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Experimental.Rendering.Universal;  // ← 네임스페이스

[RequireComponent(typeof(PixelPerfectCamera))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera battleCamera;
    [SerializeField] private CinemachineVirtualCamera eventCamera;

    [Header("Confiner Settings")]
    // 변경: CinemachineConfiner2D → CinemachineConfiner
    [SerializeField] private CinemachineConfiner confiner;
    [SerializeField] private List<PolygonCollider2D> roomBounds;
    [SerializeField] private Transform player;

    [Header("Shake Settings")]
    public float shakeAmount = 0.1f;
    public float shakeDuration = 0.5f;

    private GameManager gameManager;
    private CinemachineBrain cinemachineBrain;
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;
    private float shakeTimer;
    private Vector3 originalPosition;

    private const int MAIN_CAM_PRIO = 10;
    private const int BATTLE_CAM_ACTIVE = 15;
    private const int BATTLE_CAM_INACTIVE = 5;
    private const int EVENTE_CAM_ACTIVE = 999;
    private const int EVENT_CAM_INACTIVE = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        // 초기 우선순위 설정
        mainCamera.Priority = MAIN_CAM_PRIO;
        battleCamera.Priority = BATTLE_CAM_INACTIVE;
        eventCamera.Priority = EVENT_CAM_INACTIVE;

        if (mainCamera != null && player != null)
            mainCamera.Follow = player;
    }

    private void Update()
    {
        HandleShake();
        UpdateCameraSettings();
    }

    private void HandleShake()
    {
        if (shakeTimer > 0f)
        {
            transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
            shakeTimer -= Time.unscaledDeltaTime;
        }
        else
        {
            transform.position = originalPosition;
        }
    }

    private void UpdateCameraSettings()
    {
        bool isBattle = gameManager.isBattle;
        battleCamera.Priority = isBattle
            ? BATTLE_CAM_ACTIVE
            : BATTLE_CAM_INACTIVE;


    }

    /// <summary>
    /// 방 전환 시 Confiner 교체
    /// </summary>
    public void SwitchRoomConfiner(int roomIndex)
    {
        if ((confiner == null || roomIndex < 0 || roomIndex >= roomBounds.Count)&&roomIndex !=999)
        {
            Debug.LogWarning($"SwitchRoomConfiner: 잘못된 인덱스 {roomIndex}");
            return;
        }
        Debug.Log($"SwitchRoomConfiner: 인덱스 {roomIndex}");

        switch (roomIndex)
        {
            //짝수 앞, 홀수 뒤
            case 1: // 방0: 가장 처음 방
                pixelPerfectCamera.refResolutionX = 320;
                pixelPerfectCamera.refResolutionY = 180;
                mainCamera.m_Lens.OrthographicSize = 6f;
                confiner.m_BoundingShape2D = roomBounds[0];
                break;

            case 0: // 방1: 첫 위쪽 방
            case 3:
                pixelPerfectCamera.refResolutionX = 160;
                pixelPerfectCamera.refResolutionY = 90;
                mainCamera.m_Lens.OrthographicSize = 4.5f;
                confiner.m_BoundingShape2D = roomBounds[1];
                break;

            case 2: // 방2: 2번째 위쪽 방
            case 5:
                pixelPerfectCamera.refResolutionX = 80;
                pixelPerfectCamera.refResolutionY = 45;
                mainCamera.m_Lens.OrthographicSize = 3f;
                confiner.m_BoundingShape2D = roomBounds[2];
                break;

            case 4: // 방3: 보스방 전
                pixelPerfectCamera.refResolutionX = 160;
                pixelPerfectCamera.refResolutionY = 90;
                mainCamera.m_Lens.OrthographicSize = 7f;
                confiner.m_BoundingShape2D = roomBounds[3];
                break;
            case 999: // 보스방 : 전투
                pixelPerfectCamera.refResolutionX = 320;
                pixelPerfectCamera.refResolutionY = 180;
                mainCamera.m_Lens.OrthographicSize = 6f;
                confiner.m_BoundingShape2D = roomBounds[roomBounds.Count-1];
                break;

            default:
                Debug.LogWarning($"Unknown portalID: {roomIndex}");
                break;
        }
        confiner.InvalidatePathCache();
    }

    /// <summary>
    /// 카메라 흔들기 트리거
    /// </summary>
    public void ShakeCamera(float duration = -1f)
    {
        originalPosition = transform.position;
        shakeTimer = duration > 0f ? duration : shakeDuration;
    }
}
