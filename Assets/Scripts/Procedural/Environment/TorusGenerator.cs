using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TorusGenerator : MonoBehaviour
{
    public Vector2 radiusRange = new Vector2(0.5f, 2f); // トーラスの中心からチューブまでの距離の範囲
    public Vector2 tubeRadiusRange = new Vector2(0.1f, 0.5f); // チューブの半径の範囲
    public int radialSegments = 16; // 外周の分割数
    public int tubularSegments = 32; // チューブの分割数

    private float radius; // ランダムに生成されたトーラスの半径
    private float tubeRadius; // ランダムに生成されたチューブの半径
    public GameObject accelerator;
    public float acceleratorScale=1;
    void Start()
    {
        // ランダムに半径とチューブ半径を生成
        radius = Random.Range(radiusRange.x, radiusRange.y);
        tubeRadius = Random.Range(tubeRadiusRange.x, tubeRadiusRange.y);
        accelerator.transform.localScale *= radius*acceleratorScale;
        transform.rotation = Quaternion.Euler(90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        GenerateTorus();
    }

    void GenerateTorus()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Torus";

        // 頂点と三角形リストの初期化
        Vector3[] vertices = new Vector3[radialSegments * tubularSegments];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[radialSegments * tubularSegments * 6];

        // トーラスの頂点を計算
        for (int i = 0; i < radialSegments; i++)
        {
            float radialAngle = i * Mathf.PI * 2f / radialSegments;
            Vector3 radialCenter = new Vector3(Mathf.Cos(radialAngle), 0f, Mathf.Sin(radialAngle)) * radius;

            for (int j = 0; j < tubularSegments; j++)
            {
                float tubularAngle = j * Mathf.PI * 2f / tubularSegments;
                Vector3 offset = new Vector3(Mathf.Cos(tubularAngle) * tubeRadius, Mathf.Sin(tubularAngle) * tubeRadius, 0f);
                Vector3 vertex = radialCenter + Quaternion.AngleAxis(-radialAngle * Mathf.Rad2Deg, Vector3.up) * offset;

                int vertexIndex = i * tubularSegments + j;
                vertices[vertexIndex] = vertex;
                normals[vertexIndex] = (vertex - radialCenter).normalized;
                uv[vertexIndex] = new Vector2((float)i / radialSegments, (float)j / tubularSegments);
            }
        }

        // トーラスの三角形インデックスを計算
        int triangleIndex = 0;
        for (int i = 0; i < radialSegments; i++)
        {
            for (int j = 0; j < tubularSegments; j++)
            {
                int current = i * tubularSegments + j;
                int next = i * tubularSegments + (j + 1) % tubularSegments;
                int nextRow = ((i + 1) % radialSegments) * tubularSegments + j;
                int nextRowNext = ((i + 1) % radialSegments) * tubularSegments + (j + 1) % tubularSegments;

                // 三角形を追加
                triangles[triangleIndex++] = current;
              
                triangles[triangleIndex++] = nextRowNext;
                triangles[triangleIndex++] = nextRow;

                triangles[triangleIndex++] = current;
               
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = nextRowNext;
            }
        }

        // メッシュにデータを適用
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
       
        // メッシュフィルターにメッシュを割り当て
        meshFilter.mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
