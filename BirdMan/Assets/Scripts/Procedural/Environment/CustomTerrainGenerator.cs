using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
//[CustomEditor(typeof(CustomTerrainGenerator))]
public class CustomTerrainGenerator : MonoBehaviour
{
    public int width = 100; // メッシュの幅
    public int height = 100; // メッシュの高さ
    public Material voronoiMaterial; // Voronoiノイズ用のマテリアル
    public RenderTexture voronoiRenderTexture; // ノイズを生成するためのRender Texture

    public void GenerateMesh()
    {
        if (voronoiRenderTexture == null || voronoiMaterial == null)
        {
            Debug.LogError("Render Texture or Material is not assigned.");
            return;
        }

        // Render Textureからテクスチャ情報を取得
        Texture2D densityMap = new Texture2D(voronoiRenderTexture.width, voronoiRenderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = voronoiRenderTexture;
        densityMap.ReadPixels(new Rect(0, 0, voronoiRenderTexture.width, voronoiRenderTexture.height), 0, 0);
        densityMap.Apply();
        RenderTexture.active = null;

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        int[] triangles = new int[width * height * 6];
        Vector2[] uv = new Vector2[(width + 1) * (height + 1)];

        for (int i = 0, z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float u = (float)x / width;
                float v = (float)z / height;

                // テクスチャから密度情報を取得
                Color pixelColor = densityMap.GetPixelBilinear(u, v);
                float density = pixelColor.grayscale; // 輝度を使用して密度を決定

                // 密度に基づいて頂点の高さを設定
                float heightOffset = density * 10.0f; // 任意のスケールで高さを調整

                vertices[i] = new Vector3(x, heightOffset, z);
                uv[i] = new Vector2(u, v);
            }
        }

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

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
    /*
    [CustomEditor(typeof(CustomTerrainGenerator))]
    public class CustomTerrainGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CustomTerrainGenerator generator = (CustomTerrainGenerator)target;

            if (GUILayout.Button("Generate Mesh"))
            {
                generator.GenerateMesh();
            }
        }
    }*/
}
