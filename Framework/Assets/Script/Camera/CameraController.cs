using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameManager gameManager;
    public float smooth = 8.0f;

    public GameObject player;
    public bool isCameraMove = true;

    public float defaultCameraSize = 6;
    public float battleCameraSize = 10;
    public Transform[] datum_point;

    Vector3 target;

    private void Awake()
    {
        gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        isCameraMove = true;
    }

    private void Update()
    {
        CameraSizeCheck();
    }

    private void LateUpdate()
    {
        if (isCameraMove)
            CameraMove();
    }

    void CameraMove()
    {
        if (player == null)
        {
            Debug.LogWarning("Player가 CameraController에 할당되지 않았습니다.");
            return;
        }

        Vector3 playerPos = player.transform.localPosition;

        switch (gameManager.cameraType)
        {
            case CameraType.Hor:
                target = new Vector3(playerPos.x, datum_point[gameManager.curportalNumber].transform.localPosition.y, -10);
                break;
            case CameraType.Ver:
                target = new Vector3(datum_point[gameManager.curportalNumber].transform.localPosition.x, playerPos.y, -10);
                break;
            case CameraType.All:
                target = new Vector3(playerPos.x, playerPos.y, -10);
                break;
        }

        transform.localPosition = target;
    }

    void CameraSizeCheck()
    {
        float targetCameraSize = gameManager.isBattle ? battleCameraSize : defaultCameraSize;

        if (Camera.main.orthographicSize != targetCameraSize)
        {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetCameraSize, smooth * Time.deltaTime);
        }
    }
}
