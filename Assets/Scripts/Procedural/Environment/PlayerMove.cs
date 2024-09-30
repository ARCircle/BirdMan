using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public List<Material> targetMaterials; // 設定したいマテリアルのリストをInspectorで指定

    public float speed = 5f; // 移動速度
    public float displayRange = 10.0f; // 表示範囲

    void Update()
    {
        // z軸方向に一定速度で移動
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Playerのワールド座標をシェーダーに渡す
        Vector3 playerPosition = transform.position;

        // 各マテリアルにプレイヤーの位置と範囲を設定
        foreach (Material material in targetMaterials)
        {
            if (material != null)
            {
                material.SetVector("_PlayerPosition", playerPosition);
                material.SetFloat("_Range", displayRange);
            }
        }
    }
}
