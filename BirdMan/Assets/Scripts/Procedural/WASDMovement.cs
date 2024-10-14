using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class WASDMovement : MonoBehaviour
{
    public float forceAmount = 35f; // 加える力の強さ
    public float maxVelocity = 15f;

    private Rigidbody rb;

    void Start()
    {
        // Rigidbodyコンポーネントを取得
        rb = GetComponent<Rigidbody>();
    }
    public GameObject body1;
    public float rotationSpeed = 5f; // 回転速度
    public float jumpForceAmount = 35f; // 加える力の強さ
    Quaternion targetRotation;
    public bool isLookReversal = false;
    void Update()
    {
        if (!Application.isPlaying)
        {
            // エディタモードでの処理
            return;
        }

        Vector3 movement = Vector3.zero;

        // WASDキー入力をチェックし、対応する方向に力を加える
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

        // Rigidbodyに力を加える
        if (rb.velocity.magnitude < maxVelocity)
        {
            rb.AddForce(movement * forceAmount);
        }
        if (Input.GetKeyDown (KeyCode.Space))
        {
    
            rb.AddForce(Vector3.up* jumpForceAmount,ForceMode.Impulse);
        }
        // オブジェクトを進行方向に回転させる
        // オブジェクトを進行方向に回転させる
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-movement);
            body1.transform.rotation = Quaternion.Lerp(body1.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (rb.velocity.y != 0)
        {
            // 進行方向に基づく目標回転を計算


            //if (rb.velocity.magnitude != 0) {
            if (!isLookReversal)
                targetRotation = Quaternion.LookRotation(-rb.velocity);
                else
                    targetRotation = Quaternion.LookRotation(rb.velocity);
            //}

            // 回転を補間
            Quaternion newRotation = Quaternion.Lerp(body1.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Euler角で制限を加える
            Vector3 eulerRotation = newRotation.eulerAngles;

            // X軸の回転を0度から45度の範囲にクランプ
            if (eulerRotation.x > 180f)
            {
                eulerRotation.x -= 360f;  // 360度を超える場合、負の角度に変換
            }

            eulerRotation.x = Mathf.Clamp(eulerRotation.x, 0f, 45f);  // 0度から45度の範囲にクランプ

            // 制限を加えた回転を再びQuaternionに変換して設定
            body1.transform.rotation = Quaternion.Euler(eulerRotation);
        }

    }
}
