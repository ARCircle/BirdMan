using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PathGenerator pathGenerator;
    public float moveSpeed = 5f; // プレイヤーの移動速度

    private Vector3[] pathPoints;
    private int currentPointIndex = 0;

    void Start()
    {
        // PathGeneratorから道のポイントを取得
        pathPoints = pathGenerator.GetPathPoints();
    }

    void Update()
    {
        if (pathPoints == null || pathPoints.Length == 0) return;

        // 現在のターゲットポイントに向かって移動
        Vector3 targetPoint = pathPoints[currentPointIndex];
        Vector3 direction = (targetPoint - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // ターゲットポイントに近づいたら次のポイントに進む
        if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
        {
            currentPointIndex++;

            if (currentPointIndex >= pathPoints.Length)
            {
                // 終点に達したら停止
                currentPointIndex = pathPoints.Length - 1;
            }
        }
    }
}
