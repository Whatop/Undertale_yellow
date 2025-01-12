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

    private void Awake()
    {
        gameManager = GameManager.Instance;
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>(); // 메인 카메라에 붙은 CinemachineBrain 가져오기
        pixelPerfectCamera = GetComponent<CinemachinePixelPerfect>(); // CinemachinePixelPerfect 컴포넌트 가져오기
    }

    private void Update()
    {
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

}
