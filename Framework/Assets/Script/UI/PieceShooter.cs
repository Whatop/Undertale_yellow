using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PieceShooter : MonoBehaviour
{
    public GameObject piecePrefab; // 조각 프리팹
    public int pieceCount = 6; // 발사할 조각 개수
    public float minLaunchForce = 350f; // 초기 발사 힘 최소값
    public float deceleration = 0.95f; // X축 감속 값
    public float gravityForce = 475f; // Y축 아래로 끌어당기는 중력 값
    public float pieceLifetime = 10f; // 조각이 사라지는 시간
    public float maxLaunchForce = 600f; // 초기 발사 힘 최대값

    private List<GameObject> activePieces = new List<GameObject>(); // 발사된 조각을 관리할 리스트
    private List<Vector2> velocities = new List<Vector2>(); // 각 조각의 속도를 저장
    private List<float> maxVelocities = new List<float>(); // 각 조각의 최대 속도를 저장

    public void ShootPieces(RectTransform sourceTransform, Color pieceColor)
    {
        for (int i = 0; i < pieceCount; i++)
        {
            // -90도에서 90도 사이의 랜덤 각도 생성
            float randomAngle = Random.Range(-100f, 100f);

            // 조각을 생성
            GameObject piece = Instantiate(piecePrefab, sourceTransform.position, Quaternion.identity, sourceTransform.parent);
            RectTransform pieceRect = piece.GetComponent<RectTransform>();
            pieceRect.anchoredPosition = sourceTransform.anchoredPosition; // anchoredPosition으로 위치 지정

            // 350 ~ 600 사이의 랜덤 초기 발사 힘 설정
            float randomLaunchForce = Random.Range(minLaunchForce, maxLaunchForce);

            // 각 조각마다 다른 최대 속도를 설정
            maxVelocities.Add(randomLaunchForce);

            // 각도에 따라 초기 속도 설정
            Vector2 launchDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.up;
            Vector2 initialVelocity = launchDirection * randomLaunchForce;
            Image image = piece.GetComponent<Image>();
            if (image != null)
            {
                image.color = pieceColor;
            }
            // 조각을 리스트에 추가
            activePieces.Add(piece);
            velocities.Add(initialVelocity);
        }

        // 일정 시간 후에 조각을 파괴
        Invoke(nameof(ClearPieces), pieceLifetime);
    }

    void Update()
    {
        // 매 프레임마다 모든 조각을 이동시킴
        for (int i = 0; i < activePieces.Count; i++)
        {
            if (activePieces[i] != null)
            {
                RectTransform pieceRect = activePieces[i].GetComponent<RectTransform>();

                // X축 감속 처리
                velocities[i] = new Vector2(velocities[i].x * deceleration, velocities[i].y);

                // 중력 추가 (Y축에만 적용)
                velocities[i] += Vector2.down * gravityForce * Time.deltaTime;

                // 조각마다 각자의 최대 속도 제한
                if (Mathf.Abs(velocities[i].x) > maxVelocities[i])
                {
                    velocities[i] = new Vector2(Mathf.Sign(velocities[i].x) * maxVelocities[i], velocities[i].y);
                }

                // 조각 이동
                pieceRect.anchoredPosition += velocities[i] * Time.deltaTime;
            }
        }
    }

    void ClearPieces()
    {
        // 모든 조각을 제거하고 리스트 초기화
        foreach (GameObject piece in activePieces)
        {
            if (piece != null)
            {
                Destroy(piece);
            }
        }
        activePieces.Clear();
        velocities.Clear();
        maxVelocities.Clear();
    }
}
