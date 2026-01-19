using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorblindRendererFeature : ScriptableRendererFeature
{
#pragma warning disable CS0672
    class ColorblindPass : ScriptableRenderPass
    {
        private readonly Material _material;
        private RTHandle _source;
        private RTHandle _tempTexture;

        public ColorblindPass(Material material)
        {
            _material = material;
        }

        public void Setup(RTHandle source)
        {
            _source = source;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateHandleIfNeeded(
                ref _tempTexture,
                desc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_ColorblindTempTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null)
                return;

            var cmd = CommandBufferPool.Get("Colorblind Pass");

            Blitter.BlitCameraTexture(cmd, _source, _tempTexture, _material, 0);
            Blitter.BlitCameraTexture(cmd, _tempTexture, _source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }
#pragma warning restore CS0672

    [System.Serializable]
    public class Settings
    {
        public Material colorblindMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public Settings settings = new Settings();
    private ColorblindPass _pass;

    public override void Create()
    {
        if (settings.colorblindMaterial == null)
        {
            Debug.LogWarning("ColorblindRendererFeature : aucun material assigné.");
            return;
        }

        _pass = new ColorblindPass(settings.colorblindMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_pass == null)
            return;

#pragma warning disable CS0618
        _pass.Setup(renderer.cameraColorTargetHandle);
#pragma warning restore CS0618

        renderer.EnqueuePass(_pass);
    }
}