using UnityEngine;

[ExecuteInEditMode]
public class FocusLinesEffect : MonoBehaviour
{
    public Material focusLinesMaterial; // 集中線シェーダーを持つマテリアル
    public float lineCount = 20f;
    public float speed = 1f;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (focusLinesMaterial != null)
        {
            // シェーダーのパラメータを設定
            focusLinesMaterial.SetFloat("_LineCount", lineCount);
            focusLinesMaterial.SetFloat("_Speed", speed);
            print("s");
            // 画面全体にエフェクトを適用
            Graphics.Blit(src, dest, focusLinesMaterial);
        }
        else
        {
            // マテリアルが設定されていない場合、元のテクスチャをそのまま表示
            Graphics.Blit(src, dest);
        }
    }
}
