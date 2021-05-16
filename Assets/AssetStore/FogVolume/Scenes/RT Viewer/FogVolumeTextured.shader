Shader "Fog Volume/RT viewers/RT_FogVolume"
{
	Properties{
	[Toggle(RightEye)] _RightEye ("Right Eye?", Float) = 0	
		[hideininspector] _MainTex("Base", 2D) = "" {}
		
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
			#pragma multi_compile _ RightEye
			#include "UnityCG.cginc"
#define red float4(1, 0,0,1)
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

			sampler2D RT_FogVolume;
			sampler2D RT_FogVolumeR;
			float4 RT_FogVolume_ST;
			

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, RT_FogVolume);
			
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = 0;
				#ifdef RightEye
				col = tex2D(RT_FogVolumeR, i.uv)/**red*/;
				#else
				col = tex2D(RT_FogVolume, i.uv)/**red*/;
				#endif

			return col;
			}
			ENDCG
		}
	}
}
