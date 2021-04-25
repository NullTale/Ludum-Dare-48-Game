Shader "Unlit/BlitTest"
{
    Properties
    {
	    [HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}
        _Weight ("Weight", Range(-1.5, 1.5)) = 0.1
	    [Toggle(DEPTH_IMPACT)]
        DepthImpact("Depth Impact", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ DEPTH_IMPACT

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D   _MainTex;
            sampler2D   _DistortionMap;
            sampler2D   _CameraDepthTexture;
            float       _Weight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 distortion = tex2D(_DistortionMap, i.uv).xy * _Weight;

#ifdef DEPTH_IMPACT
                distortion *= tex2D(_CameraDepthTexture, i.uv).xx;
#endif
                fixed4 col = tex2D(_MainTex, i.uv + distortion);
                return col;
            }
            ENDCG
        }
    }
}
