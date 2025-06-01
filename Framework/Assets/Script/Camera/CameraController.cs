using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachinePixelPerfect))]
public class CameraController : MonoBehaviour
{
    private GameManager gameManager;
    private CinemachineBrain cinemachineBrain;
    private CinemachinePixelPerfect pixelPerfectCamera; // Pixel Perfect Camera

    public CinemachineVirtualCamera[] virtualCameras;
    public CinemachineVirtualCamera virtualBattleCamera;

    public float shakeAmount = 0.1f;  // 흔들림 강도
    public float shakeDuration = 0.5f;  // 흔들림 지속 시간
    private float shakeTimer = 0f;  // 흔들림 타이머
    private Vector3 originalPosition;  // 원래 카메라 위치

    private static CameraController instance;

    // 싱글톤 패턴을 위한 프로퍼티
    public static CameraController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CameraController>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("CameraController");
                    instance = obj.AddComponent<CameraController>();
                }
            }
            return instance;
        }
    }
    private void Awake()
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>(); // 메인 카메라에 붙은 CinemachineBrain 가져오기
        pixelPerfectCamera = GetComponent<CinemachinePixelPerfect>(); // CinemachinePixelPerfect 컴포넌트 가져오기
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
 
    }
    void Update()
    {
        if (shakeTimer > 0)
        {
            // 흔들림이 남아 있을 경우
            transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            // 흔들림이 끝났으면 원래 위치로 돌아가도록
            transform.position = originalPosition;
        }

        UpdateCameraSize();
    }


    void UpdateCameraSize()
    {
        // 현재 활성화된 가상 카메라 가져오기
        CinemachineVirtualCamera activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;

        if (activeVirtualCamera != null)
        {
            // 배틀 여부에 따라 Pixel Perfect 활성화 여부를 설정
            if (pixelPerfectCamera != null)
            {
                pixelPerfectCamera.enabled = gameManager.isBattle; // 배틀 중에만 Pixel Perfect 활성화
            }

            // Pixel Perfect가 비활성화된 경우에만 카메라 크기 조정
            if (pixelPerfectCamera == null || !pixelPerfectCamera.enabled)
            {
                float targetCameraSize = gameManager.isBattle ? 6 : 6; // 필요한 경우 크기를 조정
                if (Mathf.Abs(activeVirtualCamera.m_Lens.OrthographicSize - targetCameraSize) > 0.01f)
                {
                    activeVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                        activeVirtualCamera.m_Lens.OrthographicSize,
                        targetCameraSize,
                        8f * Time.deltaTime
                    );
                }
            }
        }
        else
        {
            Debug.LogWarning("활성화된 CinemachineVirtualCamera가 없습니다.");
        }

        // 카메라 우선순위 설정
        if (gameManager.isBattle)
        {
            virtualBattleCamera.Priority = 11;
        }
        else
        {
            virtualBattleCamera.Priority = 6;
        }
    }
    public void ShakeCamera()
    {
        originalPosition = transform.position;  // 원래 위치 저장
        shakeTimer = shakeDuration;  // 흔들림 지속 시간 설정
    }

}
