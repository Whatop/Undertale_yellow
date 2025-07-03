using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachinePixelPerfect))]
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera battleCamera;

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
    private CinemachinePixelPerfect pixelPerfectCamera;
    private float shakeTimer;
    private Vector3 originalPosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        pixelPerfectCamera = GetComponent<CinemachinePixelPerfect>();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

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

        // Pixel Perfect 활성/비활성
        if (pixelPerfectCamera != null)
            pixelPerfectCamera.enabled = isBattle;

        // Orthographic Size 보간
        var activeCam = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
        if (activeCam != null && (pixelPerfectCamera == null || !pixelPerfectCamera.enabled))
        {
            float targetSize = isBattle ? 6f : 6f; // 필요 시 값 조정
            if (Mathf.Abs(activeCam.m_Lens.OrthographicSize - targetSize) > 0.01f)
            {
                activeCam.m_Lens.OrthographicSize = Mathf.Lerp(
                    activeCam.m_Lens.OrthographicSize,
                    targetSize,
                    8f * Time.deltaTime
                );
            }
        }

        // 배틀 카메라 우선순위 전환
        if (battleCamera != null && mainCamera != null)
            battleCamera.Priority = isBattle ? mainCamera.Priority + 1 : mainCamera.Priority - 1;
    }

    /// <summary>
    /// 방 전환 시 Confiner 교체
    /// </summary>
    public void SwitchRoomConfiner(int roomIndex)
    {
        if (confiner == null || roomIndex < 0 || roomIndex >= roomBounds.Count)
        {
            Debug.LogWarning($"SwitchRoomConfiner: 잘못된 인덱스 {roomIndex}");
            return;
        }

        // 변경: CinemachineConfiner 사용법은 동일하게 BoundingShape에 Collider2D 할당
        confiner.m_BoundingShape2D = roomBounds[roomIndex];
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
