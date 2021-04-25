using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CoreLib.Render
{
    sealed class DistortionFeature : ScriptableRendererFeature
    {
        public const FilterMode          MapFilterMode = FilterMode.Point;
        public const RenderTextureFormat MapFormat     = RenderTextureFormat.ARGBHalf;
        public const string              MapName       = "_DistortionMap";

        public DistortionMapRenderPass m_MapPass;
        public DistortionDrawPass      m_DrawPass;

        public  DistortionSettings m_Settings = new DistortionSettings();
        private RenderTargetHandle m_DistortionRenderTarget;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public class DistortionSettings
        {
            public LayerMask LayerMask;

            public RenderPassEvent RenderDistortion;
            public Material        Distortion;
        }

        //////////////////////////////////////////////////////////////////////////
        public override void Create()
        {
            m_DistortionRenderTarget.Init(MapName);
            m_MapPass = new DistortionMapRenderPass(RenderPassEvent.AfterRenderingOpaques, m_Settings.LayerMask, MapFilterMode, name);
            m_DrawPass = new DistortionDrawPass(m_Settings.RenderDistortion, m_Settings.Distortion, name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // map pass
            m_MapPass.Setup(renderingData.cameraData.cameraTargetDescriptor, m_DistortionRenderTarget, renderer);
            renderer.EnqueuePass(m_MapPass);

            // draw pass
            m_DrawPass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(m_DrawPass);
        }
    }
}