using UnityEngine;

public class BridgeGenerator : MonoBehaviour
{
    public Transform startPoint; // 橋の開始地点
    public Transform endPoint;   // 橋の終了地点
    public GameObject segmentPrefab; // セグメントのプレハブ
    public float segmentSpacing = 0.5f; // セグメント間のスペース
    public float springForce = 100f; // バネの力
    public float damper = 5f; // ダンパーの値
    public float upwardForce = 10f; // 上向きの固定力

    void Start()
    {
        if (startPoint == null || endPoint == null || segmentPrefab == null)
        {
            Debug.LogError("StartPoint, EndPoint, or SegmentPrefab is not assigned.");
            return;
        }

        GenerateBridge();
    }

    void GenerateBridge()
    {
        // 開始地点から終了地点までの距離と方向を計算
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float totalDistance = Vector3.Distance(startPoint.position, endPoint.position);

        // セグメントのスケールを取得し、x軸とz軸のうち小さいほうの値を選択
        float segmentLength = Mathf.Min(segmentPrefab.transform.localScale.x, segmentPrefab.transform.localScale.z);

        // セグメントの数を計算（スペースを含む）
        int numberOfSegments = Mathf.FloorToInt(totalDistance / (segmentLength + segmentSpacing));

        GameObject lastSegment = null;
        // 各セグメントを生成して配置
        for (int i = 0; i <= numberOfSegments; i++)
        {
            Vector3 segmentPosition = startPoint.position + direction * i * (segmentLength + segmentSpacing);

            // セグメントの生成
            GameObject segment = Instantiate(segmentPrefab, segmentPosition, Quaternion.identity);
            segment.transform.rotation = Quaternion.LookRotation(direction); // 向きを調整

            // Rigidbodyの設定
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = segment.AddComponent<Rigidbody>();
                rb.mass = 1f; // 質量を調整（必要に応じて）
            }

            // 上向きの力を加える
         //    ConstantForce cf = segment.AddComponent<ConstantForce>();
         //   cf.force = new Vector3(0, upwardForce, 0);

            // Hinge Jointの設定
            HingeJoint hinge = segment.AddComponent<HingeJoint>();
            if (i == 0)
            {
                //hinge.connectedBody = startPoint.GetComponent<Rigidbody>();
            }
            else
            {
                hinge.connectedBody = lastSegment.GetComponent<Rigidbody>();
            }

            // 最後のセグメントは終了地点に固定
            if (i == numberOfSegments)
            {
             // hinge.connectedBody = endPoint.GetComponent<Rigidbody>();
                // x, y, z軸の移動を固定
                rb.constraints = RigidbodyConstraints.FreezePosition;
            }
            lastSegment = segment;

            // スプリングの設定
            JointSpring spring = new JointSpring();
            spring.spring = springForce;
            spring.damper = damper;
            hinge.spring = spring;
            hinge.useSpring = true;

            // 橋のセグメントを親オブジェクトの子にする
            segment.transform.parent = transform;
        }

        startPoint.gameObject.SetActive(false);
        endPoint.gameObject.SetActive(false);
        segmentPrefab.SetActive(false);
    }
}
