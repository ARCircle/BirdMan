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
    void GenerateString()
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
    struct TurtleState
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public Mesh GenerateTreeMesh()
    {
        Stack<TurtleState> transformStack = new Stack<TurtleState>();
        Vector3 currentPosition =new Vector3(0,0,0);
        Quaternion currentRotation = Quaternion.identity;

        GenerateString();

        foreach (char c in lSystemString)
        {
            if (c == 'F') // 幹または枝の成長
            {
                Vector3 startPoint = transform.position + currentPosition;

                // Raycast による位置の修正（必要に応じて）
                RaycastHit hit;
                if (Physics.Raycast(startPoint + Vector3.up * 10f, Vector3.down, out hit,2000, LayerMask.GetMask("Map")))
                {
                    startPoint = new Vector3(currentPosition.x, hit.point.y, currentPosition.z);

                    MapGenerator mapGenerator = hit.transform.gameObject.GetComponent<MapGenerator>();
                   // print(hit.point);

                    if (mapGenerator != null)
                    {

                        // このゲームオブジェクトの LSystemMeshGenerator コンポーネントを取得
                        LSystemMeshGenerator lSystemMeshGenerator = gameObject.GetComponent<LSystemMeshGenerator>();

                        if (lSystemMeshGenerator != null)
                        {
                            // print(mapGenerator.lSystemGenerators);
                            if (!mapGenerator.lSystemGenerators.Contains(lSystemMeshGenerator))
                            {
                                // lSystemGenerators リストに追加
                                //  print("lSystemGenerators リストに追加");
                                mapGenerator.lSystemGenerators.Add(lSystemMeshGenerator);
                                //print($" mapGenerator: ({mapGenerator.name})");

                            }
                            else
                            {
                                Debug.LogWarning(lSystemMeshGenerator + "はすでにあります");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("LSystemMeshGenerator コンポーネントが見つかりません。");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("MapGenerator コンポーネントが見つかりません。");
                    }

                    //break; // 一つのマップにヒットしたら、それ以上Rayを飛ばさない
                
                // MapGenerator の処理（省略）
            }
                else
                {
                    startPoint = new Vector3(currentPosition.x,0, currentPosition.z);

                }


                // 制御点を追加
                controlPoints.Add((startPoint, 0));
                radii.Add(segmentRadius);

                // 前進
                currentPosition += currentRotation * Vector3.forward * segmentLength;
            }
            else if (c == '+') // Y軸方向に回転（右方向に分岐）
            {
                currentRotation *= Quaternion.Euler(0, angleX, 0);
            }
            else if (c == '-') // Y軸方向に回転（左方向に分岐）
            {
                currentRotation *= Quaternion.Euler(0, -angleX, 0);
            }
            else if (c == '[') // スタックに現在の状態を保存（分岐の開始）
            {
                transformStack.Push(new TurtleState { position = currentPosition, rotation = currentRotation });
            }
            else if (c == ']') // スタックから状態を復元（分岐の終了）
            {
                var state = transformStack.Pop();
                currentPosition = state.position;
                currentRotation = state.rotation;
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
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
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

        int totalSegments = (controlPoints.Count - 1) * segmentsPerCurve;
        // 既存の頂点が存在するか確認
        int existingVertexIndex = -1;
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {

            //print("i"+i+"controlPoints[i].Item1_" + controlPoints[i].Item1 + "controlPoints[i].Item2_" + controlPoints[i].Item2);
        }
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            float Item2 = controlPoints[i].Item2;
            Vector3 p0 = controlPoints[i].Item1;
            Vector3 p1 = controlPoints[i + 1].Item1;
            //print("controlPoints[i].Item1_" + controlPoints[i].Item1+"controlPoints[i].Item2_" + controlPoints[i].Item2);
            existingVertexIndex = -1;
            //  if (controlPoints[i+1].Item2 >= 2)
            // {
            //  print( "i" + controlPoints[i+1].Item1);

            for (int m = 0; m < i + 1; m++)
            {
                //print("m"+controlPoints[m].Item1+"i"+ controlPoints[i+1].Item1);


                if (controlPoints[m].Item1 == controlPoints[i + 1].Item1)
                {
                    //  print("-----------m" + controlPoints[m].Item1 + "i" + controlPoints[i+1].Item1);

                    existingVertexIndex = i + 1 - m;
                    // print(existingVertexIndex);

                }
            }
            //}



            float r0 = radii[i];
            float r1 = radii[i + 1];

            for (int j = 0; j <= segmentsPerCurve; j++)
            {
                if (existingVertexIndex != -1)
                {
                    break;
                }
                float t = (float)j / segmentsPerCurve;
                Vector3 position = Vector3.Lerp(p0, p1, t);
                float radius = Mathf.Lerp(r0, r1, t);

                for (int k = 0; k < vertexCountPerRing; k++)
                {
                    float angle = k * Mathf.PI * 2 / (vertexCountPerRing - 1);
                    Vector3 offset = new Vector3(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius, 0);
                    Vector3 newPosition = position + offset;


                    /* for (int m = 0; m < controlPoints.Count; m++)
                     {
                         if (controlPoints[m].Item1 == newPosition)
                         {
                             existingVertexIndex = m;
                             break;
                         }
                     }*/

                    // if (existingVertexIndex == -1)
                    // {

                    vertices.Add(newPosition);


                    if (j > 0 && k > 0)
                    {
                        int current = vertices.Count - 1;
                        int previous = current - 1;
                        int below = current - vertexCountPerRing;
                        int belowPrevious = previous - vertexCountPerRing;

                        if (existingVertexIndex != -1)
                        {
                            // below = current - vertexCountPerRing*(existingVertexIndex-1);
                            // belowPrevious = previous - vertexCountPerRing * (existingVertexIndex - 1);

                        }

                        if (existingVertexIndex == -1)
                        {
                            if (Item2 == -1)
                            {
                                triangles.Add(below);

                                triangles.Add(current);
                                triangles.Add(previous);

                                triangles.Add(belowPrevious);

                                triangles.Add(below);
                                triangles.Add(previous);
                            }
                            else
                            {
                                triangles.Add(below);
                                triangles.Add(previous);
                                triangles.Add(current);

                                triangles.Add(belowPrevious);
                                triangles.Add(previous);
                                triangles.Add(below);
                            }
                        }
                    }
                    //  }
                    //  else
                    //  {
                    // 既存の頂点を利用
                    //  newPosition = controlPoints[existingVertexIndex].Item1;
                    //  }

                }

            }
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
