using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]



public class LSystemTreeGenerator : MonoBehaviour
{
    public int iterations = 1; // L-Systemの反復回数
    public float angleX = 25.0f; // 回転角度
    public float angleZ = 25.0f; // 回転角度
    public float segmentLength = 1.0f; // セグメントの長さ
    public float segmentRadius = 0.1f; // セグメントの半径
    public int segmentsPerCurve = 10; // 各セグメントあたりの細分化数
    public int vertexCountPerRing = 8; // 円周上の頂点数

    private LSystem lSystem;
    List<(Vector3, int)> controlPoints = new List<(Vector3, int)>{};
    private List<float> radii = new List<float>();
    private int branchCount = 1; // 分岐のカウントを開始
    public string F = "FF+[+F-F-F+L]&[-F+F+F+L]^[-F+F+F+L]"; // 幹と枝の成長
    public string R = "+[+R-R-R]&[-R+R+R]^[-R+R+R]";



    [Serializable]
    public class LeafParameter
    {

        public Vector2 piece = new Vector2(1, 2);

        public Vector2 posX = new Vector2(-0.5f, 0.5f);
        public Vector2 posY = new Vector2(-0.5f, 0.5f);
        public Vector2 posZ = new Vector2(-0.5f, 0.5f);
        public Vector2 rotX = new Vector2(-90f, 90f);
        public Vector2 rotY = new Vector2(0f, 360f);
        public Vector2 rotZ = new Vector2(0f, 90f);
        public Vector2 scale = new Vector2(0.5f, 1.5f);

    }

    [Serializable]
    public class FruitParameter
    {

        public Vector2 piece = new Vector2(1, 2);

        public Vector2 posX = new Vector2(-0.5f, 0.5f);
        public Vector2 posY = new Vector2(-0.5f, 0.5f);
        public Vector2 posZ = new Vector2(-0.5f, 0.5f);
        public Vector2 rotX = new Vector2(-90f, 90f);
        public Vector2 rotY = new Vector2(0f, 360f);
        public Vector2 rotZ = new Vector2(0f, 90f);
        public Vector2 scale = new Vector2(0.5f, 1.5f);

    }

