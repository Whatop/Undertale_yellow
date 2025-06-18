using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuickTextBar : MonoBehaviour
{
    public TypeEffect typeEffect;
    public Transform target; // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0, 1.65f, 0); // 위치 오프셋

    private void Awake()
    {
        if (typeEffect == null)
            typeEffect = GetComponentInChildren<TypeEffect>();
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    public void ShowMessage(string message, float duration = 1.5f)
    {
        gameObject.SetActive(true);
        typeEffect.SetMsg(message, () => StartCoroutine(HideAfterDelay(duration)));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
