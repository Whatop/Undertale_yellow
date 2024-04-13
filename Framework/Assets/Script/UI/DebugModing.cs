using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class DebugModing : MonoBehaviour
{
    //치트 `나중에 바꿔야할것`
    [SerializeField]
    bool isDebugMode = true;
    int debugMode = 0;
   

    void Update()
    {
        GetInputDebug();
    }

    private void GetInputDebug()
    {
        if (!isDebugMode)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            debugMode = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            debugMode = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            debugMode = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            debugMode = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            debugMode = 5;

        // 디버그 모드에 따른 처리
        switch (debugMode)
        {
            case 1:
                // 디버그 모드 1 인트로 화면
                SceneManager.LoadScene("IntroScene");
                debugMode = 0;
                break;
            case 2:
                // 디버그 모드 2 닉네임 화면
                SceneManager.LoadScene("ProduceScene");
                debugMode = 0;
                break;
            case 3:
                // 디버그 모드 3 게임 OverWolrd
                SceneManager.LoadScene("GameScene");
               
                debugMode = 0;
                break;
            case 4:
                // 디버그 모드 4 게임 InGame
                if (SceneManager.GetActiveScene().name == "GameScene")
                {
                    //UI_Item_Event_Manager.Instance.Init();
                    //UI_Item_Event_Manager.Instance.IngameStart();
                }

                debugMode = 0;
                break;
            case 5:
                // 디버그 모드 5 처리 
                
                debugMode = 0;
                break;
            default:
                // 디버그 모드가 설정되지 않은 경우 처리
                break;
        }
    }
}
