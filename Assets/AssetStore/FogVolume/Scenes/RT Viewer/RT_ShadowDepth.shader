Shader "Fog Volume/RT viewers/_ShadowTexture"
{
	Properties{
		_MainTex("Base", 2D) = "" {}
		_Divide("Divide", Range(1, 30000))=1000
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

			sampler2D _ShadowTexture;
			float4 _ShadowTexture_ST;
			fixed _Divide;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _ShadowTexture);
			
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_ShadowTexture, i.uv).r/ _Divide;
			fixed4 col = tex2D(_ShadowTexture, i.uv);
			col.r = col.r / _Divide;
			return col.r*col.g;
			}
			ENDCG
		}
	}
}
