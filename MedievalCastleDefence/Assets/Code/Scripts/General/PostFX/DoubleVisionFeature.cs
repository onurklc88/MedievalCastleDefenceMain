using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DoubleVisionFeature : ScriptableRendererFeature
{
    class DoubleVisionPass : ScriptableRenderPass
    {
        private Material material;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
        public float offset;
        public float intensity;

        public DoubleVisionPass(Material material)
        {
            this.material = material;
            tempTexture.Init("_TempDoubleVisionTexture");
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null || intensity <= 0.01f)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("Double Vision Effect");

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTexture.id, opaqueDesc);

            material.SetFloat("_Offset", offset);
            material.SetFloat("_Intensity", intensity);

            Blit(cmd, source, tempTexture.Identifier(), material, 0);
            Blit(cmd, tempTexture.Identifier(), source);

            cmd.ReleaseTemporaryRT(tempTexture.id);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material material = null;
        public float offset = 0.02f;
        [Range(0, 1)] public float intensity = 0.5f;
    }

    public Settings settings = new Settings();
    DoubleVisionPass pass;

    public override void Create()
    {
        pass = new DoubleVisionPass(settings.material);
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
            return;

        pass.offset = settings.offset;
        pass.intensity = settings.intensity;
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }
}