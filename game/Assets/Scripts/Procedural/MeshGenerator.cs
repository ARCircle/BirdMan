using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    public List<Vector3> controlPoints; // �X�v���C���̐���_�̃��X�g
    public List<float> radii; // �e�Z�N�V�����̔��a
    public bool showGizmos = true; // Gizmos �̕\���t���O
    public int segmentsPerCurve = 10; // �e�Z�O�����g������̍ו�����

    void Start()
    {
        GenerateMeshAndAssign();
    }

    void OnValidate()
    {
        // �G�f�B�^�[��ł̕ύX���Ƀ��b�V�����X�V
        if (gameObject.activeSelf && (transform.parent == null || transform.parent.gameObject.activeSelf))
            GenerateMeshAndAssign();
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
        List<Vector2> uvs = new List<Vector2>();

        if (controlPoints.Count < 2 || radii.Count < 2)
        {
            Debug.LogWarning("Not enough control points or radii to generate mesh.");
            return mesh;
        }

        int totalSegments = (controlPoints.Count - 1) * segmentsPerCurve;
        float totalLength = 0f;
        List<float> segmentLengths = new List<float>();

        // �Ȑ��̒������v�Z
        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            float length = Vector3.Distance(controlPoints[i], controlPoints[i + 1]);
            segmentLengths.Add(length);
            totalLength += length;
        }

        Vector3 prevTangent = Vector3.forward;
        Vector3 prevNormal = Vector3.up;

        int vertexCountPerRing = 16; // �~����̒��_��

        for (int i = 0; i < controlPoints.Count - 1; i++)
        {
            Vector3 p0 = controlPoints[i];
            Vector3 p1 = controlPoints[i + 1];
            float r0 = radii[i];
            float r1 = radii[i + 1];

            for (int j = 0; j <= segmentsPerCurve; j++)
            {
                float t = (float)j / segmentsPerCurve;
                Vector3 position = Vector3.Lerp(p0, p1, t);
                float radius = Mathf.Lerp(r0, r1, t);

                // �ڐ��itangent�j�̌v�Z
                Vector3 tangent = (p1 - p0).normalized;

                // �ŏ��̃t���[���̏ꍇ
                if (i == 0 && j == 0)
                {
                    prevTangent = tangent;
                    prevNormal = Vector3.up;

                    // tangent��up�x�N�g�������s�ȏꍇ��h��
                    if (Vector3.Dot(tangent, prevNormal) > 0.99f)
                    {
                        prevNormal = Vector3.right;
                    }
                }
                else
                {
                    // �V�����@���x�N�g�����v�Z
                    Quaternion rotation = Quaternion.FromToRotation(prevTangent, tangent);
                    prevNormal = rotation * prevNormal;
                    prevTangent = tangent;
                }

                Vector3 binormal = Vector3.Cross(prevNormal, tangent).normalized;
                Vector3 normal = Vector3.Cross(tangent, binormal).normalized;

                // �~����̒��_���v�Z
                for (int k = 0; k < vertexCountPerRing; k++)
                {
                    float angle = (float)k / vertexCountPerRing * Mathf.PI * 2f;
                    Vector3 radialDirection = Mathf.Cos(angle) * normal + Mathf.Sin(angle) * binormal;
                    Vector3 vertexPosition = position + radialDirection * radius;
                    vertices.Add(vertexPosition);
                    uvs.Add(new Vector2((float)k / vertexCountPerRing, totalLength));
                }

                // �O�p�`�̍쐬
                if (i > 0 || j > 0)
                {
                    int baseIndex = vertices.Count - vertexCountPerRing * 2;
                    for (int k = 0; k < vertexCountPerRing; k++)
                    {
                        int current = baseIndex + k;
                        int next = baseIndex + (k + 1) % vertexCountPerRing;
                        int currentNext = current + vertexCountPerRing;
                        int nextNext = next + vertexCountPerRing;

                        triangles.Add(current);
                        triangles.Add(currentNext);
                        triangles.Add(nextNext);

                        triangles.Add(current);
                        triangles.Add(nextNext);
                        triangles.Add(next);
                    }
                }
            }
        }

        // �n�_�ƏI�_�ɃL���b�v��ǉ�
        Vector3 startDirection = (controlPoints[1] - controlPoints[0]).normalized;
        AddCap(vertices, triangles, uvs, controlPoints[0], -startDirection, radii[0], vertexCountPerRing, true);

        Vector3 endDirection = (controlPoints[controlPoints.Count - 1] - controlPoints[controlPoints.Count - 2]).normalized;
        AddCap(vertices, triangles, uvs, controlPoints[controlPoints.Count - 1], endDirection, radii[radii.Count - 1], vertexCountPerRing, false);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void AddCap(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, Vector3 center, Vector3 direction, float radius, int vertexCountPerRing, bool isStart)
    {
        Vector3 normal = isStart ? -direction.normalized : direction.normalized;
        int centerIndex = vertices.Count;
        vertices.Add(center);
        uvs.Add(new Vector2(0.5f, 0.5f));

        Vector3 binormal = Vector3.Cross(normal, Vector3.up).normalized;
        if (binormal == Vector3.zero)
            binormal = Vector3.Cross(normal, Vector3.right).normalized;
        Vector3 tangent = Vector3.Cross(binormal, normal).normalized;

        for (int i = 0; i < vertexCountPerRing; i++)
        {
            float angle = (float)i / vertexCountPerRing * Mathf.PI * 2f;
            Vector3 radialDirection = Mathf.Cos(angle) * tangent + Mathf.Sin(angle) * binormal;
            Vector3 vertexPosition = center + radialDirection * radius;
            vertices.Add(vertexPosition);
            uvs.Add(new Vector2((Mathf.Cos(angle) + 1f) * 0.5f, (Mathf.Sin(angle) + 1f) * 0.5f));
        }

        for (int i = 0; i < vertexCountPerRing; i++)
        {
            int current = centerIndex + 1 + i;
            int next = centerIndex + 1 + (i + 1) % vertexCountPerRing;

            if (isStart)
            {
                triangles.Add(centerIndex);
                triangles.Add(next);
                triangles.Add(current);
            }
            else
            {
                triangles.Add(centerIndex);
                triangles.Add(current);
                triangles.Add(next);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // ����_�Ɣ��a�̉���
        if (controlPoints == null || radii == null || controlPoints.Count < 2 || radii.Count < 2)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < controlPoints.Count; i++)
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(controlPoints[i]), radii[i]);
            if (i < controlPoints.Count - 1)
            {
                Gizmos.DrawLine(transform.TransformPoint(controlPoints[i]), transform.TransformPoint(controlPoints[i + 1]));
            }
        }
    }

    public void SwapControlPoints(int index1, int index2)
    {
        if (index1 < 0 || index2 < 0 || index1 >= controlPoints.Count || index2 >= controlPoints.Count)
            return;

        // Control Points�̓���ւ�
        Vector3 tempPoint = controlPoints[index1];
        controlPoints[index1] = controlPoints[index2];
        controlPoints[index2] = tempPoint;

        // Radii�̓���ւ�
        float tempRadius = radii[index1];
        radii[index1] = radii[index2];
        radii[index2] = tempRadius;

        // ���b�V�����Đ���
        GenerateMeshAndAssign();
    }

    // �V�����ǉ�����~���[�@�\
    public void MirrorControlPoints(string axis)
    {
        if (axis == "X")
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                controlPoints[i] = new Vector3(-controlPoints[i].x, controlPoints[i].y, controlPoints[i].z);
            }
        }
        else if (axis == "Y")
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                controlPoints[i] = new Vector3(controlPoints[i].x, -controlPoints[i].y, controlPoints[i].z);
            }
        }
        else if (axis == "Z")
        {
            for (int i = 0; i < controlPoints.Count; i++)
            {
                controlPoints[i] = new Vector3(controlPoints[i].x, controlPoints[i].y, -controlPoints[i].z);
            }
        }

        // ���b�V�����Đ���
        GenerateMeshAndAssign();
    }
}


