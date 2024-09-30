using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    public int numberOfPoints = 10; // ���̃|�C���g��
    public CustomTerrainGenerator terrainGenerator; // �n�`�����X�N���v�g�ւ̎Q��
    public GameObject pathPrefab; // ����\�����邽�߂̃v���n�u

    private Vector3[] pathPoints;

    void Awake()
    {
        GeneratePath();
    }

    public void GeneratePath()
    {
        if (terrainGenerator == null)
        {
            Debug.LogError("Terrain Generator is not assigned.");
            return;
        }

        Vector3[] terrainVertices = terrainGenerator.GetComponent<MeshFilter>().mesh.vertices;
        pathPoints = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);

            // �x�W�G�Ȑ��܂��͑��̃A���S���Y�����g���āA����2D�ʒu���v�Z
            Vector3 pointOnTerrain = CalculateBezierPoint(t, new Vector3(0, 0, 0), new Vector3(25, 0, 25), new Vector3(75, 0, 75), new Vector3(100, 0, 100));

            // �n�`�̍����ɍ��킹��Y���W�𒲐�
            float height = GetTerrainHeightAtPoint(pointOnTerrain.x, pointOnTerrain.z, terrainVertices);
            pathPoints[i] = new Vector3(pointOnTerrain.x, height, pointOnTerrain.z);
        }

        CreatePathMesh();
    }

    // �n�`�̍������擾
    private float GetTerrainHeightAtPoint(float x, float z, Vector3[] terrainVertices)
    {
        // �ȒP�ȗ�Ƃ��āA�ł��߂����_�̍������擾
        Vector3 closestVertex = terrainVertices[0];
        float minDistance = Vector3.Distance(new Vector3(x, 0, z), new Vector3(closestVertex.x, 0, closestVertex.z));

        foreach (var vertex in terrainVertices)
        {
            float distance = Vector3.Distance(new Vector3(x, 0, z), new Vector3(vertex.x, 0, vertex.z));
            if (distance < minDistance)
            {
                minDistance = distance;
                closestVertex = vertex;
            }
        }

        return closestVertex.y;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // ��ꍀ
        p += 3 * uu * t * p1; // ���
        p += 3 * u * tt * p2; // ��O��
        p += ttt * p3; // ��l��

        return p;
    }

    private void CreatePathMesh()
    {
        for (int i = 0; i < pathPoints.Length; i++)
        {
            Instantiate(pathPrefab, pathPoints[i], Quaternion.identity);
        }
    }

    public Vector3[] GetPathPoints()
    {
        return pathPoints;
    }
}
