using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CoreLib.Render
{
    /// <summary>
    /// Copy the given color buffer to the given destination color buffer.
    ///
    /// You can use this pass to copy a color buffer to the destination,
    /// so you can use it later in rendering. For example, you can copy
    /// the opaque texture to use it for distortion effects.
    /// </summary>
    internal class BlitPass : ScriptableRenderPass
    {
        public  Material               m_BlitMaterial        = null;
        public  int                    m_BlitShaderPassIndex = 0;
        public  FilterMode             m_FilterMode          = FilterMode.Point;
        private RenderTargetIdentifier m_Source;
        private RenderTargetHandle     m_Destination;

        private RenderTargetHandle m_TemporaryColorTexture;
        private string             m_ProfilerTag;

        //////////////////////////////////////////////////////////////////////////
        public enum RenderTarget
        {
            Color,
            RenderTexture,
        }

        //////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Create the CopyColorPass
        /// </summary>
        public BlitPass(RenderPassEvent renderPassEvent, Material blitMaterial, int blitShaderPassIndex, string tag)
        {
            this.renderPassEvent  = renderPassEvent;
            m_BlitMaterial        = blitMaterial;
            m_BlitShaderPassIndex = blitShaderPassIndex;
            m_ProfilerTag         = tag;
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
        }

        /// <summary>
        /// Configure the pass with the source and destination to execute on.
        /// </summary>
        /// <param name="source">Source Render Target</param>
        /// <param name="destination">Destination Render Target</param>
        public void Setup(ref RenderTargetIdentifier source, ref RenderTargetHandle destination)
        {
            m_Source      = source;
            m_Destination = destination;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(m_ProfilerTag);

            var opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // can't read and write to same color target, create a temp render target to blit. 
            if (m_Destination == RenderTargetHandle.CameraTarget)
            {
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, m_FilterMode);
                Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier(), m_BlitMaterial, m_BlitShaderPassIndex);
                Blit(cmd, m_TemporaryColorTexture.Identifier(), m_Source);
            }
            else
            {
                Blit(cmd, m_Source, m_Destination.Identifier(), m_BlitMaterial, m_BlitShaderPassIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (m_Destination == RenderTargetHandle.CameraTarget)
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
        }
    }
}