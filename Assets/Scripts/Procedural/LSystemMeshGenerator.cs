using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemMeshGenerator : MonoBehaviour
{
    public int iterations = 1; // L-Systemの反復回数
    public float angleX = 25.0f; // X軸の回転角度
    public float segmentLength = 1.0f; // セグメントの長さ
    public float segmentRadius = 0.1f; // セグメントの半径
    public int segmentsPerCurve = 10; // 各セグメントあたりの細分化数
    public int vertexCountPerRing = 8; // 円周上の頂点数

    private LSystemMesh lSystem;
    private List<(Vector3, int)> controlPoints = new List<(Vector3, int)>(); // 成長の制御点
    private List<float> radii = new List<float>(); // 各セグメントの半径
    private int branchCount = 1; // 分岐のカウント

    public string F = "FF+[+F-F-F]"; // 幹と枝の成長のルール
    string lSystemString;
    void Start()
    {
        // L-Systemのルールを定義
        Dictionary<char, string> rules = new Dictionary<char, string>
        {
            { 'F', F } // 幹と枝の成長
        };

        // L-Systemの初期設定
        lSystem = new LSystemMesh("F", rules, iterations);

        // L-Systemに基づいて木を生成
        lSystemString = lSystem.Generate();

      
    }

     public Mesh GenerateTreeMesh(GameObject mapObject)
    {
        Stack<Matrix4x4> transformStack = new Stack<Matrix4x4>();
        Matrix4x4 currentTransform = Matrix4x4.identity;

        // シーン内の「Map」タグがついたオブジェクトを取得
       // GameObject[] mapObjects = GameObject.FindGameObjectsWithTag("Map");
        //print(mapObject);
        // L-Systemの文字列を処理
        foreach (char c in lSystemString)
        {
            if (c == 'F') // 幹または枝の成長
            {
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);

                // 各MapオブジェクトのMeshColliderに対してRayを飛ばす
               
                    MeshCollider meshCollider = mapObject.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        // 現在のポイントからRayをY軸方向（下）に飛ばす
                        RaycastHit hit;
                     
                        if (Physics.Raycast(startPoint + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Map")))
                        {
                            // Rayが何かに衝突したら、その位置を新しい成長点に設定
                            startPoint = hit.point;
                           //print(hit.point+hit.transform.name);
                            //break; // 一つのマップにヒットしたら、それ以上Rayを飛ばさない
                        }
                    }
                

                // 制御点を追加
                controlPoints.Add((startPoint, 0));
                radii.Add(segmentRadius);

                // Z方向にセグメントを進める
                currentTransform *= Matrix4x4.Translate(Vector3.forward * segmentLength);
            }
            else if (c == '+') // X軸方向に回転（右方向に分岐）
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, angleX, 0));
            }
            else if (c == '-') // X軸方向に回転（左方向に分岐）
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, -angleX, 0));
            }
            else if (c == '[') // スタックに現在のトランスフォームを保存（分岐の開始）
            {
                transformStack.Push(currentTransform);
            }
            else if (c == ']') // スタックからトランスフォームを取り出し（分岐の終了）
            {
                currentTransform = transformStack.Pop();
            }
        }

        // メッシュの生成とアサイン
        Mesh mesh = GenerateMesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        return mesh;
    }

    public void GenerateMeshAndAssign()
    {
        Mesh mesh = GenerateMesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        if (controlPoints.Count < 2 || radii.Count < 2)
        {
            Debug.LogWarning("Not enough control points or radii to generate mesh.");
            return mesh;
        }

        // 制御点に沿ったメッシュ生成
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 p0 = controlPoints[i].Item1;
            Vector3 p1 = controlPoints[i + 1].Item1;

            float r0 = radii[i];
            float r1 = radii[i + 1];

            for (int j = 0; j <= segmentsPerCurve; j++)
            {
                float t = (float)j / segmentsPerCurve;
                Vector3 position = Vector3.Lerp(p0, p1, t);
                float radius = Mathf.Lerp(r0, r1, t);

                for (int k = 0; k < vertexCountPerRing; k++)
                {
                    float angle = k * Mathf.PI * 2 / vertexCountPerRing;
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                    vertices.Add(position + offset);
                }
            }
        }

        // 三角形の生成（メッシュの面）
        for (int i = 0; i < vertices.Count - vertexCountPerRing; i++)
        {
            int current = i;
            int next = i + vertexCountPerRing;

            if ((i + 1) % vertexCountPerRing == 0)
            {
                next -= vertexCountPerRing;
            }

            triangles.Add(current);
            triangles.Add(next);
            triangles.Add(current + 1);

            triangles.Add(current + 1);
            triangles.Add(next);
            triangles.Add(next + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}

public class LSystemMesh
{
    public string axiom;
    private Dictionary<char, string> rules;
    public int iterations;

    public LSystemMesh(string axiom, Dictionary<char, string> rules, int iterations)
    {
        this.axiom = axiom;
        this.rules = rules;
        this.iterations = iterations;
    }

    public string Generate()
    {
        string currentString = axiom;

        for (int i = 0; i < iterations; i++)
        {
            string nextString = "";

            foreach (char c in currentString)
            {
                if (rules.ContainsKey(c))
                {
                    nextString += rules[c];
                }
                else
                {
                    nextString += c;
                }
            }

            currentString = nextString;
        }

        return currentString;
    }
}
