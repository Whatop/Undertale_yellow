using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TypeEffect : MonoBehaviour
{
    public float CharPerSeconds = 10f; // 초당 문자 수
    private string targetMsg;
    public TextMeshProUGUI msgText;
    private float interval;
    private int index;
    public int txtId = 0;
    private System.Action onEffectEndCallback;
    private string txtsound = "SND_TXT1";

    private void Awake()
    {
        msgText = GetComponent<TextMeshProUGUI>();
    }

    public void SetMsg(string msg, System.Action onEffectEnd)
    {
        targetMsg = msg;
        onEffectEndCallback = onEffectEnd;
        EffectStart();
    }
    public void SetMsg(string msg, System.Action onEffectEnd, int eventNumber)
    {
        targetMsg = msg;
        onEffectEndCallback = onEffectEnd;
        switch (eventNumber)
        {
            case 100:
                txtsound = "voice_flowey_1";
                txtId = 17;
                break;
                

        }
        EffectStart();
    }
    public void Skip()
    {
        msgText.text = targetMsg;
        EffectEnd();
    }

    private void EffectStart()
    {
        msgText.text = "";
        index = 0;
        interval = 1.0f / CharPerSeconds;
        Invoke("Effecting", interval);
    }

    private void Effecting()
    {
        if (msgText.text == targetMsg)
        {
            EffectEnd();
            return;
        }

        if (index < targetMsg.Length && targetMsg[index].ToString() != " " && targetMsg[index].ToString() != "?" && targetMsg[index].ToString() != "." && targetMsg[index].ToString() != "*")
        {
            SoundManager.Instance.SFXTextPlay(txtsound, txtId);
        }

        if (index < targetMsg.Length)
        {
            msgText.text += targetMsg[index];
            index++;
            Invoke("Effecting", targetMsg[index - 1].ToString() == " " ? 0.02f : 1f / CharPerSeconds);
        }
    }

    private void EffectEnd()
    {
        onEffectEndCallback?.Invoke();
    }
}
