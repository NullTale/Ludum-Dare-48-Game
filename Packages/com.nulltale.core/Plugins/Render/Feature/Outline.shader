Shader "Unlit/Outline"
{
	Properties
	{
		_Step("Step", Float) = 1.0
		_Intensivity("Intensivity", Float) = 1.0
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Always

			CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"
                #define count 2

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                sampler2D   Outline_Source;
                sampler2D   Outline_Shape;
                float       _Step;
                float       _Intensivity;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 outline = tex2D(Outline_Shape, i.uv);
                    fixed4 source = tex2D(Outline_Source, i.uv);
                    fixed4 blured = outline;

                    // step * texel size
                    float2 step = float2(_Step * (1.0 -_ScreenParams.z), _Step * (1.0 - _ScreenParams.w));

                    for (int n = 0; n < count; n++)
                    {
                        fixed4 left = tex2D(Outline_Shape, i.uv - fixed2(n * step.x, 0));
                        fixed4 right = tex2D(Outline_Shape, i.uv + fixed2(n * step.x, 0));
                        fixed4 up = tex2D(Outline_Shape, i.uv + fixed2(0, n * step.y));
                        fixed4 down = tex2D(Outline_Shape, i.uv - fixed2(0, n * step.y));

                        blured += (left + right + up + down);
                    }
                    blured /= count * 4 + 1;

                    return source + max(0, blured - outline) * _Intensivity;
                    // return outline;
                    // return blured;
                    // return _Color;
                }
                ENDCG
		}
	}
}