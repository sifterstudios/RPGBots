Shader "Fog Volume/RT viewers/_CameraDepthTexture"
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

			sampler2D _CameraDepthTexture;
			float4 _MainTex_ST;
			

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_CameraDepthTexture, i.uv).r;

				return col*_Intensity;
			}
			ENDCG
		}
	}
}
