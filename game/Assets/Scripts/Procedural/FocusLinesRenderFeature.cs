using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FocusLinesRenderFeature : ScriptableRendererFeature
{
    public Material focusLinesMaterial;
    public Color lineColor = Color.white;  // �W�����̐F���w��
    public float lineLength = 1.0f;        // �W�����̒������w��
            // �W�����̕\��/��\�����Ǘ�����t���O

    FocusLinesRenderPass focusLinesRenderPass;

    public override void Create()
    {
        focusLinesRenderPass = new FocusLinesRenderPass(focusLinesMaterial);
        focusLinesRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (focusLinesMaterial != null )  // isEnabled��true�̏ꍇ�ɂ̂ݕ`��
        {
            // �V�F�[�_�[�ɐF�ƒ������Z�b�g
            focusLinesMaterial.SetColor("_LineColor", lineColor);
            focusLinesMaterial.SetFloat("_LineLength", lineLength);

            renderer.EnqueuePass(focusLinesRenderPass);
        }
    }
}
