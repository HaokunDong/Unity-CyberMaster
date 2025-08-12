using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

public class BladeFightEffectFeature : ScriptableRendererFeature
{
    class BladeFightEffectPass : ScriptableRenderPass
    {
        public Material material;
        RenderTargetHandle tempTexture;

        public BladeFightEffectPass()
        {
            tempTexture.Init("_TempBladeFightTexture");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
            ConfigureTarget(tempTexture.Identifier());
            ConfigureClear(ClearFlag.None, Color.clear);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("BladeFightEffect");

            var cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.Blit(cameraColorTarget, tempTexture.Identifier(), material);
            cmd.Blit(tempTexture.Identifier(), cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (tempTexture != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(tempTexture.id);
            }
        }
    }

    BladeFightEffectPass pass;
    public Material material;

    public override void Create()
    {
        pass = new BladeFightEffectPass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
            material = material,
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
