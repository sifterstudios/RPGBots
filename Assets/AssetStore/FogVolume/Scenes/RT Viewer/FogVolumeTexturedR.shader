Shader "Fog Volume/RT viewers/RT_FogVolumeR"
{
	Properties{
		_MainTex("Base", 2D) = "" {}
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
#define green float4(0, 1,0,1)
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

			sampler2D RT_FogVolumeR;
			float4 RT_FogVolumeR_ST;
			

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, RT_FogVolumeR);
			
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(RT_FogVolumeR, i.uv)/**green*/;

			return col;
			}
			ENDCG
		}
	}
}
