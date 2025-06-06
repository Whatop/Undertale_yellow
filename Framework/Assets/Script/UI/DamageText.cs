using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageText : MonoBehaviour
{
    public bool isObject;
    
    private float speed = 0.75f; // 이동 속도
    private float destroyTime = 1f; // 텍스트가 사라지는 시간
    private TextMeshProUGUI damageText;

    private Vector3 moveDirection;

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();

    }

    public void Initialize(float damageAmount)
    {
        if (!isObject)
        {
            damageText.text = "<fade d=0>" + damageAmount;
            moveDirection = Vector3.up; // 텍스트 이동 방향 (위쪽으로)
        }
        else
        {
            speed = 1.5f;
            damageText.text = "<fade d=0>" + damageAmount;
            moveDirection = Vector3.right; 
            moveDirection += Vector3.up;
        }
        Destroy(gameObject, destroyTime); // 설정된 시간 후에 텍스트 제거
    }

    private void Update()
    {

        if (!isObject)
        transform.Translate(moveDirection * speed * Time.deltaTime);
        else
        {
                moveDirection -= Vector3.up * Time.deltaTime;
            transform.localScale = new Vector3(moveDirection.y + 0.5f, moveDirection.y+ 0.5f, 1);
        transform.Translate(moveDirection * speed * Time.deltaTime);
        }
    }
}
