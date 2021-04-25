using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CoreLib.Render
{
    internal class DistortionDrawPass : ScriptableRenderPass
    {
        public  Material               m_BlitMaterial;
        private RenderTargetIdentifier m_Source;

        private RenderTargetHandle m_TemporaryColorTexture;
        private string             m_ProfilerTag;

        //////////////////////////////////////////////////////////////////////////
        public DistortionDrawPass(RenderPassEvent renderEvent, Material blitMaterial, string tag)
        {
            renderPassEvent = renderEvent;
            m_BlitMaterial  = blitMaterial;
            m_ProfilerTag   = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        }

        public void Setup(in RenderTargetIdentifier source)
        {
            m_Source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.depthBufferBits = 0;
            
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, cameraTextureDescriptor, FilterMode.Point);
            ConfigureTarget(m_TemporaryColorTexture.Identifier());
            ConfigureClear(ClearFlag.Color, Color.gray);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(m_ProfilerTag);

            // can't read and write to same color target, create a temp render target to blit. 
            Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier(), m_BlitMaterial, 0);
            Blit(cmd, m_TemporaryColorTexture.Identifier(), m_Source);

            // 
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }
}