using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralRockGenerator : MonoBehaviour
{
    public float rockRadius = 1f; // ��{�̊�̔��a
    public int subdivisions = 3;  // ���̂̍ו������x��
    public float noiseStrength = 0.3f; // �m�C�Y�̋���
    public bool flattenBelowY = true;  // Y=0�ȉ��𕽂�ɂ��邩�ǂ����̃t���O

    private MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        GenerateRock();
    }

    void GenerateRock()
    {
        // ��{�ƂȂ鋅�̂𐶐�
        Mesh rockMesh = CreateIcoSphere(rockRadius, subdivisions);

        // ���_�Ƀm�C�Y�������Ċ�炵���`���
        Vector3[] vertices = rockMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            // ���_�Ƀ����_���ȃm�C�Y��������
            vertices[i] += vertices[i].normalized * Random.Range(-noiseStrength, noiseStrength);

            // Y=0�ȉ��̒��_�𕽂�ɂ��鏈��
            if (flattenBelowY && vertices[i].y < 0)
            {
                vertices[i].y = -0.1f; // Y=0�ȉ��̕����𕽂�ɂ���
            }
        }

        // ���b�V���ɒ��_�𔽉f
        rockMesh.vertices = vertices;
        rockMesh.RecalculateNormals();
        rockMesh.RecalculateBounds();

        // ���b�V����MeshFilter�ɐݒ�
        meshFilter.mesh = rockMesh;
    }

    // ����\�ʑ̃x�[�X�̋��̂𐶐�
    Mesh CreateIcoSphere(float radius, int subdivision)
    {
        Mesh mesh = new Mesh();
        IcoSphereCreator.Create(radius, subdivision, ref mesh);
        return mesh;
    }
}

public static class IcoSphereCreator
{
    // ����\�ʑ̃x�[�X�̋��̂��쐬���郁�\�b�h
    public static void Create(float radius, int subdivisions, ref Mesh mesh)
    {
        // ����\�ʑ̂̏������_�ƎO�p�`��ݒ�
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        InitializeIcosahedron(ref vertices, ref triangles);

        // �w�肳�ꂽ�񐔂����ו������s��
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

        // ���_�𐳋K�����Ĕ��a��K�p
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = vertices[i].normalized * radius;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    // ����\�ʑ̂̏�����
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

    // ���_�̒��ԓ_���擾����
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
