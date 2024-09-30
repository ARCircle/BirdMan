using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class WASDMovement1 : MonoBehaviour
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
    public float jumpForceAmount2 = 2f; // ������͂̋���
    Quaternion targetRotation;
    public bool isLookReversal = false;
    void Update()
    {
        if (!Application.isPlaying) return;
        Vector3 movement = Vector3.zero;

        // �����̈ړ��R�[�h
        if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) movement += Vector3.back;
        if (Input.GetKey(KeyCode.A)) movement += Vector3.left;
        if (Input.GetKey(KeyCode.D)) movement += Vector3.right;

        if (rb.velocity.magnitude < maxVelocity)
            rb.AddForce(movement * forceAmount);

        if (Input.GetKeyDown(KeyCode.Space))
            rb.AddForce(Vector3.up * jumpForceAmount, ForceMode.Impulse);

        // �G���I�u�W�F�N�g�ɂԂ������Ƃ��ɏ�ɒ��˂鏈��
        RaycastHit hitLow, hitHigh;
        Vector3 startLow = transform.position + Vector3.up * 0.1f; // �Ⴂ�ʒu�����Ray
        Vector3 startHigh = transform.position + Vector3.up * 0.6f; // �����ʒu�����Ray
        Vector3 direction = body1.transform.forward;

        // Ray��`��
        lineRendererLow.positionCount = 2;
        lineRendererLow.SetPosition(0, startLow);
        lineRendererLow.SetPosition(1, startLow + direction * rayDistance);

        lineRendererHigh.positionCount = 2;
        lineRendererHigh.SetPosition(0, startHigh);
        lineRendererHigh.SetPosition(1, startHigh + direction * rayDistance);

        if (Physics.Raycast(startLow, direction, out hitLow, rayDistance))// && Physics.Raycast(startHigh, direction, out hitHigh, rayDistance)
        {
            rb.AddForce(Vector3.up * jumpForceAmount2);
        }

        if (movement != Vector3.zero)
        {
            Vector3 moveDirection = movement.normalized;
            targetRotation = Quaternion.LookRotation(moveDirection);
            body1.transform.rotation = Quaternion.Lerp(body1.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    public LineRenderer lineRendererLow; // �Ⴂ�ʒu��Ray�p
    public LineRenderer lineRendererHigh; // �����ʒu��Ray�p
    public float rayDistance = 1f;

  

}
