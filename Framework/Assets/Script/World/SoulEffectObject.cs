using System.Collections;
using UnityEngine;

public class SoulEffectObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float fadeDuration = 0.1f; // 투명해지는 시간

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    private void OnEnable()
    {
        Color color = spriteRenderer.color;
        color.a = 0.25f;
        spriteRenderer.color = color;
        StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        float elapsedTime = 0f;
        Color color = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        gameObject.SetActive(false); // 비활성화
    }
}
