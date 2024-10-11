using UnityEngine;

public class BridgeGenerator : MonoBehaviour
{
    public Transform startPoint; // ���̊J�n�n�_
    public Transform endPoint;   // ���̏I���n�_
    public GameObject segmentPrefab; // �Z�O�����g�̃v���n�u
    public float segmentSpacing = 0.5f; // �Z�O�����g�Ԃ̃X�y�[�X
    public float springForce = 100f; // �o�l�̗�
    public float damper = 5f; // �_���p�[�̒l
    public float upwardForce = 10f; // ������̌Œ��

    void Start()
    {
        if (startPoint == null || endPoint == null || segmentPrefab == null)
        {
            Debug.LogError("StartPoint, EndPoint, or SegmentPrefab is not assigned.");
            return;
        }

        GenerateBridge();
    }

    void GenerateBridge()
    {
        // �J�n�n�_����I���n�_�܂ł̋����ƕ������v�Z
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float totalDistance = Vector3.Distance(startPoint.position, endPoint.position);

        // �Z�O�����g�̃X�P�[�����擾���Ax����z���̂����������ق��̒l��I��
        float segmentLength = Mathf.Min(segmentPrefab.transform.localScale.x, segmentPrefab.transform.localScale.z);

        // �Z�O�����g�̐����v�Z�i�X�y�[�X���܂ށj
        int numberOfSegments = Mathf.FloorToInt(totalDistance / (segmentLength + segmentSpacing));

        GameObject lastSegment = null;
        // �e�Z�O�����g�𐶐����Ĕz�u
        for (int i = 0; i <= numberOfSegments; i++)
        {
            Vector3 segmentPosition = startPoint.position + direction * i * (segmentLength + segmentSpacing);

            // �Z�O�����g�̐���
            GameObject segment = Instantiate(segmentPrefab, segmentPosition, Quaternion.identity);
            segment.transform.rotation = Quaternion.LookRotation(direction); // �����𒲐�

            // Rigidbody�̐ݒ�
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = segment.AddComponent<Rigidbody>();
                rb.mass = 1f; // ���ʂ𒲐��i�K�v�ɉ����āj
            }

            // ������̗͂�������
         //    ConstantForce cf = segment.AddComponent<ConstantForce>();
         //   cf.force = new Vector3(0, upwardForce, 0);

            // Hinge Joint�̐ݒ�
            HingeJoint hinge = segment.AddComponent<HingeJoint>();
            if (i == 0)
            {
                //hinge.connectedBody = startPoint.GetComponent<Rigidbody>();
            }
            else
            {
                hinge.connectedBody = lastSegment.GetComponent<Rigidbody>();
            }

            // �Ō�̃Z�O�����g�͏I���n�_�ɌŒ�
            if (i == numberOfSegments)
            {
             // hinge.connectedBody = endPoint.GetComponent<Rigidbody>();
                // x, y, z���̈ړ����Œ�
                rb.constraints = RigidbodyConstraints.FreezePosition;
            }
            lastSegment = segment;

            // �X�v�����O�̐ݒ�
            JointSpring spring = new JointSpring();
            spring.spring = springForce;
            spring.damper = damper;
            hinge.spring = spring;
            hinge.useSpring = true;

            // ���̃Z�O�����g��e�I�u�W�F�N�g�̎q�ɂ���
            segment.transform.parent = transform;
        }

        startPoint.gameObject.SetActive(false);
        endPoint.gameObject.SetActive(false);
        segmentPrefab.SetActive(false);
    }
}
