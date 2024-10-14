using UnityEngine;

public class SmoothFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothTime = 0.3f; // カメラがスムーズに追従する時間

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        // ターゲット位置にオフセットを適用
        Vector3 targetPositionWithOffset = target.position + offset;

        // カメラの位置をターゲット位置に向かってスムーズに移動（SmoothDampを使用）
        transform.position = Vector3.SmoothDamp(transform.position, targetPositionWithOffset, ref velocity, smoothTime);
    }
}
