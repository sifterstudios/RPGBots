Shader "Fog Volume/RT viewers/RT_DepthR"
{
	Properties{
		_MainTex("Base", 2D) = "" {}
		_Intensity("Intensity", Range(1, 20)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma multi_compile _ _FOG_LOWRES_RENDERER 	
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			float _Intensity;
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

			sampler2D RT_DepthR, _MainTex;
			float4 RT_DepthR_ST, _MainTex_TexelSize, _MainTex_ST;
			sampler2D   _CameraDepthTexture;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, RT_DepthR);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float Depth = tex2D(RT_DepthR, i.uv).r;
				Depth *= 1000;
				return 1 / Depth*_Intensity;

			}
			ENDCG
		}
	}
}
