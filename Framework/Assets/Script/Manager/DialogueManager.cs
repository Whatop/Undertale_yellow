using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DialogueManager : MonoBehaviour
{
    private string dialogueFolderPath = "Assets/Dialogues/"; // 대화 내용이 저장된 폴더 경로

    public void StartDialogue(int npcID)
    {
        string filePath = dialogueFolderPath + "NPC_" + npcID + ".txt"; // NPC의 대화 파일 경로
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                Debug.Log(line); // 대화 내용 출력
            }
        }
        else
        {
            Debug.LogWarning("NPC_" + npcID + ".txt 파일을 찾을 수 없습니다.");
        }
    }
}
