// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Distortion"
{
    Properties
    {
        [Toggle(GEOMETRY)]
        _Geometry("Geometry Only", Float) = 0
	    [NoScaleOffset] [Normal]
        _NormalTex ("Texture", 2D) = "" {}
        _Weight ("Weight", Range(-4, 4)) = 0.5
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

            #pragma multi_compile _ GEOMETRY

            #include "UnityCG.cginc"

            half _Weight;

#ifdef GEOMETRY
            struct appdata
            {
                float4 vertex : POSITION;
                half3  normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half3  normal : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal) * _Weight;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = half4(i.normal, 1.0f);
                return col;
            }
#else
            sampler2D _NormalTex;

            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 tspace0 : TEXCOORD1; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD2; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD3; // tangent.z, bitangent.z, normal.z
            };

            vertexOutput vert(appdata_tan input)
            {
                vertexOutput output;
                output.pos = UnityObjectToClipPos(input.vertex);
                output.uv = input.texcoord;

                half3 wNormal = UnityObjectToWorldNormal(input.normal);
                half3 wTangent = UnityObjectToWorldDir(input.tangent.xyz);
                half3 wBitangent = cross(wNormal, wTangent);
                // output the tangent space matrix
                output.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                output.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                output.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

                return output;
            }


            half4 frag(vertexOutput input) : SV_Target
            {
                half3 tnormal = UnpackNormal(tex2D(_NormalTex, input.uv));
                // transform normal from tangent to world space
                half3 worldNormal;
                worldNormal.x = dot(input.tspace0, tnormal);
                worldNormal.y = dot(input.tspace1, tnormal);
                worldNormal.z = dot(input.tspace2, tnormal);

                return half4((worldNormal), 1) * _Weight;
            }
#endif
            ENDCG
        }
    }
}
