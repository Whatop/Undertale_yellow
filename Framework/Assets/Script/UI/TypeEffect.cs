using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeEffect : MonoBehaviour
{
    public float CharPerSeconds = 10f; // 초당 문자 수
    private string targetMsg;
    public TextMeshProUGUI msgText;
    private int index;
    public int txtId = 0;
    private System.Action onEffectEndCallback;
    private string txtsound = "SND_TXT1";
    private Coroutine typingCoroutine;

    private void Awake()
    {
        msgText = GetComponent<TextMeshProUGUI>();
    }

    public void SetMsg(string msg, System.Action onEffectEnd)
    {
        targetMsg = msg;
        onEffectEndCallback = onEffectEnd;
        StartEffect();
    }

    public void SetMsg(string msg, System.Action onEffectEnd, int eventNumber)
    {
        targetMsg = msg;
        onEffectEndCallback = onEffectEnd;
        switch (eventNumber)
        {
            case 0:
                txtsound = "SND_TXT1";
                txtId = 1;
                break;
            case 100:
                txtsound = "voice_flowey_1";
                txtId = 17;
                break;
            default:
                txtsound = "SND_TXT1";
                txtId = 0;
                break;
        }
        StartEffect();
    }

    public void Skip()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        msgText.text = targetMsg;
        EffectEnd();
    }

    private void StartEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        msgText.text = "";
        index = 0;
        typingCoroutine = StartCoroutine(Effecting());
    }

    private IEnumerator Effecting()
    {
        while (index < targetMsg.Length)
        {
            // 태그 시작 감지 및 처리
            if (targetMsg[index] == '<')
            {
                int closeIndex = targetMsg.IndexOf('>', index);
                if (closeIndex != -1)
                {
                    string tag = targetMsg.Substring(index, closeIndex - index + 1);
                    msgText.text += tag;
                    index = closeIndex + 1;
                }
            }
            else
            {
                // 개별 문자 출력
                if (targetMsg[index].ToString() != " " && targetMsg[index].ToString() != "?" &&
                    targetMsg[index].ToString() != "." && targetMsg[index].ToString() != "*")
                {
                    SoundManager.Instance.SFXTextPlay(txtsound, txtId);
                }

                msgText.text += targetMsg[index];
                index++;
            }

            // 지연 시간 적용
            float delay = (index < targetMsg.Length && targetMsg[index - 1].ToString() == " ") ? 0.02f : 1f / CharPerSeconds;
            yield return new WaitForSeconds(delay);
        }

        EffectEnd();
    }

    private void EffectEnd()
    {
        onEffectEndCallback?.Invoke();
    }
}
