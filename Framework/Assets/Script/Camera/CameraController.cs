using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private GameManager gameManager;
    private CinemachineBrain cinemachineBrain;

    public CinemachineVirtualCamera[] virtualCameras;
    public CinemachineVirtualCamera virtualBattleCamera;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>(); // 메인 카메라에 붙은 CinemachineBrain 컴포넌트 가져오기
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
            float targetCameraSize = gameManager.isBattle ? 6 : 6;

            // 카메라 크기가 너무 작게 변동하지 않도록 조정
            if (Mathf.Abs(activeVirtualCamera.m_Lens.OrthographicSize - targetCameraSize) > 0.01f)
            {
             activeVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(activeVirtualCamera.m_Lens.OrthographicSize, targetCameraSize, 8f * Time.deltaTime);
            }
        }
        else
        {
            Debug.LogWarning("활성화된 CinemachineVirtualCamera가 없습니다.");
        }
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
