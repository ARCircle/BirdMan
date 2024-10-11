/*using UnityEngine;
using UnityEditor; // �G�f�B�^�֘A�̋@�\���g�����߂ɕK�v

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[CustomEditor(typeof(CustomPlaneGenerator))] // �G�f�B�^�N���X�𓯂��t�@�C�����Œ�`
public class CustomPlaneGenerator : MonoBehaviour
{
    public int width = 100; // X�����̒��_��
    public int height = 100; // Z�����̒��_��

    // ���b�V���𐶐����郁�\�b�h���p�u���b�N�ɂ���
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
    // �C���X�y�N�^��̃{�^����ǉ����邽�߂̃G�f�B�^����
    [CustomEditor(typeof(CustomPlaneGenerator))]
    public class CustomPlaneGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // �f�t�H���g�̃C���X�y�N�^�̕`��
            DrawDefaultInspector();

            // CustomPlaneGenerator�̃C���X�^���X���擾
            CustomPlaneGenerator generator = (CustomPlaneGenerator)target;

            // �{�^�����C���X�y�N�^�ɒǉ�
            if (GUILayout.Button("Generate Mesh"))
            {
                // �{�^���������ꂽ�Ƃ��Ƀ��b�V���������\�b�h���Ăяo��
                generator.GenerateMesh();
            }
        }
    }
}*/
