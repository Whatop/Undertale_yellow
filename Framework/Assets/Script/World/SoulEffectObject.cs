using System.Collections;
using UnityEngine;

public class SoulEffectObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float fadeDuration = 0.1f; // 투명해지는 시간
    private Color targetColor = Color.white;

    public void SetColor(Color newColor)
    {
        targetColor = newColor;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
    }

    private void OnEnable()
    {
        spriteRenderer.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0.25f);
        StartCoroutine(FadeOutAndDisable());
    }
    private void OnDisable()
    {
        spriteRenderer.color = new Color(1, 1, 1, 1f); // 완전 초기화
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