    public LeafParameter leafP;
    public FruitParameter fruitP;
    void Start()
    {
        // L-Systemのルールを定義
        // L-Systemのルールを定義
        Dictionary<char, string> rules = new Dictionary<char, string>
    {
        { 'F',F }, // 幹と枝の成長
        
        { 'R',R } // 根の成長パターンを追加
    };

        lSystem = new LSystem("RF", rules, iterations);

        string lSystemString = lSystem.Generate();
        // メッシュ生成
        GenerateTreeMesh(lSystemString);

      
    }
    Vector3 leafPoint1;
    void GenerateTreeMesh(string lSystemString)
    {
        Stack<Matrix4x4> transformStack = new Stack<Matrix4x4>();
        Matrix4x4 currentTransform = Matrix4x4.identity;
        int i = 1;
        char lastChar= 'R';
        foreach (char c in lSystemString)
        {
            float angleX2 = UnityEngine.Random.Range(-angleX, angleX); // X軸回転角度をランダムに設定
            float angleZ2 = UnityEngine.Random.Range(-angleZ, angleZ); // X軸回転角度をランダムに設定

            if (c == 'R') // 根の処理を最初に行う
            {
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);
                //print("startPoint"+startPoint);
                // 根の制御点を追加
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, -1)); // 根を示すタイプを「-1」に設定
                    radii.Add(segmentRadius * Mathf.Pow(0.9f, i));
                    i++;
                }
                lastChar = c;
                //currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength);
                // 下方向に進むためにY軸の負方向に移動
                currentTransform *= Matrix4x4.Translate(Vector3.down * segmentLength);
            }
            else if (c == 'F' && lastChar == 'R') // 根の上に幹を生成() // 根の上に幹を生成
            {
                currentTransform = Matrix4x4.identity; // 幹の生成前にトランスフォームをリセット
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);
                i = 1;
                // 幹の制御点を追加
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, 0));
                    radii.Add(segmentRadius * Mathf.Pow(0.9f, i));
                    i++;
                }
                lastChar = c;
                currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength);
            }
            else if (c == 'F' ) // 根の上に幹を生成(c == 'F' && lastChar == 'R') // 根の上に幹を生成
            {
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);

                // 幹の制御点を追加
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, 0));
                    radii.Add(segmentRadius * Mathf.Pow(0.9f, i));
                    i++;
                }
                lastChar = c;
                currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength);
            }
            else if (c == 'B') // 枝の処理を追加
            {
                // 枝のセグメントの処理
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);

                // 枝の制御点を追加
                if (controlPoints.Count == 0 || controlPoints[controlPoints.Count - 1].Item1 != startPoint)
                {
                    controlPoints.Add((startPoint, 3)); // 枝を示すタイプを「3」に設定
                    radii.Add(segmentRadius * Mathf.Pow(0.7f, i)); // 枝の太さは幹より細い
                    i++;
                }

                currentTransform *= Matrix4x4.Translate(Vector3.up * segmentLength * 0.5f); // 枝の長さを調整（幹より短くする）
            }
            else if (c == 'L') // 葉の生成処理を追加
            {
                Vector3 leafPosition = currentTransform.MultiplyPoint(Vector3.zero);
                
                GenerateLeaves(leafPosition);
            }
            else if (c == 'O') // リンゴの生成処理を追加
            {
                Vector3 currentPoint = currentTransform.MultiplyPoint(Vector3.zero);
                // 前回の点（もし存在するなら）
                if (controlPoints.Count > 0)
                {
                    // 最後の制御点を取得
                    Vector3 lastPoint = controlPoints[controlPoints.Count - 1].Item1;

                    // 内分比を決める（例：1:2 の比率で内分する場合）
                    float ratio = 0f; // 1:2 の比率を使用していると仮定

                    // 内分点の計算
                    Vector3 fruitPosition = Vector3.Lerp(lastPoint, currentPoint, ratio);

                    // 内分点を使ってリンゴを配置
                    GenerateFruits(fruitPosition);
                }
                else
                {
                    // 最初のリンゴの位置は現在の変換行列の位置を使用
                    GenerateFruits(currentPoint);
                }
            }
            else if (c == '+')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, angleX2));
            }
            else if (c == '-')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, -angleX2));
            }
            else if (c == '&')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(angleZ2, 0, 0)); // y-z平面での回転（下向き）
            }
            else if (c == '^')
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(-angleZ2, 0, 0)); // y-z平面での回転（上向き）
            }
            else if (c == '[')
            {
              // print("lastChar" + lastChar );
                leafPoint1 = currentTransform.MultiplyPoint(Vector3.zero);
                if(lastChar=='R')
                    controlPoints.Add((currentTransform.MultiplyPoint(Vector3.zero), -1));
                else
                controlPoints.Add((currentTransform.MultiplyPoint(Vector3.zero), 2));
                radii.Add(segmentRadius* Mathf.Pow(0.9f, i));
                transformStack.Push(currentTransform);
             
            }
            else if (c == ']')
            {



                Vector3 endPoint = currentTransform.MultiplyPoint(Vector3.zero);
//if(lastChar != 'R')
               // GenerateLeaves((4*endPoint+leafPoint1)/5);
                //GenerateLeaves((4*endPoint+leafPoint1)/5);
                currentTransform = transformStack.Pop();

                i--;  // 分岐から戻るときはインデックスを減少させる（枝が太くなる）
            }
        }

        // メッシュの生成とアサイン
        GenerateMeshAndAssign();
    }
    public GameObject leafPrefab;
    // public Vector2 leafP.rotX = new Vector2(0f,90f); // 葉の回転範囲

    void GenerateLeaves(Vector3 endPoint)
    {
        if (leafPrefab != null)
        {
            float randomPiece = Mathf.FloorToInt(UnityEngine.Random.Range(leafP.piece.x, leafP.piece.y));

            // Xの整数部分のループ
            for (int i = 0; i < randomPiece; i++)
            {
              
          

           
            // ランダムなオフセットを設定
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(leafP.posX.x, leafP.posX.y),
                UnityEngine.Random.Range(leafP.posY.x, leafP.posY.y),
                UnityEngine.Random.Range(leafP.posZ.x, leafP.posZ.y)
            );

                // ランダムな回転を設定
                Quaternion randomRotation = Quaternion.Euler(
                    UnityEngine.Random.Range(leafP.rotX.x, leafP.rotX.y), // X軸の回転範囲
                    UnityEngine.Random.Range(leafP.rotY.x, leafP.rotY.y), // Y軸の回転範囲
                    UnityEngine.Random.Range(leafP.rotZ.x, leafP.rotZ.y)  // Z軸の回転範囲
                );
                // ランダムなスケールを設定
                float randomScale = UnityEngine.Random.Range(leafP.scale.x, leafP.scale.y);


                // 葉を配置
                //   GameObject leaf = Instantiate(leafPrefab, endPoint+transform.position + randomOffset, Quaternion.identity, transform);
                // 葉を配置
                GameObject leaf = Instantiate(leafPrefab, endPoint + transform.position + randomOffset, randomRotation, transform);

               // GenerateFruits(endPoint,leaf);
            // スケールを適用
            leaf.transform.localScale = leaf.transform.localScale*randomScale;

            }
        }
    }
    public GameObject fruitPrefab; // リンゴ用のプレハブを指定するためのpublicフィールド
    public Camera mainCamera; // 使用するカメラを指定（Orthographicカメラ）
    void GenerateFruits(Vector3 fruitPosition)
    {
        if (fruitPrefab != null)
        {
            // ランダムなスケールを設定
            float randomScale = UnityEngine.Random.Range(leafP.scale.x, fruitP.scale.y);
            // ランダムな回転を設定
            Quaternion randomRotation = Quaternion.Euler(
                UnityEngine.Random.Range(fruitP.rotX.x, fruitP.rotX.y), // X軸の回転範囲
                UnityEngine.Random.Range(fruitP.rotY.x, fruitP.rotY.y), // Y軸の回転範囲
                UnityEngine.Random.Range(fruitP.rotZ.x, fruitP.rotZ.y)  // Z軸の回転範囲
            );

            // リンゴを配置
            GameObject fruit = Instantiate(fruitPrefab, fruitPosition + transform.position, randomRotation, transform);

            // スケールを適用
            fruit.transform.localScale = fruit.transform.localScale * randomScale;
        }
    }

    /* void GenerateFruits(Vector3 endPoint, GameObject leaf)
     {
         if (leafPrefab != null && fruitPrefab != null)
         {
             Mesh leafMesh = leaf.GetComponent<MeshFilter>().sharedMesh; // leafPrefabのメッシュを取得
             if(leafMesh == null)
                 leafMesh = leaf.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh; // leafPrefabのメッシュを取得

             Vector3[] vertices = leafMesh.vertices; // メッシュの頂点を取得
             Vector3[] normals = leafMesh.normals; // メッシュの法線を取得

             int randomPiece = Mathf.FloorToInt(UnityEngine.Random.Range(leafPiece.x, leafPiece.y));

             for (int i = 0; i < randomPiece; i++)
             {
                 // ランダムなスケールを設定
                 float randomScale = UnityEngine.Random.Range(leafScaleRange.x, leafScaleRange.y);

                 // 頂点をランダムに選択
                 int randomIndex = UnityEngine.Random.Range(0, vertices.Length);
                 Vector3 randomVertex = vertices[randomIndex];
                 Vector3 normal = normals[randomIndex];

                 // 法線がY方向に負であるかを確認
                 if (normal.y < 0.5f)
                 {
                     // ワールド座標系に変換
                     Vector3 leafPosition = leaf.transform.TransformPoint(randomVertex);

                     // ランダムなオフセットを設定
                     Vector3 randomOffset = new Vector3(
                         UnityEngine.Random.Range(-leafPositionOffsetRange.x, leafPositionOffsetRange.x),
                         UnityEngine.Random.Range(-leafPositionOffsetRange.y, leafPositionOffsetRange.y),
                         UnityEngine.Random.Range(-leafPositionOffsetRange.z, leafPositionOffsetRange.z)
                     );

                     // カメラからオブジェクトまでのレイを生成
                     // print("leafPosition" + leafPosition);
                     //Vector3 leafPositionOffset = new Vector3(0, 0.01f, 0);
                    // leafPosition = leafPosition + normal/8f;
                     Vector3 leafPositionOffset = normal*10;
                     Vector3 directionToLeaf = leafPosition - mainCamera.transform.position;
                     //directionToLeaf -= directionToLeaf.normalized/100;
                     // レイキャストを行い、カメラとfruitPrefabの間に他のオブジェクトがあるか確認
                     if (Physics.Raycast(mainCamera.transform.position, directionToLeaf, out RaycastHit hitInfo, directionToLeaf.magnitude))
                     {
                         // カメラからのレイが遮られていない場合、リンゴを配置
                        //print("hitInfo.transform.name" +hitInfo.transform.name);

                     }
                     else
                     {
                         // カメラの正面方向を取得
                         Vector3 cameraForward = mainCamera.transform.forward;

                         // 法線とカメラ正面の内積を計算
                         float dotProduct = Vector3.Dot(normal, cameraForward);
                        // print(dotProduct);
                         // 内積が負の場合、オブジェクトはカメラの方向を向いている
                         if (dotProduct < 0)
                         {
                             // 法線の逆方向に向けて回転を設定
                             Quaternion rotation = Quaternion.LookRotation(mainCamera.transform.forward,-normal);
                             //Quaternion rotation = Quaternion.LookRotation(mainCamera.transform.forward);//billboard風になる


                             // リンゴを配置
                            // GameObject fruit = Instantiate(fruitPrefab, leafPosition, rotation, transform);
                             GameObject fruit = Instantiate(fruitPrefab, leafPosition, Quaternion.identity, transform);
                             // リンゴ用のマテリアルを取得し、Render Queueを設定する


                             // スケールを適用
                            // fruit.transform.localScale = Vector3.one * randomScale;
                         }
                     }
                 }
                 else
                 {
                   //  i--; // 法線が負でない場合、カウンターを減らして再試行
                 }
             }
         }
     }*/



    public void GenerateMeshAndAssign()
    {
        Mesh mesh = GenerateMesh();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
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

                for (int m = 0; m < i+1; m++)
                {
                    //print("m"+controlPoints[m].Item1+"i"+ controlPoints[i+1].Item1);
                    

                    if (controlPoints[m].Item1 == controlPoints[i+1].Item1)
                    {
                       //  print("-----------m" + controlPoints[m].Item1 + "i" + controlPoints[i+1].Item1);

                        existingVertexIndex = i+1-m;
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
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
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

                        if (existingVertexIndex  != -1)
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

public class LSystem
{
    public string axiom;
    private Dictionary<char, string> rules;
    public int iterations;

    public LSystem(string axiom, Dictionary<char, string> rules, int iterations)
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
