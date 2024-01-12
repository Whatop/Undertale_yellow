using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float smoth = 5.0f;

    public GameObject player;
    public bool isCameraMove = true;

    Vector3 target;

    private void OnEnable()
    {
        isCameraMove = true;
    }
    private void LateUpdate()
    {
        if(isCameraMove)
            Move();
    }
    void Move()
    {
        target = new Vector3(player.transform.localPosition.x , player.transform.localPosition.y, -10);
        transform.localPosition = target;
    }

}