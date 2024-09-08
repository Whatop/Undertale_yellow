using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private GameManager gameManager;

    // 여러 가상 카메라를 배열로 관리하지만, 직접 제어하지 않음
    public CinemachineVirtualCamera[] virtualCameras;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        UpdateCameraSize();
    }

    void UpdateCameraSize()
    {
        foreach (var virtualCamera in virtualCameras)
        {
            if (virtualCamera == null)
            {
                Debug.LogWarning("CinemachineVirtualCamera가 CameraController에 할당되지 않았습니다.");
                continue;
            }

            // 2D 카메라의 OrthographicSize를 조정
            float targetCameraSize = gameManager.isBattle ? 10 : 6; // 전투 중일 때와 기본 상태의 카메라 크기
            if (Mathf.Abs(virtualCamera.m_Lens.OrthographicSize - targetCameraSize) > 0.01f )
            {
                
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetCameraSize, 8f * Time.deltaTime);
            }
        }
    }
}
