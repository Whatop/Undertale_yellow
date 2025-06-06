using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LaserFadeOut : MonoBehaviour
{
    [Header("레이저 기본 설정")]
    public Transform laserOrigin;       // 레이저 발사 시작 지점 (예: 소울의 Transform)
    public float maxDistance = 30f;     // 레이저가 닿을 수 있는 최대 거리
    public LayerMask obstacleMask;      // Raycast에 사용할 레이어 마스크 (Wall, Barrier, Soul 등 모두 포함)

    [Header("성장 & 페이드 설정")]
    public float growSpeed = 10f;       // 레이저가 초당 얼만큼 길어질지 (유닛/초)
    public float fadeDuration = 0.5f;   // Barrier에 닿은 뒤 알파가 1→0으로 줄어드는 시간(초)

    [Header("비주얼 두께 설정")]
    public float thickness = 0.8f;      // 레이저 선의 두께 (LineRenderer.widthMultiplier 대신 간단히 사용)

    [Header("DoT 설정")]
    public float dotInterval = 0.2f;    // Soul(Player) 지속 데미지 주기 (초)
    public float dotDPS = 50f;          // 초당 데미지 (Damage Per Second)
    private Dictionary<GameObject, float> hitTimer = new Dictionary<GameObject, float>();

    // “Barrier까지의” 목표 거리 (매 프레임 재계산)
    private float targetDistance = 0f;
    // 현재 레이저가 자라난 길이 (World 단위 기준)
    private float currentLength = 0f;
    // 페이드아웃 시작 이후 경과 시간
    private float fadeTimer = 0f;

    // 원본 선 색상 그라데이션(알파 포함)을 저장
    private Gradient originalGradient;
    private bool hasHitBarrier = false;  // Barrier 충돌 여부 추가

    // LineRenderer 컴포넌트
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // 반드시 두 점으로 그리도록 세팅
        lineRenderer.positionCount = 2;

        // LineRenderer를 “월드 좌표”로 그리게 설정
        lineRenderer.useWorldSpace = true;

        // 레이저 두께 설정
        lineRenderer.widthMultiplier = thickness;

        // 현재 Inspector에 설정된 Color Gradient를 복사해 둠
        originalGradient = lineRenderer.colorGradient;
    }
    private void Start()
    {
        currentLength = 0f;
        fadeTimer = 0f;
        hitTimer.Clear();

        // Start/End Color 알파를 1로 초기화
        Color sc = lineRenderer.startColor;
        Color ec = lineRenderer.endColor;
        sc.a = 1f;
        ec.a = 1f;
        lineRenderer.startColor = sc;
        lineRenderer.endColor = ec;
    }

    private void Update()
    {
        Vector2 originPos = laserOrigin.position;
        Vector2 direction = laserOrigin.right.normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(originPos, direction, maxDistance, obstacleMask);
        float nearestBarrierDist = maxDistance;
        bool barrierFound = false;
        List<RaycastHit2D> soulHits = new List<RaycastHit2D>();
        foreach (var h in hits)
        {
            if (h.collider == null) continue;

            if (h.collider != null && h.collider.CompareTag("Barrier") && h.distance < nearestBarrierDist)
            {
                nearestBarrierDist = h.distance;
               barrierFound = true;
            }
            else if (h.collider.CompareTag("Soul"))
            {
                soulHits.Add(h);
            }
        }
        // 7) 적에게 DoT(지속 데미지) 처리
        if (barrierFound)
        {
            // Barrier 앞에 있는 Enemy들만 처리
            foreach (var eh in soulHits)
            {
                if (eh.distance < nearestBarrierDist)
                {
                    ApplyDotToEnemy(eh.collider.gameObject);
                }
            }
        }
        else
        {
            // Barrier 없으면 구간 내 모든 Enemy에게 처리
            foreach (var eh in soulHits)
            {
                ApplyDotToEnemy(eh.collider.gameObject);
            }
        }

        // 1. targetDistance 설정
        targetDistance = barrierFound ? nearestBarrierDist : maxDistance;

        // 2. 성장 vs 페이드
        if (!hasHitBarrier && currentLength < targetDistance)
        {
            currentLength += growSpeed * Time.deltaTime;
            currentLength = Mathf.Min(currentLength, targetDistance);
        }

        // 3. Barrier 도달 조건
        if (barrierFound && !hasHitBarrier && currentLength >= nearestBarrierDist - (growSpeed * Time.deltaTime))
        {
            hasHitBarrier = true;
            EffectManager.Instance.SpawnEffect("barrier_flash", originPos + direction * nearestBarrierDist, Quaternion.identity);
        }

        // 4. LineRenderer 그리기
        float actualDist = barrierFound ? nearestBarrierDist : currentLength;
        lineRenderer.SetPosition(0, originPos);
        lineRenderer.SetPosition(1, originPos + direction * actualDist);

        // 5. 페이드 트리거: Barrier에 닿았거나, maxDistance에 도달했을 때
        if (hasHitBarrier || currentLength >= maxDistance)
        {
            fadeTimer += Time.deltaTime;
            float fadePercent = Mathf.Clamp01(fadeTimer / fadeDuration);
            float currentAlpha = Mathf.Lerp(1f, 0f, fadePercent);

            SetLineRendererAlpha(currentAlpha);

            if (Mathf.Approximately(currentAlpha, 0f))
            {
                Destroy(gameObject);
            }
        }
    }

    // Material 색상 변경 함수
    private void SetLineRendererAlpha(float alpha)
    {
        if (lineRenderer.material.HasProperty("_BaseColor"))
        {
            Color currentColor = lineRenderer.material.GetColor("_BaseColor");
            currentColor.a = alpha;
            lineRenderer.material.SetColor("_BaseColor", currentColor);
        }
        else if (lineRenderer.material.HasProperty("_Color"))
        {
            Color currentColor = lineRenderer.material.color;
            currentColor.a = alpha;
            lineRenderer.material.color = currentColor;
        }
    }


    /// <summary>
    /// 일정 간격(dotInterval)마다 Soul(GameObject)에 DoT(지속 데미지) 적용
    /// </summary>
    private void ApplyDotToEnemy(GameObject soulObj)
    {
        if (!hitTimer.ContainsKey(soulObj) || Time.time - hitTimer[soulObj] >= dotInterval)
        {
            hitTimer[soulObj] = Time.time;
            // 초당 dotDPS 피해량을 Time.deltaTime과 곱해서 프레임당 DoT
            GameManager.Instance.GetPlayerData().player.GetComponent<PlayerMovement>().TakeDamage(1);


        }
    }


}
