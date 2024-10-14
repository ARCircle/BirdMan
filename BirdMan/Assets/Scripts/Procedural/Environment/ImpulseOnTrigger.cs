using UnityEngine;

public class ImpulseOnTrigger : MonoBehaviour
{
    public float impulseForce = 10f; // z方向に与えるインパルスの強さ
    public Rigidbody rb;
    private void OnTriggerEnter(Collider other)
    {
        // トリガーに入ったオブジェクトがRigidbodyを持っているか確認
       
            // Rigidbodyにz方向にImpulseを与える
            rb.AddForce(new Vector3(0, 0, impulseForce), ForceMode.Impulse);
        
    }
}
