using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FocusLinesRenderFeature : ScriptableRendererFeature
{
    public Material focusLinesMaterial;
    public Color lineColor = Color.white;  // 集中線の色を指定
    public float lineLength = 1.0f;        // 集中線の長さを指定
            // 集中線の表示/非表示を管理するフラグ

    FocusLinesRenderPass focusLinesRenderPass;

    public override void Create()
    {
        focusLinesRenderPass = new FocusLinesRenderPass(focusLinesMaterial);
        focusLinesRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (focusLinesMaterial != null )  // isEnabledがtrueの場合にのみ描画
        {
            // シェーダーに色と長さをセット
            focusLinesMaterial.SetColor("_LineColor", lineColor);
            focusLinesMaterial.SetFloat("_LineLength", lineLength);

            renderer.EnqueuePass(focusLinesRenderPass);
        }
    }
}
