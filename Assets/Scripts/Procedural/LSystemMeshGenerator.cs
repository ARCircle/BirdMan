using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemMeshGenerator : MonoBehaviour
{
    public int iterations = 1; // L-System�̔�����
    public float angleX = 25.0f; // X���̉�]�p�x
    public float segmentLength = 1.0f; // �Z�O�����g�̒���
    public float segmentRadius = 0.1f; // �Z�O�����g�̔��a
    public int segmentsPerCurve = 10; // �e�Z�O�����g������̍ו�����
    public int vertexCountPerRing = 8; // �~����̒��_��

    private LSystemMesh lSystem;
    private List<(Vector3, int)> controlPoints = new List<(Vector3, int)>(); // �����̐���_
    private List<float> radii = new List<float>(); // �e�Z�O�����g�̔��a
    private int branchCount = 1; // ����̃J�E���g

    public string F = "FF+[+F-F-F]"; // ���Ǝ}�̐����̃��[��
    string lSystemString;
    void Start()
    {
        // L-System�̃��[�����`
        Dictionary<char, string> rules = new Dictionary<char, string>
        {
            { 'F', F } // ���Ǝ}�̐���
        };

        // L-System�̏����ݒ�
        lSystem = new LSystemMesh("F", rules, iterations);

        // L-System�Ɋ�Â��Ė؂𐶐�
        lSystemString = lSystem.Generate();

      
    }

     public Mesh GenerateTreeMesh(GameObject mapObject)
    {
        Stack<Matrix4x4> transformStack = new Stack<Matrix4x4>();
        Matrix4x4 currentTransform = Matrix4x4.identity;

        // �V�[�����́uMap�v�^�O�������I�u�W�F�N�g���擾
       // GameObject[] mapObjects = GameObject.FindGameObjectsWithTag("Map");
        //print(mapObject);
        // L-System�̕����������
        foreach (char c in lSystemString)
        {
            if (c == 'F') // ���܂��͎}�̐���
            {
                Vector3 startPoint = currentTransform.MultiplyPoint(Vector3.zero);

                // �eMap�I�u�W�F�N�g��MeshCollider�ɑ΂���Ray���΂�
               
                    MeshCollider meshCollider = mapObject.GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        // ���݂̃|�C���g����Ray��Y�������i���j�ɔ�΂�
                        RaycastHit hit;
                     
                        if (Physics.Raycast(startPoint + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Map")))
                        {
                            // Ray�������ɏՓ˂�����A���̈ʒu��V���������_�ɐݒ�
                            startPoint = hit.point;
                           //print(hit.point+hit.transform.name);
                            //break; // ��̃}�b�v�Ƀq�b�g������A����ȏ�Ray���΂��Ȃ�
                        }
                    }
                

                // ����_��ǉ�
                controlPoints.Add((startPoint, 0));
                radii.Add(segmentRadius);

                // Z�����ɃZ�O�����g��i�߂�
                currentTransform *= Matrix4x4.Translate(Vector3.forward * segmentLength);
            }
            else if (c == '+') // X�������ɉ�]�i�E�����ɕ���j
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, angleX, 0));
            }
            else if (c == '-') // X�������ɉ�]�i�������ɕ���j
            {
                currentTransform *= Matrix4x4.Rotate(Quaternion.Euler(0, -angleX, 0));
            }
            else if (c == '[') // �X�^�b�N�Ɍ��݂̃g�����X�t�H�[����ۑ��i����̊J�n�j
            {
                transformStack.Push(currentTransform);
            }
            else if (c == ']') // �X�^�b�N����g�����X�t�H�[�������o���i����̏I���j
            {
                currentTransform = transformStack.Pop();
            }
        }

        // ���b�V���̐����ƃA�T�C��
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

        // ����_�ɉ��������b�V������
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

        // �O�p�`�̐����i���b�V���̖ʁj
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
