using UnityEngine;

public class FocusLineTrigger : MonoBehaviour
{
    public Material focusLineMaterial;  // 集中線シェーダーを使っているマテリアルを指定

    void Start()
    {
        // 初期状態では集中線を非表示に設定
        SetFocusLineVisible(false);
    }

    // トリガーに侵入した際に集中線を表示
    void OnTriggerEnter(Collider other)
    {
        SetFocusLineVisible(true); // 集中線を表示
    }

    // トリガーから出た際に集中線を非表示
    void OnTriggerExit(Collider other)
    {
        SetFocusLineVisible(false); // 集中線を非表示
    }

    // マテリアルの _ShowFocusLines を変更するメソッド
    private void SetFocusLineVisible(bool isVisible)
    {
        if (focusLineMaterial != null)
        {
            focusLineMaterial.SetFloat("_ShowFocusLines", isVisible ? 1.0f : 0.0f); // シェーダーのフラグを変更
        }
    }
}
