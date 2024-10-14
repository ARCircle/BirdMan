using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothTime = 0.3f; // �J�������X���[�Y�ɒǏ]���鎞��

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        // �^�[�Q�b�g�ʒu�ɃI�t�Z�b�g��K�p
        Vector3 targetPositionWithOffset = target.position + offset;

        // �J�����̈ʒu���^�[�Q�b�g�ʒu�Ɍ������ăX���[�Y�Ɉړ��iSmoothDamp���g�p�j
        transform.position = Vector3.SmoothDamp(transform.position, targetPositionWithOffset, ref velocity, smoothTime);
    }
}
