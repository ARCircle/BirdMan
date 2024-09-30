using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralRockGenerator : MonoBehaviour
{
    public float rockRadius = 1f; // 基本の岩の半径
    public int subdivisions = 3;  // 球体の細分化レベル
    public float noiseStrength = 0.3f; // ノイズの強さ
    public bool flattenBelowY = true;  // Y=0以下を平らにするかどうかのフラグ

    private MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        GenerateRock();
    }

    void GenerateRock()
    {
        // 基本となる球体を生成
        Mesh rockMesh = CreateIcoSphere(rockRadius, subdivisions);

        // 頂点にノイズを加えて岩らしい形状に
        Vector3[] vertices = rockMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            // 頂点にランダムなノイズを加える
            vertices[i] += vertices[i].normalized * Random.Range(-noiseStrength, noiseStrength);

            // Y=0以下の頂点を平らにする処理
            if (flattenBelowY && vertices[i].y < 0)
            {
                vertices[i].y = -0.1f; // Y=0以下の部分を平らにする
            }
        }

        // メッシュに頂点を反映
        rockMesh.vertices = vertices;
        rockMesh.RecalculateNormals();
        rockMesh.RecalculateBounds();

        // メッシュをMeshFilterに設定
        meshFilter.mesh = rockMesh;
    }

    // 正二十面体ベースの球体を生成
    Mesh CreateIcoSphere(float radius, int subdivision)
    {
        Mesh mesh = new Mesh();
        IcoSphereCreator.Create(radius, subdivision, ref mesh);
        return mesh;
    }
}

public static class IcoSphereCreator
{
    // 正二十面体ベースの球体を作成するメソッド
    public static void Create(float radius, int subdivisions, ref Mesh mesh)
    {
        // 正二十面体の初期頂点と三角形を設定
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        InitializeIcosahedron(ref vertices, ref triangles);

        // 指定された回数だけ細分化を行う
        Dictionary<long, int> middlePointCache = new Dictionary<long, int>();
        for (int i = 0; i < subdivisions; i++)
        {
            List<int> newTriangles = new List<int>();
            for (int j = 0; j < triangles.Count; j += 3)
            {
                int v1 = triangles[j];
                int v2 = triangles[j + 1];
                int v3 = triangles[j + 2];

                int a = GetMiddlePoint(v1, v2, ref vertices, ref middlePointCache);
                int b = GetMiddlePoint(v2, v3, ref vertices, ref middlePointCache);
                int c = GetMiddlePoint(v3, v1, ref vertices, ref middlePointCache);

                newTriangles.AddRange(new int[] { v1, a, c });
                newTriangles.AddRange(new int[] { v2, b, a });
                newTriangles.AddRange(new int[] { v3, c, b });
                newTriangles.AddRange(new int[] { a, b, c });
            }
            triangles = newTriangles;
        }

        // 頂点を正規化して半径を適用
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].normalized * radius;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    // 正二十面体の初期化
    private static void InitializeIcosahedron(ref List<Vector3> vertices, ref List<int> triangles)
    {
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        vertices.AddRange(new Vector3[]
        {
            new Vector3(-1,  t, 0), new Vector3( 1,  t, 0),
            new Vector3(-1, -t, 0), new Vector3( 1, -t, 0),
            new Vector3( 0, -1,  t), new Vector3( 0,  1,  t),
            new Vector3( 0, -1, -t), new Vector3( 0,  1, -t),
            new Vector3( t,  0, -1), new Vector3( t,  0,  1),
            new Vector3(-t,  0, -1), new Vector3(-t,  0,  1)
        });

        triangles.AddRange(new int[]
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1
        });
    }

    // 頂点の中間点を取得する
    private static int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> middlePointCache)
    {
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        if (middlePointCache.TryGetValue(key, out int ret))
        {
            return ret;
        }

        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = (point1 + point2) / 2f;

        int i = vertices.Count;
        vertices.Add(middle.normalized);
        middlePointCache.Add(key, i);

        return i;
    }
}
