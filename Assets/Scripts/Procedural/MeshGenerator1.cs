using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator1 : MonoBehaviour
{
    public List<Vector3> controlPoints; // スプラインの制御点のリスト
    public List<float> radii; // 各セクションの半径
    public List<Vector3> rotationAngles; // 各セクションの回転角度
    public bool showGizmos = true; // Gizmos の表示フラグ

    void Start()
    {
        //
        //
        //GenerateMeshAndAssign();
    }

    void OnValidate()
    {
        // エディター上での変更時にメッシュを更新
        if(gameObject.activeSelf&& transform.parent.gameObject.activeSelf)
        GenerateMeshAndAssign();
    }

    public void GenerateMeshAndAssign()
    {
        Mesh mesh = GenerateMesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    // スプライン上の点を取得するための関数
    public Vector3 GetPoint(float t, int segmentIndex)
    {
        return Vector3.Lerp(controlPoints[segmentIndex], controlPoints[segmentIndex + 1], t);
    }

    public Quaternion GetRotation(float t, int segmentIndex)
    {
        Vector3 tangent = (controlPoints[segmentIndex + 1] - controlPoints[segmentIndex]).normalized;

        // 基本の回転
        Quaternion baseRotation = Quaternion.LookRotation(tangent);

        // Inspectorから設定された回転角度を取得し、Quaternionに変換
        Vector3 eulerRotation = Vector3.Lerp(rotationAngles[segmentIndex], rotationAngles[segmentIndex + 1], t);
        Quaternion customRotation = Quaternion.Euler(eulerRotation);

        // 基本の回転にInspectorの回転を適用
        return baseRotation * customRotation;
    }

    private void GenerateCap(Vector3 center, float radius, Quaternion rotation, List<Vector3> vertices, List<int> triangles, bool isStartCap)
    {
        int segments = 10;
        int centerIndex = vertices.Count;
        vertices.Add(center);

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 localPoint = new Vector3(cos * radius, sin * radius, 0);
            Vector3 edgePoint = center + rotation * localPoint;
            vertices.Add(edgePoint);

            if (i < segments)
            {
                int edgeIndex = vertices.Count - 1;
                if (isStartCap)
                {
                    triangles.Add(centerIndex);
                    triangles.Add(edgeIndex + 1);
                    triangles.Add(edgeIndex);
                }
                else
                {
                    triangles.Add(centerIndex);
                    triangles.Add(edgeIndex);
                    triangles.Add(edgeIndex + 1);
                }
            }
        }
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int segmentCount = controlPoints.Count - 1; // 有効なセグメントの数
        Vector3 lastPoint1 = Vector3.zero;
        Quaternion lastRot1 = Quaternion.identity;

        for (int i = 0; i < segmentCount; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                float t0 = (float)j / 10;
                float t1 = (float)(j + 1) / 10;

                Vector3 p0 = GetPoint(t0, i);
                Vector3 p1 = GetPoint(t1, i);

                Quaternion rot0 = GetRotation(t0, i);
                Quaternion rot1 = GetRotation(t1, i);

                float r0 = Mathf.Lerp(radii[i], radii[i + 1], t0);
                float r1 = Mathf.Lerp(radii[i], radii[i + 1], t1);

                // セグメント間のつなぎ目がずれないように修正
                if (j == 0 && i > 0)
                {
                    p0 = lastPoint1;
                    rot0 = lastRot1;
                }

                GenerateCylinderSegment(p0, p1, r0, r1, rot0, rot1, vertices, triangles);

                lastPoint1 = p1;
                lastRot1 = rot1;
            }
        }

        // メッシュの端にキャップを追加
        GenerateCap(controlPoints[0], radii[0], GetRotation(0, 0), vertices, triangles, true);  // 始点キャップ
        GenerateCap(controlPoints[controlPoints.Count - 1], radii[radii.Count - 1], GetRotation(1, segmentCount - 1), vertices, triangles, false); // 終点キャップ

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }


    public void GenerateCylinderSegment(Vector3 p0, Vector3 p1, float r0, float r1, Quaternion rot0, Quaternion rot1, List<Vector3> vertices, List<int> triangles)
    {
        int segments = 10; // 円柱を構成するセグメントの数

        // p0からp1の方向ベクトルを計算
        Vector3 direction = (p1 - p0).normalized;
        Vector3 orthogonalVector = Vector3.Cross(direction, Vector3.up);
        Vector3 previousPoint0 = Vector3.zero;
        Vector3 previousPoint1 = Vector3.zero;
        // 円柱を構成する各頂点の位置を計算し、ねじれを防ぐ
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2 / segments;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector3 localPoint0 = new Vector3(cos * r0, sin * r0, 0);
            Vector3 localPoint1 = new Vector3(cos * r1, sin * r1, 0);
          localPoint0 = new Vector3(cos * r0,0, sin * r0);
            localPoint1 = new Vector3(cos * r1, 0, sin * r1);
            // 最後のメッシュを次のメッシュの先端として使用し、方向ベクトルに平行な成分を補正
            Vector3 point0 = p0 + rot0 * localPoint0;
            Vector3 point1 = p1 + rot1 * localPoint1;
             // point0 = p0 +  localPoint0;
           //  point1 = p1 + localPoint1;
            // 上記補正のため、方向ベクトルに平行な成分を削除
            //point0 -= Vector3.Dot(point0 - p0, orthogonalVector) * orthogonalVector;
            //point1 -= Vector3.Dot(point1 - p1, orthogonalVector) * orthogonalVector;
            // 最初のポイントを保存して、次のセグメントで再利用する
            if (i == 0)
            {
                previousPoint0 = point0;
                previousPoint1 = point1;
            }
            vertices.Add(point0);
            vertices.Add(point1);

            if (i < segments)
            {
                int startIndex = vertices.Count - 2;
              
                triangles.Add(startIndex);
                triangles.Add(startIndex + 3);
                triangles.Add(startIndex + 1);

                triangles.Add(startIndex);
                triangles.Add(startIndex + 2);
                triangles.Add(startIndex + 3);
                // print(p1.z + " ," + p0.z);
                /*if (p1.z >= p0.z)
                {
                    print(p1.z + " >=" + p0.z);
                    // z成分が増加している場合の三角形生成
                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 1);

                    triangles.Add(startIndex);
                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 3);
                }
                else
                {
                    // z成分が減少している場合の三角形生成
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex + 1);
                    triangles.Add(startIndex);

                    triangles.Add(startIndex + 2);
                    triangles.Add(startIndex + 3);
                    triangles.Add(startIndex);
                }*/
            }
        }
    }


    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 制御点と半径の可視化
        if (controlPoints == null || radii == null || controlPoints.Count < 2 || radii.Count < 2)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < controlPoints.Count; i++)
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(controlPoints[i]), radii[i]);
            if (i < controlPoints.Count - 1)
            {
                Gizmos.DrawLine(transform.TransformPoint(controlPoints[i]), transform.TransformPoint(controlPoints[i + 1]));
            }
        }
    }

    public void SwapControlPoints(int index1, int index2)
    {
        if (index1 < 0 || index2 < 0 || index1 >= controlPoints.Count || index2 >= controlPoints.Count)
            return;

        // Control Pointsの入れ替え
        Vector3 tempPoint = controlPoints[index1];
        controlPoints[index1] = controlPoints[index2];
        controlPoints[index2] = tempPoint;

        // Radiiの入れ替え
        float tempRadius = radii[index1];
        radii[index1] = radii[index2];
        radii[index2] = tempRadius;

        // Rotation Anglesの入れ替え
        Vector3 tempAngle = rotationAngles[index1];
        rotationAngles[index1] = rotationAngles[index2];
        rotationAngles[index2] = tempAngle;

        // メッシュを再生成
        GenerateMeshAndAssign();
    }
}
