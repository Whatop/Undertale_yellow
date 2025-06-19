using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuickTextBar : MonoBehaviour
{
    public int txtId = 0;                  // 사운드용 텍스트 ID
    public float charPerSec = 10f;         // 타이핑 속도
    public TypeEffect typeEffect;
    public Transform target; // 따라갈 대상 (플레이어)
    public Vector3 offset = new Vector3(0, 1.65f, 0); // 위치 오프셋
    public bool IsBusy { get; private set; } = false;

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

    public void ShowMessage(string msg)
    {
        gameObject.SetActive(true);
        IsBusy = true;

        typeEffect.CharPerSeconds = charPerSec;
        typeEffect.txtId = txtId;

        typeEffect.SetMsg(msg, () =>
        {
            gameObject.SetActive(false);
            IsBusy = false;
        });
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
