using UnityEngine;

public class ImpulseOnTrigger : MonoBehaviour
{
    public float impulseForce = 10f; // z�����ɗ^����C���p���X�̋���
    public Rigidbody rb;
    private void OnTriggerEnter(Collider other)
    {
        // �g���K�[�ɓ������I�u�W�F�N�g��Rigidbody�������Ă��邩�m�F
       
            // Rigidbody��z������Impulse��^����
            rb.AddForce(new Vector3(0, 0, impulseForce), ForceMode.Impulse);
        
    }
}
