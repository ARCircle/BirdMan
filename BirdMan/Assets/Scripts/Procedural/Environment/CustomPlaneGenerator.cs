/*using UnityEngine;
using UnityEditor; // エディタ関連の機能を使うために必要

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[CustomEditor(typeof(CustomPlaneGenerator))] // エディタクラスを同じファイル内で定義
public class CustomPlaneGenerator : MonoBehaviour
{
    public int width = 100; // X方向の頂点数
    public int height = 100; // Z方向の頂点数

    // メッシュを生成するメソッドをパブリックにする
    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        int[] triangles = new int[width * height * 6];
        Vector2[] uv = new Vector2[(width + 1) * (height + 1)];

        for (int i = 0, z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                vertices[i] = new Vector3(x, 0, z);
                uv[i] = new Vector2((float)x / width, (float)z / height);
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
    // インスペクタ上のボタンを追加するためのエディタ部分
    [CustomEditor(typeof(CustomPlaneGenerator))]
    public class CustomPlaneGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // デフォルトのインスペクタの描画
            DrawDefaultInspector();

            // CustomPlaneGeneratorのインスタンスを取得
            CustomPlaneGenerator generator = (CustomPlaneGenerator)target;

            // ボタンをインスペクタに追加
            if (GUILayout.Button("Generate Mesh"))
            {
                // ボタンが押されたときにメッシュ生成メソッドを呼び出す
                generator.GenerateMesh();
            }
        }
    }
}*/
