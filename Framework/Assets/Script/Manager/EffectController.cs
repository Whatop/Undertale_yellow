using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    private Animator animator;
    private float animationLength;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            // 첫 번째 애니메이션 클립의 길이를 가져옴
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips.Length > 0)
            {
                animationLength = clips[0].length; // 필요한 애니메이션 길이를 설정
            }
        }
    }

    private void OnEnable()
    {
        // 애니메이션이 끝나면 자동으로 비활성화
        Invoke(nameof(DisableEffect), animationLength);
    }

    private void DisableEffect()
    {
        gameObject.SetActive(false); // 이펙트를 비활성화
    }
}
