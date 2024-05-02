using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float speed = 1f; // 이동 속도
    public float destroyTime = 1.5f; // 텍스트가 사라지는 시간

    private TextMeshProUGUI damageText;
    private Vector3 moveDirection;

    private void Awake()
    {
        damageText = GetComponent<TextMeshProUGUI>();
    }

    public void Initialize(int damageAmount)
    {
        damageText.text = "-" + damageAmount;
        moveDirection = Vector3.up; // 텍스트 이동 방향 (위쪽으로)
        Destroy(gameObject, destroyTime); // 설정된 시간 후에 텍스트 제거
    }

    private void Update()
    {
        // 텍스트를 이동시킴
        transform.Translate(moveDirection * speed * Time.deltaTime);
    }
}
