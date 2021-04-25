using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CoreLib.Render
{
    sealed class OutlineMapRenderPass : ScriptableRenderPass
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

        private  RenderTargetHandle      m_OutlineRenderTarget;
        internal RenderTextureDescriptor m_OutlineRTDescriptor;
        private  ScriptableRenderer      m_Renderer;
        private  FilterMode              m_MapFilterMode;
        private  FilteringSettings       m_TransparentFilteringSettings;
        private  Material                m_Material;
        private  Material                m_MaterialBlit;

        private  RenderTargetHandle      m_TemporaryRenderTarget;

        //////////////////////////////////////////////////////////////////////////
        public OutlineMapRenderPass(RenderPassEvent renderEvent, int layerMask, FilterMode filterMode, string tag)
        {
            m_Tag              = tag;
            m_ProfilingSampler = new ProfilingSampler(tag);
            m_MapFilterMode    = filterMode;
        
            renderPassEvent = renderEvent;

            m_OpaqueFilteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            m_RenderStateBlock            =  new RenderStateBlock(RenderStateMask.Nothing);
            m_RenderStateBlock.mask       |= RenderStateMask.Depth;
            m_RenderStateBlock.depthState =  new DepthState(false, CompareFunction.Equal);
        
            foreach (var passName in s_ShaderTags)
                m_ShaderTagIdList.Add(new ShaderTagId(passName));
        }

        public void Setup(RenderTextureDescriptor cameraDescriptor, RenderTargetHandle outlineRenderTarget,
                          ScriptableRenderer renderer, Material SettingsOutlineObjectDraw, Material material)
        {
            // configure render target
            m_OutlineRenderTarget = outlineRenderTarget;
            m_Renderer = renderer;
            m_OutlineRTDescriptor = new RenderTextureDescriptor(cameraDescriptor.width, cameraDescriptor.height, DistortionFeature.MapFormat, 0, 0);

            m_Material     = material;
            m_MaterialBlit = SettingsOutlineObjectDraw;
            
            m_TemporaryRenderTarget.Init("m_TemporaryRenderTarget");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // set render target to distortion map
            cmd.GetTemporaryRT(m_OutlineRenderTarget.id, m_OutlineRTDescriptor, m_MapFilterMode);
            ConfigureTarget(m_OutlineRenderTarget.Identifier(), m_Renderer.cameraDepthTarget);
            ConfigureClear(ClearFlag.Color, new Color(0, 0, 0, 0));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, SortingCriteria.CommonTransparent);
            drawingSettings.overrideMaterial = m_Material;
            drawingSettings.overrideMaterialPassIndex = 0;

            // profiling command
            var cmd = CommandBufferPool.Get(m_Tag);

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_OpaqueFilteringSettings, ref m_RenderStateBlock);
                //cmd.DrawRenderer();
                //m_Material.SetColor("Shape_Color");
                
                cmd.SetGlobalTexture("Outline_Shape", m_OutlineRenderTarget.Identifier());
                cmd.SetGlobalTexture("Outline_Source", renderingData.cameraData.renderer.cameraColorTarget);
                
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0;

                cmd.GetTemporaryRT(m_TemporaryRenderTarget.id, desc, FilterMode.Point);
                
                // cmd.SetRenderTarget(m_TemporaryColorTexture.Identifier());
                // cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_MaterialBlit);
                // cmd.SetViewProjectionMatrices(renderingData.cameraData.camera.worldToCameraMatrix, renderingData.cameraData.camera.projectionMatrix);

                cmd.Blit(m_OutlineRenderTarget.Identifier(), m_TemporaryRenderTarget.Identifier(), m_MaterialBlit);
                cmd.Blit(m_TemporaryRenderTarget.Identifier(),  renderingData.cameraData.renderer.cameraColorTarget);

                //cmd.Blit(m_OutlineRenderTarget.Identifier(), renderingData.cameraData.renderer.cameraColorTarget);
                // ScriptableRenderer.SetRenderTarget(cmd, destination, BuiltinRenderTextureType.CameraTarget, clearFlag, clearColor);
                // cmd.Blit(source, destination, material, passIndex);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_OutlineRenderTarget.id);
            cmd.ReleaseTemporaryRT(m_TemporaryRenderTarget.id);
        }
    }
}