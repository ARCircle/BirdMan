using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PathGenerator pathGenerator;
    public float moveSpeed = 5f; // �v���C���[�̈ړ����x

    private Vector3[] pathPoints;
    private int currentPointIndex = 0;

    void Start()
    {
        // PathGenerator���瓹�̃|�C���g���擾
        pathPoints = pathGenerator.GetPathPoints();
    }

    void Update()
    {
        if (pathPoints == null || pathPoints.Length == 0) return;

        // ���݂̃^�[�Q�b�g�|�C���g�Ɍ������Ĉړ�
        Vector3 targetPoint = pathPoints[currentPointIndex];
        Vector3 direction = (targetPoint - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // �^�[�Q�b�g�|�C���g�ɋ߂Â����玟�̃|�C���g�ɐi��
        if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
        {
            currentPointIndex++;

            if (currentPointIndex >= pathPoints.Length)
            {
                // �I�_�ɒB�������~
                currentPointIndex = pathPoints.Length - 1;
            }
        }
    }
}
