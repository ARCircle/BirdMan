using UnityEngine;

public class ObjectDisplayManager : MonoBehaviour
{
    public float rotationSpeed = 30f;  // 回転速度
    public Camera displayCamera;       // オブジェクトを表示するカメラ
    public float scalePadding = 1.2f;  // カメラに収まる余裕（倍率）

    public GameObject oldDisplayObject = null;

    void Update()
    {
        // カメラの親オブジェクトを回転させる
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    public void Display(GameObject newDisplayObject)
    {
        if (oldDisplayObject != null)
            Destroy(oldDisplayObject);

        // newDisplayObjectとその子オブジェクトにレイヤーを適用
        SetLayerRecursively(newDisplayObject, LayerMask.NameToLayer("ObjectDisplay"));

        // 子オブジェクトも含むすべてのレンダラーを取得
        Renderer[] renderers = newDisplayObject.GetComponentsInChildren<Renderer>();

        // Rigidbodyを持っている場合はisKinematicに設定
        Rigidbody rb = newDisplayObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        newDisplayObject.transform.position = Vector3.zero;
        newDisplayObject.transform.rotation = Quaternion.identity;

        // バウンド全体を計算するための初期値
        Bounds combinedBounds = new Bounds();
        int i = 0;

        foreach (Renderer renderer in renderers)
        {
            print(i + renderer.name + renderer.bounds);
            i++;
            combinedBounds.Encapsulate(renderer.bounds); // 各レンダラーのバウンドを結合

            // MaterialPropertyBlockを使用して _UseRange を設定
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);  // 現在のプロパティブロックを取得
            mpb.SetFloat("_UseRange", 0.0f); // 各オブジェクトごとに設定（例: 無効化）
            renderer.SetPropertyBlock(mpb);  // プロパティブロックを設定
        }

        Vector3 boundsSize = combinedBounds.size;
        print(boundsSize);

        // オブジェクトのスケールを調整してカメラ内に収める
        float maxBoundSize = Mathf.Max(boundsSize.x, boundsSize.y, boundsSize.z);
        float distanceToCamera = Vector3.Distance(newDisplayObject.transform.position, displayCamera.transform.position);
        float fieldOfViewInRadians = displayCamera.fieldOfView * Mathf.Deg2Rad;
        float desiredScale = (distanceToCamera * Mathf.Tan(fieldOfViewInRadians / 2)) / maxBoundSize;

        newDisplayObject.transform.localScale = Vector3.one * desiredScale * scalePadding; // スケール調整

        // バウンドの中心をワールドの原点に合わせる
        newDisplayObject.transform.position = Vector3.zero;
        Bounds combinedBounds2 = new Bounds();

        foreach (Renderer renderer in renderers)
        {
            combinedBounds2.Encapsulate(renderer.bounds); // 各レンダラーのバウンドを再度結合
        }

        Vector3 boundsCenter = combinedBounds2.center;
        print(boundsCenter);
        newDisplayObject.transform.position -= boundsCenter;

        oldDisplayObject = newDisplayObject;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer); // 子オブジェクトにも適用
        }
    }
}
