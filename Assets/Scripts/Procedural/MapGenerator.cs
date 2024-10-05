using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGenerator : MonoBehaviour
{
    public ComputeShader meshComputeShader; // Compute Shader
    public int gridSize = 20;
    public float meshSize = 10f; // メッシュの全体的なサイズ
    public float noiseScale = 5f; // ノイズのスケール
    public float noiseStrength = 2f; // 高さの強さ

    // L-System Mesh Generatorsのリストを追加
    public List<LSystemMeshGenerator> lSystemGenerators; // L-System Mesh Generatorのリスト

    public float booleanDistance = 15f; // ブーリアン処理の範囲
    public float booleanEffectStrength = 5f; // ブーリアン処理の強度

    [HideInInspector]
    public Mesh mesh;
    [HideInInspector]
    public Vector3[] vertices;

    private MeshCollider meshCollider;

    // ブーリアンオブジェクトの最大数を定義
    private const int MAX_BOOLEAN_VERTICES = 10000; // 必要に応じて調整
   
    void Start()
    {
      
        // まずブーリアン処理なしで地形を生成
        GenerateMeshUsingComputeShader(new Vector3[0]);

       // その後にブーリアン処理を適用した地形を生成
       //StartCoroutine(GenerateTerrainAndApplyBoolean());
    }

    /*public void GenerateBooleanTerrain()
    {
        // 1. L-System Meshを生成
        List<Vector3> booleanVertices = new List<Vector3>();
        //print("GenerateBooleanTerrain()");
        if (lSystemGenerators.Count != 0)
        {
            foreach (LSystemMeshGenerator lSystemGenerator in lSystemGenerators)
            {
              //  print($" lSystemGenerator: ({lSystemGenerator.name})");

                if (lSystemGenerator != null)
                {
                    // Mesh lSystemMesh = lSystemGenerator.GenerateTreeMesh(gameObject);
                    Mesh lSystemMesh = lSystemGenerator.GetComponent<MeshFilter>().sharedMesh;
                    Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
                    if (lSystemMesh != null)
                    {

                        // メッシュのローカル座標での頂点をワールド座標に変換
                        Vector3 meshPosition = lSystemGenerator.transform.position;

                        for (int i = 0; i < lSystemMesh.vertices.Length; i++)
                        {
                          //  print($" meshPosition{i}: ({meshPosition})");
                          //  print($"lSystemMesh.vertices {i}: ({lSystemMesh.vertices[i]})");
                            // 各頂点にメッシュの位置を考慮
                            //Vector3 worldVertex = lSystemGenerator.transform.TransformPoint(lSystemMesh.vertices[i]);
                            booleanVertices.Add(lSystemMesh.vertices[i] + meshPosition);  // メッシュの位置を引く
                        //    booleanVertices.Add(lSystemMesh.vertices[i]);  // メッシュの位置を引く
                            //print($"頂点 {i}: ({lSystemMesh.vertices[i]})+mesh {i}: ({mesh.vertices[i]})");
                        }
                       // lSystemGenerator.gameObject.SetActive(true);
                        // メッシュの頂点を取得し、リストに追加
                         //booleanVertices.AddRange(lSystemMesh.vertices);
                        // print(lSystemMesh.vertices.Length);

                        // 各頂点の座標を表示
                        //  for (int i = 0; i < lSystemMesh.vertices.Length; i++)
                        // {
                        //   Vector3 vertex = lSystemMesh.vertices[i];
                        // print($"頂点 {i}: ({vertex.x}, {vertex.y}, {vertex.z})");
                        // }
                    }


                }

            }


            // 2. Compute Shaderで地形を生成し、ブーリアン処理を適用
            GenerateMeshUsingComputeShader(booleanVertices.ToArray());
        }
        
   } */


    void GenerateMeshUsingComputeShader(Vector3[] booleanVertices)
    {
        // メッシュのサイズとグリッドに対応した頂点数
        int vertexCount = (gridSize + 1) * (gridSize + 1);

        // Compute Shader のスレッド数を計算
        int threadGroupSize = Mathf.CeilToInt((float)(gridSize + 1) / 8.0f);
      ///  Debug.Log("ThreadGroupSize: " + threadGroupSize);

        // 頂点データを格納するバッファを作成
        ComputeBuffer vertexBuffer = new ComputeBuffer(vertexCount, sizeof(float) * 5); // float3 (position) + float2 (uv)

        // Compute Shader にデータを設定
        meshComputeShader.SetInt("gridSize", gridSize);
        meshComputeShader.SetFloat("meshSize", meshSize);
        meshComputeShader.SetFloat("noiseScale", noiseScale);
        meshComputeShader.SetFloat("noiseStrength", noiseStrength);
        meshComputeShader.SetBuffer(0, "result", vertexBuffer);

        // メッシュの位置をglobalOffsetとしてシェーダーに渡す
        Vector2 globalOffset = new Vector2(transform.position.x, transform.position.z);
        //Vector2 globalOffset = new Vector2(transform.position.x, transform.position.z);
        meshComputeShader.SetVector("globalOffset", globalOffset);

        // ブーリアンオブジェクトの頂点データを準備
        int booleanVertexCount = booleanVertices.Length;
        //print(booleanVertexCount);
        /*if (booleanVertexCount > 0)
        {
          
            booleanVertexCount = Mathf.Min(booleanVertexCount, MAX_BOOLEAN_VERTICES);
            //print(booleanVertexCount);
            ComputeBuffer booleanVerticesBuffer = new ComputeBuffer(MAX_BOOLEAN_VERTICES, sizeof(float) * 3);
            booleanVerticesBuffer.SetData(booleanVertices);

            meshComputeShader.SetBuffer(0, "booleanVertices", booleanVerticesBuffer);
            meshComputeShader.SetInt("booleanVertexCount", booleanVertexCount);
            meshComputeShader.SetFloat("booleanDistance", booleanDistance);
            meshComputeShader.SetFloat("booleanEffectStrength", booleanEffectStrength);

            print(booleanVertexCount);
            // Compute Shader を実行
            meshComputeShader.Dispatch(0, threadGroupSize, threadGroupSize, 1);

            booleanVerticesBuffer.Release();
        }
        else
        {*/
            // print("boolean!!!!!");
            //booleanVertexCount = Mathf.Min(booleanVertexCount, MAX_BOOLEAN_VERTICES);


            // ブーリアン処理を行わない場合、booleanVertexCountを0に設定
            booleanVertexCount = Mathf.Min(booleanVertexCount, MAX_BOOLEAN_VERTICES);
            //print(booleanVertexCount);
            ComputeBuffer booleanVerticesBuffer = new ComputeBuffer(MAX_BOOLEAN_VERTICES, sizeof(float) * 3);
            booleanVerticesBuffer.SetData(booleanVertices);

           meshComputeShader.SetBuffer(0, "booleanVertices", booleanVerticesBuffer);
            meshComputeShader.SetInt("booleanVertexCount", 0);
            meshComputeShader.SetFloat("booleanDistance", booleanDistance);
            meshComputeShader.SetFloat("booleanEffectStrength", booleanEffectStrength);


            // Compute Shader を実行
            meshComputeShader.Dispatch(0, threadGroupSize, threadGroupSize, 1);
            booleanVerticesBuffer.Release();
            // バッファを解放

       // }

        // 結果を取得する
        VertexData[] vertexData = new VertexData[vertexCount];
        vertexBuffer.GetData(vertexData);
        vertexBuffer.Release(); // メモリを解放

        // メッシュデータを設定
        mesh = new Mesh();
        vertices = new Vector3[vertexCount]; // クラス変数を初期化
        Vector2[] uvs = new Vector2[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            vertices[i] = vertexData[i].position;
            uvs[i] = vertexData[i].uv;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = CreateTriangles(gridSize, gridSize);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        // メッシュコライダーの更新
       
           meshCollider = gameObject.GetComponent<MeshCollider>();
        
        meshCollider.sharedMesh = mesh;
       // meshCollider.convex = true;
    }

    int[] CreateTriangles(int width, int height)
    {
        int[] triangles = new int[width * height * 6];
        for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++)
        {
            for (int x = 0; x < width; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                triangles[ti + 5] = vi + width + 2;
            }
        }
        return triangles;
    }

    private void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

   
}

struct VertexData
{
    public Vector3 position; // 頂点の位置
    public Vector2 uv;       // UV座標
}
