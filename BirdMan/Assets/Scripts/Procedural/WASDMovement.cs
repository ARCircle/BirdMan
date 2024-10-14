using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class WASDMovement : MonoBehaviour
{
    public float forceAmount = 35f; // ������͂̋���
    public float maxVelocity = 15f;

    private Rigidbody rb;

    void Start()
    {
        // Rigidbody�R���|�[�l���g���擾
        rb = GetComponent<Rigidbody>();
    }
    public GameObject body1;
    public float rotationSpeed = 5f; // ��]���x
    public float jumpForceAmount = 35f; // ������͂̋���
    Quaternion targetRotation;
    public bool isLookReversal = false;
    void Update()
    {
        if (!Application.isPlaying)
        {
            // �G�f�B�^���[�h�ł̏���
            return;
        }

        Vector3 movement = Vector3.zero;

        // WASD�L�[���͂��`�F�b�N���A�Ή���������ɗ͂�������
        if (Input.GetKey(KeyCode.W))
        {
            movement += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement += Vector3.right;
        }

        // Rigidbody�ɗ͂�������
        if (rb.velocity.magnitude < maxVelocity)
        {
            rb.AddForce(movement * forceAmount);
        }
        if (Input.GetKeyDown (KeyCode.Space))
        {
    
            rb.AddForce(Vector3.up* jumpForceAmount,ForceMode.Impulse);
        }
        // �I�u�W�F�N�g��i�s�����ɉ�]������
        // �I�u�W�F�N�g��i�s�����ɉ�]������
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-movement);
            body1.transform.rotation = Quaternion.Lerp(body1.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (rb.velocity.y != 0)
        {
            // �i�s�����Ɋ�Â��ڕW��]���v�Z


            //if (rb.velocity.magnitude != 0) {
            if (!isLookReversal)
                targetRotation = Quaternion.LookRotation(-rb.velocity);
                else
                    targetRotation = Quaternion.LookRotation(rb.velocity);
            //}

            // ��]����
            Quaternion newRotation = Quaternion.Lerp(body1.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Euler�p�Ő�����������
            Vector3 eulerRotation = newRotation.eulerAngles;

            // X���̉�]��0�x����45�x�͈̔͂ɃN�����v
            if (eulerRotation.x > 180f)
            {
                eulerRotation.x -= 360f;  // 360�x�𒴂���ꍇ�A���̊p�x�ɕϊ�
            }

            eulerRotation.x = Mathf.Clamp(eulerRotation.x, 0f, 45f);  // 0�x����45�x�͈̔͂ɃN�����v

            // ��������������]���Ă�Quaternion�ɕϊ����Đݒ�
            body1.transform.rotation = Quaternion.Euler(eulerRotation);
        }

    }
}
