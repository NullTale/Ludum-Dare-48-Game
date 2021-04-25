using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CoreLib.Render
{
    sealed class OutlineFeature : ScriptableRendererFeature
    {
        public const FilterMode          MapFilterMode = FilterMode.Point;
        public const RenderTextureFormat MapFormat     = RenderTextureFormat.ARGB1555;
        public const string              MapName       = "_OutlineMap";

        public OutlineMapRenderPass m_MapPass;
        // public OutlineDrawPass      m_DrawPass;

        public  Settings m_Settings = new Settings();
        private RenderTargetHandle m_MapRenderTarget;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public class Settings
        {
            public LayerMask LayerMask;

            public RenderPassEvent RenderDistortion;
            public float           Scale;
            public Material        OutlineObjectDraw;
            public Material        OutlineDraw;
        }

        //////////////////////////////////////////////////////////////////////////
        public override void Create()
        {
            m_MapRenderTarget.Init(MapName);
            m_MapPass = new OutlineMapRenderPass(m_Settings.RenderDistortion, m_Settings.LayerMask, MapFilterMode, name);
            // m_DrawPass = new OutlineDrawPass(m_Settings.RenderDistortion, m_Settings.Distortion, name);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // map pass
            m_MapPass.Setup(renderingData.cameraData.cameraTargetDescriptor, m_MapRenderTarget, renderer, m_Settings.OutlineDraw, m_Settings.OutlineObjectDraw);
            renderer.EnqueuePass(m_MapPass);

            // // draw pass
            // m_DrawPass.Setup(renderer.cameraColorTarget);
            // renderer.EnqueuePass(m_DrawPass);
        }
    }
}