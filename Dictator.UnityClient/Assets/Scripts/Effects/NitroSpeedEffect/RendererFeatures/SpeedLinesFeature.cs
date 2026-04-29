using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class SpeedLinesFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material SpeedLinesMaterial;
        public RenderPassEvent InjectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings FeatureSettings = new Settings();

    private SpeedLinesPass _pass;

    public override void Create()
    {
        _pass = new SpeedLinesPass(FeatureSettings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (FeatureSettings.SpeedLinesMaterial == null)
            return;

        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        renderer.EnqueuePass(_pass);
    }

    private class SpeedLinesPass : ScriptableRenderPass
    {
        private readonly Settings _settings;
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        public SpeedLinesPass(Settings settings)
        {
            _settings = settings;
            renderPassEvent = settings.InjectionPoint;
            requiresIntermediateTexture = true;
        }

        private class PassData
        {
            public TextureHandle Source;
            public Material Material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_settings.SpeedLinesMaterial == null)
                return;

            float intensity = _settings.SpeedLinesMaterial.GetFloat(IntensityId);
            if (intensity <= 0f)
                return;

            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
                return;

            var sourceTexture = resourceData.activeColorTexture;

            var descriptor = renderGraph.GetTextureDesc(sourceTexture);
            descriptor.name = "_SpeedLinesTempTex";
            descriptor.clearBuffer = false;

            var tempTexture = renderGraph.CreateTexture(descriptor);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SpeedLines Pass", out var passData))
            {
                passData.Source = sourceTexture;
                passData.Material = _settings.SpeedLinesMaterial;

                builder.UseTexture(sourceTexture, AccessFlags.Read);
                builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), data.Material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("SpeedLines Composite", out var passData))
            {
                passData.Source = tempTexture;
                passData.Material = null;

                builder.UseTexture(tempTexture, AccessFlags.Read);
                builder.SetRenderAttachment(sourceTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.Source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }
    }
}