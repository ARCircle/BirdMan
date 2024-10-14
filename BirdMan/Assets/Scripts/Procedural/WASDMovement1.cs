using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class WASDMovement1 : MonoBehaviour
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
    public float jumpForceAmount2 = 2f; // 加える力の強さ
    Quaternion targetRotation;
    public bool isLookReversal = false;
    void Update()
    {
        if (!Application.isPlaying) return;
        Vector3 movement = Vector3.zero;

        // 既存の移動コード
        if (Input.GetKey(KeyCode.W)) movement += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) movement += Vector3.back;
        if (Input.GetKey(KeyCode.A)) movement += Vector3.left;
        if (Input.GetKey(KeyCode.D)) movement += Vector3.right;

        if (rb.velocity.magnitude < maxVelocity)
            rb.AddForce(movement * forceAmount);

        if (Input.GetKeyDown(KeyCode.Space))
            rb.AddForce(Vector3.up * jumpForceAmount, ForceMode.Impulse);

        // 膝下オブジェクトにぶつかったときに上に跳ねる処理
        RaycastHit hitLow, hitHigh;
        Vector3 startLow = transform.position + Vector3.up * 0.1f; // 低い位置からのRay
        Vector3 startHigh = transform.position + Vector3.up * 0.6f; // 高い位置からのRay
        Vector3 direction = body1.transform.forward;

        // Rayを描画
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
    public LineRenderer lineRendererLow; // 低い位置のRay用
    public LineRenderer lineRendererHigh; // 高い位置のRay用
    public float rayDistance = 1f;

  

}
