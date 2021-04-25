using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CoreLib.Render
{
    sealed class DistortionMapRenderPass : ScriptableRenderPass
    {
        static readonly string[] s_ShaderTags =
        {
            "UniversalForward",
            "LightweightForward",
            "SRPDefaultUnlit",
        };

        private ProfilingSampler  m_ProfilingSampler;
        private FilteringSettings m_OpaqueFilteringSettings;
        private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        private RenderStateBlock  m_RenderStateBlock;
        private string            m_Tag;

        private  RenderTargetHandle      m_DistortionRenderTarget;
        internal RenderTextureDescriptor m_DistortionRTDescriptor;
        private  ScriptableRenderer      m_Renderer;
        private  FilterMode              m_MapFilterMode;
        private  FilteringSettings       m_TransparentFilteringSettings;
        
        //////////////////////////////////////////////////////////////////////////
        public DistortionMapRenderPass(RenderPassEvent renderEvent, int layerMask, FilterMode filterMode, string tag)
        {
            m_Tag              = tag;
            m_ProfilingSampler = new ProfilingSampler(tag);
            m_MapFilterMode    = filterMode;

            renderPassEvent = renderEvent;

            m_OpaqueFilteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            m_RenderStateBlock            =  new RenderStateBlock(RenderStateMask.Nothing);
            m_RenderStateBlock.mask       |= RenderStateMask.Depth;
            m_RenderStateBlock.depthState =  new DepthState(false, CompareFunction.LessEqual);
        
            foreach (var passName in s_ShaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId(passName));
        }

        public void Setup(RenderTextureDescriptor cameraDescriptor, RenderTargetHandle distortionRenderTarget, ScriptableRenderer renderer)
        {
            // configure render target
            m_DistortionRenderTarget = distortionRenderTarget;
            m_Renderer = renderer;
            m_DistortionRTDescriptor = new RenderTextureDescriptor(cameraDescriptor.width, cameraDescriptor.height, DistortionFeature.MapFormat, 0, 0);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // set render target to distortion map
            cmd.GetTemporaryRT(m_DistortionRenderTarget.id, m_DistortionRTDescriptor, m_MapFilterMode);
            ConfigureTarget(m_DistortionRenderTarget.Identifier(), m_Renderer.cameraDepthTarget);
            ConfigureClear(ClearFlag.Color, Color.blue);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, SortingCriteria.CommonTransparent);

            // profiling command
            var cmd = CommandBufferPool.Get(m_Tag);

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_OpaqueFilteringSettings, ref m_RenderStateBlock);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_DistortionRenderTarget.id);
        }
    }
}