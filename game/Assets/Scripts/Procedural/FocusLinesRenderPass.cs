using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FocusLinesRenderPass : ScriptableRenderPass
{
    private Material focusLinesMaterial;
    private RenderTargetHandle tempTextureHandle;

    public FocusLinesRenderPass(Material material)
    {
        focusLinesMaterial = material;
        tempTextureHandle.Init("_TemporaryColorTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("FocusLinesEffect");

        // �J�����̃^�[�Q�b�g�e�N�X�`�����擾
        RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;

        RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
        cmd.GetTemporaryRT(tempTextureHandle.id, opaqueDesc);

        // Blit �ŃG�t�F�N�g��K�p
        Blit(cmd, source, tempTextureHandle.Identifier(), focusLinesMaterial);
        Blit(cmd, tempTextureHandle.Identifier(), source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempTextureHandle.id);
    }
}
