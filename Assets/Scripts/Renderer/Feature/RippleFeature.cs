using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

public class RippleFeature : ScriptableRendererFeature
{
    class RipplePass : ScriptableRenderPass
    {
        public Material rippleMaterial;
        RenderTargetHandle tempTexture;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;

            CommandBuffer cmd = CommandBufferPool.Get("RippleEffect");

            cmd.Blit(cameraColorTarget, tempTexture.Identifier(), rippleMaterial);
            cmd.Blit(tempTexture.Identifier(), cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    RipplePass _ripplePass;
    public Material rippleMaterial;

    public override void Create()
    {
        _ripplePass = new RipplePass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
            rippleMaterial = rippleMaterial
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_ripplePass);
    }
}
