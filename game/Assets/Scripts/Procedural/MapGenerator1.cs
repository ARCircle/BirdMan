using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapGenerator1 : MonoBehaviour
{
    public int gridSize = 20;
    public float meshSize = 10f; // メッシュの全体的なサイズ
    [HideInInspector]
    public Mesh mesh;
    [HideInInspector]
    public Vector3[] vertices;
    private MeshCollider meshCollider;

    public List<LSystemMeshGenerator> lSystemGenerators; // LSystemMeshGeneratorのリストを追加

    public float SandAmount = 1000;
    public float booleanDistance = 15f; // 凹ませる強度
    public float booleanEffectStrength = 5f; // 凹ませる強度

    public float noiseScale = 5f; // ノイズのスケール
    public float noiseStrength = 2f; // 高さの強さ

    public float minZForSidePlane = -5f; // 板を生成するZ座標の範囲
    public float sidePlaneHeight = -1f; // 板の高さ（下方向に伸ばす）

    public int textureResolution = 256; // テクスチャの解像度
    private Texture2D heightMap; // 高さマップ用のテクスチャ

    private void Start()
    {
        GenerateMesh();

        // meshCollider = gameObject.AddComponent<MeshCollider>();

        meshCollider.sharedMesh = mesh;

        // すべてのLSystemMeshGeneratorに対してGenerateTreeMeshを実行
        StartCoroutine(GenerateTreeMeshesCoroutine());

        // ApplyBooleanEffectCoroutineを実行
        StartCoroutine(ApplyBooleanEffectCoroutine());
    }

    // LSystemMeshGeneratorオブジェクトに順次GenerateTreeMeshを実行するコルーチン
    private IEnumerator GenerateTreeMeshesCoroutine()
    {
        foreach (var lSystemGenerator in lSystemGenerators)
        {
            if (lSystemGenerator != null)
            {
                string lSystemString = "F"; // 例: 実際のL-System文字列をここに挿入
                //lSystemGenerator.GenerateTreeMesh(gameObject);
                yield return null; // 次のフレームまで待機してから次のオブジェクトに移る
            }
        }
    }

    public Vector3[,] verticesSide; // 2次元配列で頂点を管理
    private float globalOffsetX = 0f; // グローバルなオフセットX
    private float globalOffsetZ = 0f; // グローバルなオフセットZ

    private void GenerateMesh()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        float stepSize = meshSize / gridSize;
        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        verticesSide = new Vector3[gridSize + 1, gridSize + 1]; // 2次元配列を初期化

        Vector2[] uvs = new Vector2[vertices.Length];

        float xOffset = -meshSize / 2f;
        float zOffset = -meshSize / 2f;

        float offsetX = transform.position.x; // 連続性のためのオフセット
        float offsetZ = transform.position.z; // 連続性のためのオフセット

        for (int i = 0, z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++, i++)
            {
                // オクターブノイズを使った地形生成
                float y = -Mathf.PerlinNoise((offsetZ + z * stepSize) / noiseScale, (offsetX + x * stepSize) / noiseScale) * noiseStrength;
                // float y = -FractalNoise(((float)z + 200 * (offsetZ / 32f)) / noiseScale,  ((float)x + offsetX) / noiseScale, 2, 0.5f, 2.0f) * noiseStrength;

                vertices[i] = new Vector3(x * stepSize + xOffset, y, z * stepSize + zOffset);
                verticesSide[z, x] = new Vector3(x * stepSize + xOffset, y, z * stepSize + zOffset); // 2次元で保存

                uvs[i] = new Vector2((float)x / gridSize, (float)z / gridSize);
            }
        }

        // メッシュの設定
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = CreateTriangles(gridSize, gridSize);
        mesh.RecalculateNormals();

        // メッシュコライダーの更新
        UpdateMeshCollider();
    }

    float FractalNoise(float x, float z, int octaves, float persistence, float lacunarity)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;  // 正規化用

        for (int i = 0; i < octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;  // 持続性（amplitudeの減衰率）
            frequency *= lacunarity;  // 周波数の増加率
        }

        return total / maxValue;  // 0～1に正規化
    }

    private void UpdateMeshCollider()
    {
        // メッシュコライダーが存在しない場合は追加
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        // メッシュコライダーのメッシュを更新
        meshCollider.sharedMesh = null; // 一度nullにしてから更新
        meshCollider.sharedMesh = mesh;
    }

    public GameObject player; // プレイヤーオブジェクト

    private GameObject currentSidePlane; // 現在表示されているSidePlane
    private int currentSidePlaneIndex = -1; // 現在表示されているSidePlaneのインデックス

    private void GenerateSidePlanes()
    {
        // 各z列ごとにサイドプレーンを生成
        for (int z = 0; z <= gridSize; z++)
        {
            List<Vector3> sideVerticesList = new List<Vector3>();

            // 各z列のx方向の頂点を収集
            for (int x = 0; x <= gridSize; x++)
            {
                sideVerticesList.Add(verticesSide[z, x]);
            }

            if (sideVerticesList.Count < 2)
            {
                Debug.LogWarning("Not enough vertices to create a side plane.");
                continue; // このz列はスキップ
            }

            // X座標でソートして、頂点リストを昇順に並べる
            sideVerticesList.Sort((v1, v2) => v1.x.CompareTo(v2.x));

            // 上辺と下辺の頂点を定義
            int vertexCount = sideVerticesList.Count;
            Vector3[] sideVertices = new Vector3[vertexCount * 2]; // 上辺と下辺を合わせた頂点数

            // 上辺の頂点を設定 (sideVerticesList に基づく)
            for (int i = 0; i < vertexCount; i++)
            {
                sideVertices[i] = sideVerticesList[i]; // 上辺
                sideVertices[i + vertexCount] = new Vector3(sideVerticesList[i].x, sideVerticesList[i].y + sidePlaneHeight, sideVerticesList[i].z); // 下の頂点
            }

            // 三角形のインデックスを生成
            int[] sideTriangles = new int[(vertexCount - 1) * 6];
            for (int i = 0, ti = 0; i < vertexCount - 1; i++, ti += 6)
            {
                sideTriangles[ti] = i;
                sideTriangles[ti + 1] = i + 1;
                sideTriangles[ti + 2] = i + vertexCount;

                sideTriangles[ti + 3] = i + 1;
                sideTriangles[ti + 4] = i + vertexCount + 1;
                sideTriangles[ti + 5] = i + vertexCount;
            }

            // インデックスを管理するための辞書などに格納しておく
        }
    }

    public float scrollSpeed = 1f;
    bool isSet;
    private void Update()
    {
        // プレイヤーのz方向の位置を確認してSidePlaneの表示を管理
        float playerZPosition = player.transform.position.z;
    }

    // 高さマップを生成する関数
    private void GenerateHeightMap()
    {
        // 高さマップ用のテクスチャを生成
        heightMap = new Texture2D(textureResolution, textureResolution, TextureFormat.RGB24, false);

        // メッシュの頂点を取得
        Vector3[] vertices = mesh.vertices;

        // メッシュ全体のx, zの範囲を計算
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var vertex in vertices)
        {
            if (vertex.x < minX) minX = vertex.x;
            if (vertex.x > maxX) maxX = vertex.x;
            if (vertex.z < minZ) minZ = vertex.z;
            if (vertex.z > maxZ) maxZ = vertex.z;
        }

        // 各頂点のy座標を取得し、それに基づいてピクセルに色を付ける
        for (int i = 0; i < vertices.Length; i++)
        {
            // 頂点のx, z座標をテクスチャのピクセル座標にマッピング
            float normalizedX = Mathf.InverseLerp(minX, maxX, vertices[i].x);
            float normalizedZ = Mathf.InverseLerp(minZ, maxZ, vertices[i].z);

            // テクスチャ座標に丸める
            int xCoord = Mathf.RoundToInt(normalizedX * (textureResolution - 1));
            int zCoord = Mathf.RoundToInt(normalizedZ * (textureResolution - 1));

            // y座標を0～1の範囲に正規化（必要に応じて調整）
            float normalizedHeight = Mathf.InverseLerp(-1f, 1f, vertices[i].y / 10); // 高さの範囲を仮定

            // テクスチャに高さを反映（白黒画像として扱う）
            Color color = new Color(normalizedHeight, normalizedHeight, normalizedHeight);
            heightMap.SetPixel(xCoord, zCoord, color);
        }

        // テクスチャを適用
        heightMap.Apply();
    }

    public float mountainHeightPower = 0.2f;
    public float lakeHeightMin = -0.2f;
    public float loadHeightMin = -0.1f;

    // ApplyBooleanEffe

    private int frameCounter = 0;
    private float totalTime = 0f;
    public float fpsTarget = 30f; // 目標のFPSを設定
    private int stepSize = 500; // 処理ごとのデフォルトのステップ数

    // コルーチンでFPSを監視しつつ処理を調整
    private IEnumerator ApplyBooleanEffectCoroutine()
    {
        // ブーリアン処理用オブジェクトを検索
        GameObject[] booleanObjects = GameObject.FindGameObjectsWithTag("BooleanObject");
        if (booleanObjects == null || booleanObjects.Length == 0) yield break;

        foreach (var booleanObject in booleanObjects)
        {
            if (booleanObject == null || booleanObject.transform == null) continue;

            MeshFilter meshFilter = booleanObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.mesh == null) continue;

            booleanObject.GetComponent<MeshRenderer>().enabled = false;

            Mesh booleanMesh = meshFilter.mesh;
            Transform booleanTransform = booleanObject.transform;
            Vector3[] booleanVertices = booleanMesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (booleanTransform == null) continue;

                Vector3 worldVertexPosition = transform.TransformPoint(vertices[i]);

                foreach (Vector3 booleanVertex in booleanVertices)
                {
                    Vector3 worldBooleanVertex = booleanTransform.TransformPoint(booleanVertex);

                    if (worldBooleanVertex.y <= 0)
                    {
                        float distance = Vector3.Distance(worldVertexPosition, worldBooleanVertex);
                        if (distance < booleanDistance)
                        {
                            float effect = Mathf.Clamp01(1 - (distance / booleanEffectStrength));

                            if (booleanObject.name.Contains("Mountain"))
                            {
                                vertices[i].y += mountainHeightPower * booleanEffectStrength;
                            }
                            else if (booleanObject.name.Contains("Lake") || booleanObject.name.Contains("River"))
                            {
                                vertices[i].y -= effect * booleanEffectStrength;
                                if (vertices[i].y < lakeHeightMin)
                                    vertices[i].y = lakeHeightMin;
                            }
                            else if (booleanObject.name.Contains("Load"))
                            {
                                vertices[i].y -= effect * booleanEffectStrength;
                                if (vertices[i].y < loadHeightMin)
                                    vertices[i].y = loadHeightMin;
                            }
                        }
                    }
                }

                // FPSに基づいた処理の頻度を調整
                frameCounter++;
                totalTime += Time.deltaTime;

                if (frameCounter % 10 == 0)
                {
                    float currentFPS = frameCounter / totalTime; // 現在のFPSを計算
                    print(currentFPS);
                    if (currentFPS < fpsTarget)
                    {
                        stepSize = Mathf.Min(stepSize + 50, 1000); // FPSが低い場合はステップ数を増やして処理を軽減
                    }
                    else
                    {
                        stepSize = Mathf.Max(stepSize - 50, 10); // FPSが高い場合はステップ数を減らして処理を増加
                    }

                    // リセット
                    frameCounter = 0;
                    totalTime = 0f;
                }

                if (i % stepSize == 0)
                {
                    yield return null; // FPSの調整に応じて処理を中断
                }
            }
        }

        UpdateMesh();
        GenerateHeightMap();
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
        meshCollider.sharedMesh = null; // メッシュコライダーの更新
        meshCollider.sharedMesh = mesh;
    }

    // メッシュを滑らかにする関数
    private void SmoothMesh(int iterations)
    {
        for (int iter = 0; iter < iterations; iter++)
        {
            Vector3[] newVertices = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 currentVertex = vertices[i];
                float averageY = 0f;
                int neighborCount = 0;

                for (int zOffset = -1; zOffset <= 1; zOffset++)
                {
                    for (int xOffset = -1; xOffset <= 1; xOffset++)
                    {
                        if (xOffset == 0 && zOffset == 0) continue;

                        int neighborX = i % (gridSize + 1) + xOffset;
                        int neighborZ = i / (gridSize + 1) + zOffset;

                        if (neighborX >= 0 && neighborX <= gridSize && neighborZ >= 0 && neighborZ <= gridSize)
                        {
                            int neighborIndex = neighborZ * (gridSize + 1) + neighborX;
                            averageY += vertices[neighborIndex].y;
                            neighborCount++;
                        }
                    }
                }

                if (neighborCount > 0)
                {
                    float smoothedY = Mathf.Lerp(currentVertex.y, averageY / neighborCount, 0.5f);
                    newVertices[i] = new Vector3(currentVertex.x, smoothedY, currentVertex.z);
                }
                else
                {
                    newVertices[i] = currentVertex;
                }
            }

            vertices = newVertices;
        }

        UpdateMesh();
    }
}
