using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageScript : MonoBehaviour
{
    private Image image;
    public Sprite[] sprites; // 프로젝트에 있는 스프라이트들을 저장할 배열


    public void SetImage(int index)
    {
        if (image == null)
        image = GetComponent<Image>();

        // 유효한 인덱스 범위 내에 있는지 확인합니다.
        if (index >= 0 && index < sprites.Length)
            {
                // 해당 인덱스에 해당하는 스프라이트로 이미지를 변경합니다.
                image.sprite = sprites[index];
            }
            else
            {
                Debug.LogError("Index out of range.");
            }
    }
}
