using UnityEngine;

public class RadialMenuHelper : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject segmentPrefab;    // 생성할 세그먼트 프리팹
    public int count = 8;               // 세그먼트 개수
    public float radius;         // 중심에서 거리
    public bool faceOutward = true;     // 세그먼트를 바깥 방향으로 회전시킬지 여부

    [ContextMenu("Generate Segments")]
    public void GenerateSegments()
    {
        if (segmentPrefab == null)
        {
            Debug.LogError("세그먼트 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 기존 자식 삭제
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(rad) * radius,  // 추천 radius
                Mathf.Sin(rad) * radius
            );

            GameObject segment = Instantiate(segmentPrefab, transform);
            segment.transform.localPosition = pos;

            if (faceOutward)
            {
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }

            // 스케일 조정
            segment.transform.localScale = Vector3.one * 0.85f;

            segment.name = $"Segment_{i}";
        }

        Debug.Log($"{count}개의 세그먼트를 생성했습니다.");
    }


}
