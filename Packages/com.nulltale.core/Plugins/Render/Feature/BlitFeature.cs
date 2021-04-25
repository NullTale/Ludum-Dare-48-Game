using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CoreLib.Render
{
    public class BlitFeature : ScriptableRendererFeature
    {
        public  BlitSettings       m_Settings = new BlitSettings();
        private RenderTargetHandle m_RenderTextureHandle;
        private BlitPass           m_BlitPass;

        //////////////////////////////////////////////////////////////////////////
        [Serializable]
        public class BlitSettings
        {
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

            public Material BlitMaterial          = null;
            public int      BlitMaterialPassIndex = -1;
            public Target   Destination           = Target.Color;
            public string   TextureId             = "_BlitPassTexture";
        }

        public enum Target
        {
            Color,
            Texture
        }

        //////////////////////////////////////////////////////////////////////////
        public override void Create()
        {
            var passIndex = m_Settings.BlitMaterial != null ? m_Settings.BlitMaterial.passCount - 1 : 1;
            m_Settings.BlitMaterialPassIndex = Mathf.Clamp(m_Settings.BlitMaterialPassIndex, -1, passIndex);
            m_BlitPass = new BlitPass(m_Settings.Event, m_Settings.BlitMaterial, m_Settings.BlitMaterialPassIndex, name);
            m_RenderTextureHandle.Init(m_Settings.TextureId);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src  = renderer.cameraColorTarget;
            var dest = (m_Settings.Destination == Target.Color) ? RenderTargetHandle.CameraTarget : m_RenderTextureHandle;

            if (m_Settings.BlitMaterial == null)
            {
                Debug.LogWarningFormat(
                    "Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.",
                    GetType().Name);
                return;
            }

            m_BlitPass.Setup(ref src, ref dest);
            renderer.EnqueuePass(m_BlitPass);
        }
    }
}