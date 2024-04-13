using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 6f; // 조절할 확대 속도
    public float minZoom = 2f;   // 최소 크기
    public float maxZoom = 8f;  // 최대 크기

    void Update()
    {
        float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");

        // 마우스 휠 입력이 감지되면
        if (scrollWheelInput != 0)
        {
            // 카메라 확대/축소를 위한 새로운 카메라 크기 계산
            float newCameraSize = Camera.main.orthographicSize - scrollWheelInput * zoomSpeed;

            // 새로운 카메라 크기를 최소 크기와 최대 크기 사이로 제한
            newCameraSize = Mathf.Clamp(newCameraSize, minZoom, maxZoom);

            // 카메라 크기를 업데이트
            Camera.main.orthographicSize = newCameraSize;
        }
    }
}
