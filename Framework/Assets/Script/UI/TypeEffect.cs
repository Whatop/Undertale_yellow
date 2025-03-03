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
        bool isExpressionSet = false; // 표정 설정 여부를 추적
        string currentText = "";      // 현재까지 출력된 텍스트

        while (index < targetMsg.Length)
        {
            // 새로운 단어나 문장이 시작될 때 표정 설정
            if (!isExpressionSet && (index == 0 || targetMsg[index - 1] == ' '))
            {
                if (currentExpression != null && DialogueManager.Instance.currentNPC != null)
                {
                    DialogueManager.Instance.currentNPC.SetExpression(currentExpression);
                    isExpressionSet = true;
                }
            }

            // 태그 시작 감지 및 처리
            if (targetMsg[index] == '<')
            {
                int closeIndex = targetMsg.IndexOf('>', index);
                if (closeIndex != -1)
                {
                    // 태그를 전체적으로 처리
                    string tag = targetMsg.Substring(index, closeIndex - index + 1);
                    currentText += tag; // 태그를 유지하며 추가
                    msgText.text = currentText; // 텍스트 업데이트
                    index = closeIndex + 1;
                    continue; // 태그는 한 번에 처리되므로 다음 루프로 넘어감
                }
            }

            // 일반 문자 출력
            if (targetMsg[index].ToString() != " " && targetMsg[index].ToString() != "?" &&
                targetMsg[index].ToString() != "." && targetMsg[index].ToString() != "*")
            {
                SoundManager.Instance.SFXTextPlay(txtsound, txtId);
            }

            // 문자 추가
            currentText += targetMsg[index];
            msgText.text = currentText; // 텍스트 업데이트
            index++;

            // 지연 시간 적용
            float delay = (index < targetMsg.Length && targetMsg[index - 1].ToString() == " ") ? 0.02f : 1f / CharPerSeconds;
            yield return new WaitForSeconds(delay);
        }

        typingCoroutine = null; // 타이핑 완료 후 null로 설정
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
