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
    public Coroutine typingCoroutine;
    // 기존 코드 유지
    private string currentExpression; // 현재 표정 정보 저장

    public bool isTextSfxEnable = true; // 텍스트 SFX on/off 제어
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
                txtsound = "SND_sASR";
                txtId = 3;
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
    public void SetMsg(string msg, System.Action onEffectEnd, int eventNumber, string expression = null)
    {
        targetMsg = msg;
        onEffectEndCallback = onEffectEnd;
        currentExpression = expression; // 현재 표정 정보를 저장

       
        switch (eventNumber)
        {
            case 0:
                txtsound = "SND_sASR";
                txtId = 3;
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
    public void Clear()
    {
            msgText.text = "";
    }
    public bool IsEffecting()
    {
        return typingCoroutine != null; // `Effecting` 코루틴이 실행 중이면 true 반환
    }

    public void Skip()
    {
        if (IsEffecting()) // 코루틴이 진행 중일 때만 스킵 허용
        {
            StopCoroutine(typingCoroutine);
            msgText.text = targetMsg;
            EffectEnd();
            // Skip 시에도 대화 종료 후 상호작용을 딜레이합니다.
            DialogueManager.Instance.currentNPC?.StartCoroutine(DialogueManager.Instance.currentNPC.InteractionDelay());

        }
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
        bool isExpressionSet = false;
        string currentText = "";

        while (index < targetMsg.Length)
        {
            if (!isExpressionSet && (index == 0 || targetMsg[index - 1] == ' '))
            {
                if (currentExpression != null && DialogueManager.Instance.currentNPC != null)
                {
                    DialogueManager.Instance.currentNPC.SetExpression(currentExpression);
                    isExpressionSet = true;
                }
            }

            if (targetMsg[index] == '<')
            {
                int closeIndex = targetMsg.IndexOf('>', index);
                if (closeIndex != -1)
                {
                    string tag = targetMsg.Substring(index, closeIndex - index + 1);
                    currentText += tag;
                    msgText.text = currentText;
                    index = closeIndex + 1;
                    continue;
                }
            }

            char currentChar = targetMsg[index];

            // 사운드 재생
            if ( currentChar != ' ' && currentChar != '?' && currentChar != '.' && currentChar != '*')
            {
                if(isTextSfxEnable)
                    SoundManager.Instance.SFXTextPlay(txtsound, txtId);
                else
                    SoundManager.Instance.SFXErrorTextPlay(txtsound, txtId);
                

            }
            

            currentText += currentChar;
            msgText.text = currentText;
            index++;

            // 줄넘김 문자일 경우 약간의 추가 딜레이
            if (currentChar == '\n')
            {
                yield return new WaitForSeconds(0.2f); // 줄넘김 딜레이 추가
            }
            else
            {
                yield return new WaitForSeconds(1f / CharPerSeconds);
            }
        }

        EffectEnd();
    }



    private void EffectEnd()
    {
        typingCoroutine = null; // 코루틴이 종료되었음을 명확히 설정
        onEffectEndCallback?.Invoke();
        if (DialogueManager.Instance.currentNPC != null)
        {
            DialogueManager.Instance.currentNPC.SetExpression("Default");
        }
    }
    private void OnDisable()
    {
        if (msgText != null)
        {
            msgText.text = "";
        }
    }
}
